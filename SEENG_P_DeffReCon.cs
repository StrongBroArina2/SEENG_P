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

        public void ReloadDefinitions(WorkshopMod currentMod)
        {
            try
            {
                // modlist
                var modsToLoad = new List<MyObjectBuilder_Checkpoint.ModItem>();

                // sss
                foreach (var mod in MyAPIGateway.Session.Mods)
                {
                    if (mod.Name != "Audio_shipSounds.sbc")
                    {
                        modsToLoad.Add(mod);
                    }
                }

                // modadd
                if (currentMod != null)
                {
                    modsToLoad.Add(new MyObjectBuilder_Checkpoint.ModItem
                    {
                        Name = currentMod.ModPath,
                        PublishedFileId = 0
                    });
                    MyLog.Default.WriteLine($"SEENGCore: Added mod to load: {currentMod.ModPath}");
                }

                // mmmm skybox loader, mu beloved, if only...
                MyDefinitionManager.Static.UnloadData();
                MyDefinitionManager.Static.LoadData(modsToLoad);

                // ma ears!
                FixSoundVolume();
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"SEENGCore: Failed to reload definitions: {e.Message}");
                MyAPIGateway.Utilities.ShowMessage("SEENGCore", "Failed to reload SBCs.");
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
                MyAPIGateway.Utilities.ShowMessage("SEENGCore", "ERROR: Failed to reset sound volume.");
            }
        }
    }
}