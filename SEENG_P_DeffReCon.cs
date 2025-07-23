using Sandbox.Game;
using System.IO;
using System.Collections.Generic;
using VRage.Game;
using VRage.Utils;
using VRage.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using System;
using VRage.Game.ModAPI;
using VRage;
using VRageMath;
using Sandbox.Game.Entities;
using Sandbox.Common.ObjectBuilders;
using System.Linq;
using VRage.Game.Entity;
using VRage.ModAPI;

namespace SEENG_Core
{
    public class DefRecCon
    {
        public void PreloadWavFiles(string gameContentPath, List<WorkshopMod> workshopMods)
        {
            string gameAudioPath = Path.Combine(gameContentPath, "Audio");
            Directory.CreateDirectory(gameAudioPath);

            foreach (var mod in workshopMods)
            {
                if (Directory.Exists(mod.ArcShipSysPath))
                {
                    foreach (string wavFile in Directory.GetFiles(mod.ArcShipSysPath, "*.wav", SearchOption.AllDirectories))
                    {
                        try
                        {
                            string fileName = Path.GetFileName(wavFile);
                            string destPath = Path.Combine(gameAudioPath, fileName);
                            File.Copy(wavFile, destPath, true);
                            MyLog.Default.WriteLine($"SEENGCore: Copied WAV file: {fileName} to {destPath}");
                        }
                        catch (Exception e)
                        {
                            MyLog.Default.WriteLine($"SEENGCore: Failed to copy WAV file {wavFile}: {e.Message}");
                        }
                    }
                }
            }
        }

        public void ReloadDefinitions(WorkshopMod currentMod, bool respawnGrids = false, bool respawnPlanets = false)
        {
            try
            {
                // mod lsit
                var modsToLoad = new List<MyObjectBuilder_Checkpoint.ModItem>();
                foreach (var mod in MyAPIGateway.Session.Mods)
                {
                    if (mod.Name != "Audio_shipSounds.sbc")
                    {
                        modsToLoad.Add(mod);
                    }
                }

                if (currentMod != null)
                {
                    modsToLoad.Add(new MyObjectBuilder_Checkpoint.ModItem
                    {
                        Name = currentMod.ModPath,
                        PublishedFileId = 0
                    });
                    MyLog.Default.WriteLine($"SEENGCore: Added mod to load: {currentMod.ModPath}");
                }

                // old defs
                foreach (MyDefinitionBase def in MyDefinitionManager.Static.GetAllDefinitions())
                {
                    if (def.DisplayNameEnum.HasValue)
                    {
                        def.DisplayNameString = "(OLD) " + MyTexts.GetString(def.DisplayNameEnum.Value);
                        def.DisplayNameEnum = null;
                    }
                    else if (!string.IsNullOrEmpty(def.DisplayNameString))
                    {
                        def.DisplayNameString = "(OLD) " + def.DisplayNameString;
                    }

                    var blockDef = def as MyCubeBlockDefinition;
                    if (blockDef != null)
                    {
                        blockDef.BlockVariantsGroup = null; //MyToolbarItemCubeBlock
                    }
                }

                //  ENV
                MyEnvironmentDefinition oldEnvDef = MyDefinitionManager.Static.EnvironmentDefinition;
                MyDefinitionErrors.Add(null, "Definitions SBCs reloaded, SBC errors earlier than this can be ignored.", TErrorSeverity.Warning, true);

                // RELOAD
                MyDefinitionManager.Static.UnloadData();
                MyDefinitionManager.Static.LoadData(modsToLoad);

                // 6. ENV2
                MyEnvironmentDefinition newEnvDef = MyDefinitionManager.Static.GetDefinition<MyEnvironmentDefinition>(MyStringHash.GetOrCompute("Default"));
                if (newEnvDef != null && oldEnvDef != null)
                {
                    oldEnvDef.Merge(newEnvDef);
                }               
                else
                {
                    MyAPIGateway.Utilities.ShowMessage("SEENGCore", "Engine refited!");
                }

                // hud + snd
                FixSoundVolume();
                RefreshHudDefinition();
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"SEENGCore: Failed to reload definitions: {e.Message}");
                MyAPIGateway.Utilities.ShowMessage("SEENGCore", "Failed to reload SBC.");
            }
        }

        private void FixSoundVolume()
        {
            try
            {
                float gameVolume = 1f;
                float musicVolume = 1f;
                float voiceChatVolume = 1f;

                IMyConfig userCfg = MyAPIGateway.Session?.Config;
                if (userCfg != null)
                {
                    gameVolume = userCfg.GameVolume;
                    musicVolume = userCfg.MusicVolume;
                    voiceChatVolume = userCfg.VoiceChatVolume;
                }

                MyVisualScriptLogicProvider.SetVolumeLocal(gameVolume, musicVolume, voiceChatVolume);
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"SEENGCore: Failed to reset sound volume: {e.Message}");
                MyAPIGateway.Utilities.ShowMessage("SEENGCore", "ERROR: Failed to reset sound volume. Try to re-enter cockpit.");
            }
        }

        private void RefreshHudDefinition()
        {
            try
            {
                var activeCockpit = MyAPIGateway.Session?.ControlledObject as MyCockpit;
                if (activeCockpit != null)
                {
                    HudRefresh(activeCockpit);
                    return;
                }

                string charHud = null;
                IMyCharacter character = MyAPIGateway.Session?.Player?.Character as IMyCharacter;
                if (character != null)
                {
                    MyCharacterDefinition charDef = (MyCharacterDefinition)character.Definition;
                    charHud = charDef.HUD;
                }

                foreach (MyEntity ent in MyEntities.GetEntities())
                {
                    var existingGrid = ent as MyCubeGrid;
                    if (existingGrid == null)
                        continue;

                    foreach (MyCubeBlock block in existingGrid.GetFatBlocks())
                    {
                        var foundCockpit = block as MyCockpit;
                        if (foundCockpit == null)
                            continue;

                        HudRefresh(foundCockpit, charHud);
                        return;
                    }
                }

                MyCockpitDefinition anyCockpitDef = MyDefinitionManager.Static.GetAllDefinitions().FirstOrDefault(d => d is MyCockpitDefinition) as MyCockpitDefinition;
                if (anyCockpitDef == null)
                    throw new Exception("No cockpit blocks exist in your world!");

                MyObjectBuilder_CubeGrid tempGridObj = new MyObjectBuilder_CubeGrid()
                {
                    CreatePhysics = false,
                    GridSizeEnum = MyCubeSize.Large,
                    PositionAndOrientation = new MyPositionAndOrientation(Vector3D.Zero, Vector3.Forward, Vector3.Up),
                    PersistentFlags = MyPersistentEntityFlags2.InScene,
                    IsStatic = true,
                    Editable = false,
                    DestructibleBlocks = false,
                    IsRespawnGrid = false,
                    Name = "TemporaryGrid",
                };

                tempGridObj.CubeBlocks.Add(new MyObjectBuilder_Cockpit()
                {
                    SubtypeName = anyCockpitDef.Id.SubtypeName,
                });

                MyAPIGateway.Entities.RemapObjectBuilder(tempGridObj);

                var tempGrid = (MyCubeGrid)MyAPIGateway.Entities.CreateFromObjectBuilder(tempGridObj);
                tempGrid.IsPreview = true;
                tempGrid.Save = false;
                tempGrid.Flags = EntityFlags.None;
                tempGrid.Render.Visible = false;
                MyAPIGateway.Entities.AddEntity(tempGrid);

                var tempCockpit = (MyCockpit)tempGrid.GetFatBlocks()[0];
                HudRefresh(tempCockpit, charHud);

                tempGrid.Close();
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"SEENGCore: Failed to refresh HUD definition: {e.Message}");
                MyAPIGateway.Utilities.ShowMessage("SEENGCore", "ERROR: Failed to refresh HUD. Re-enter a cockpit to fix.");
            }
        }

        private void HudRefresh(MyCockpit cockpit, string overrideHud = null)
        {
            FakeCockpit fakeCockpit = new FakeCockpit { SlimBlock = cockpit.SlimBlock };

            if (overrideHud != null)
            {
                string originalHud = fakeCockpit.BlockDefinition.HUD;
                fakeCockpit.BlockDefinition.HUD = overrideHud;
                fakeCockpit.OnAssumeControl(null);
                fakeCockpit.BlockDefinition.HUD = originalHud;
            }
            else
            {
                fakeCockpit.OnAssumeControl(null);
            }

            fakeCockpit.SlimBlock = null;
        }

        private class FakeCockpit : MyCockpit
        {
            protected override void UpdateCameraAfterChange(bool resetHeadLocalAngle = true)
            {
                //OnAssumeControl
            }
        }
    }
}