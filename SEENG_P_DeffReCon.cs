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
        private readonly List<(List<string> files, long cleanupFrame)> _pendingCleanups = new List<(List<string>, long)>();
        public void Update()
        {
            long currentFrame = MyAPIGateway.Session.GameplayFrameCounter;
            for (int i = _pendingCleanups.Count - 1; i >= 0; i--)
            {
                var (files, cleanupFrame) = _pendingCleanups[i];
                if (currentFrame >= cleanupFrame)
                {
                    MyLog.Default.WriteLine($"SEENGCore: Files loaded into memory.");
                    foreach (string wavFile in files)
                    {
                        try
                        {
                            if (File.Exists(wavFile))
                            {
                                File.Delete(wavFile);
                                MyLog.Default.WriteLine($"SEENGCore: WAV nomore: {wavFile}");
                            }
                            else
                            {
                                MyLog.Default.WriteLine($"SEENGCore: WAV file not: {wavFile}");
                            }
                        }
                        catch (Exception e)
                        {
                            MyLog.Default.WriteLine($"SEENGCore: Fail to {wavFile}: {e.Message}");
                        }
                    }
                    MyLog.Default.WriteLine("SEENGCore: WAV file completed.");
                    _pendingCleanups.RemoveAt(i);
                }
            }
        }

        public void ReloadDefinitions(WorkshopMod currentMod, string gameContentPath)
        {
            try
            {
                List<string> copiedWavFiles = new List<string>();
                if (currentMod != null)
                {
                    string gameAudioPath = Path.Combine(gameContentPath, "Audio");
                    Directory.CreateDirectory(gameAudioPath);

                    if (!string.IsNullOrEmpty(currentMod.ArcShipSysPath) && Directory.Exists(currentMod.ArcShipSysPath))
                    {
                        foreach (string wavFile in Directory.GetFiles(currentMod.ArcShipSysPath, "*.wav", SearchOption.AllDirectories))
                        {
                            try
                            {
                                string fileName = Path.GetFileName(wavFile);
                                string destPath = Path.Combine(gameAudioPath, fileName);
                                File.Copy(wavFile, destPath, true);
                                copiedWavFiles.Add(destPath); 
                            }
                            catch (Exception e)
                            {
                                MyLog.Default.WriteLine($"SEENGCore: Failed to WAV{wavFile}: {e.Message}");
                            }
                        }
                    }
                    else
                    {
                        MyLog.Default.WriteLine($"SEENGCore: Invalid or missing ArcShipSysPath for mod: {currentMod.ModPath}");
                    }
                }
                else
                {
                    MyLog.Default.WriteLine("SEENGCore: No mod selected for WAV file.");
                }

                // modlist 
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
                        blockDef.BlockVariantsGroup = null;
                    }
                }

                // EVB
                MyEnvironmentDefinition oldEnvDef = MyDefinitionManager.Static.EnvironmentDefinition;
                MyDefinitionErrors.Add(null, "SBC errors earlier than this can be ignored.", TErrorSeverity.Warning, true);

                // Reload
                MyDefinitionManager.Static.UnloadData();
                MyDefinitionManager.Static.LoadData(modsToLoad);
                if (copiedWavFiles.Count > 0)
                {
                    long cleanupFrame = MyAPIGateway.Session.GameplayFrameCounter + 300; 
                    _pendingCleanups.Add((copiedWavFiles, cleanupFrame));                   
                }

                // ENV2
                MyEnvironmentDefinition newEnvDef = MyDefinitionManager.Static.GetDefinition<MyEnvironmentDefinition>(MyStringHash.GetOrCompute("Default"));
                if (newEnvDef != null && oldEnvDef != null)
                {
                    oldEnvDef.Merge(newEnvDef);
                }
                MyAPIGateway.Utilities.ShowMessage("SEENG", "Engine refited!");

                // snd n hud fix
                FixSoundVolume();
                RefreshHudDefinition();
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"SEENGCore: Failed to reload definitions: {e.Message}");
                MyAPIGateway.Utilities.ShowMessage("SEENGCore", "Failed to refit engine. Check log for details.");
            }
        }
        private void FixSoundVolume()
        {
            try
            {
                var userCfg = MyAPIGateway.Session?.Config;
                if (userCfg != null)
                {
                    MyVisualScriptLogicProvider.SetVolumeLocal(
                        userCfg.GameVolume,
                        userCfg.MusicVolume,
                        userCfg.VoiceChatVolume);
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"SEENGCore: Failed to reset sound volume: {e.Message}");
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
                // OnAssumeControl
            }
        }
    }
}