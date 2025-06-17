using System.IO;
using System.Collections.Generic;
using VRage.Utils;

namespace SEENG_Core
{
    public class Loader
    {
        public List<WorkshopMod> WorkshopMods { get; } = new List<WorkshopMod>();

        public void PopulateWorkshopMods()
        {
            string workshopPath = Path.GetFullPath(@"..\..\..\workshop\content\244850\");
            if (!Directory.Exists(workshopPath))
            {
                MyLog.Default.WriteLine("Workshop path not found: " + workshopPath);
                return;
            }

            foreach (string modDir in Directory.GetDirectories(workshopPath))
            {
                string modDataPath = Path.Combine(modDir, "Data");
                if (!Directory.Exists(modDataPath))
                    continue;

                string seengmFile = Path.Combine(modDataPath, "SEENGM.txt");
                if (File.Exists(seengmFile))
                {
                    string modDisplayName = File.ReadAllText(seengmFile).Trim();
                    if (string.IsNullOrWhiteSpace(modDisplayName))
                        modDisplayName = Path.GetFileName(modDir);

                    string audioSbcPath = Path.Combine(modDataPath, "seeng_engine.sbc"); // база
                    if (File.Exists(audioSbcPath) || 
                        File.Exists(Path.Combine(modDataPath, "seeng_manuvering.sbc")) ||
                        File.Exists(Path.Combine(modDataPath, "seeng_systems.sbc")) ||
                        File.Exists(Path.Combine(modDataPath, "ShipSoundGroups.sbc")) ||
                        File.Exists(Path.Combine(modDataPath, "seeng_thrusters.sbc")))
                    {
                        string arcShipSysPath = Path.Combine(modDir, "AUDIO_SEENG"); 
                        WorkshopMods.Add(new WorkshopMod(modDisplayName, modDir, arcShipSysPath, audioSbcPath));
                    }
                }
            }
        }
    }
}