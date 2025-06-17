using Sandbox.Game;
using VRage.Game.Components;
using VRage.Utils;
using System;
using System.IO;
using System.Xml.Linq;
using Sandbox.ModAPI;
using VRage.Game;

namespace SEENG_Core
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class TemporaryKostil : MySessionComponentBase
    {
        private bool _isInitialized = false;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
            _isInitialized = true;
        }

        private void OnMessageEntered(string messageText, ref bool sendToOthers)
        {
            if (messageText.StartsWith("/seeng speed ", StringComparison.OrdinalIgnoreCase))
            {
                sendToOthers = false;
                string[] parts = messageText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 3)
                {
                    if (float.TryParse(parts[2], out float newSpeed) && newSpeed >= 0)
                    {
                        WorkshopMod lastMod = ModManager.LastSelectedMod;
                        if (lastMod != null && ApplyCustomFullSpeed(lastMod, newSpeed))
                        {
                            MyAPIGateway.Utilities.ShowMessage("SEENGCore", $"FullSpeed changed to {newSpeed} for addon '{lastMod.Name}'. Please press 'Refit Engine' again to apply.");
                        }
                        else
                        {
                            MyAPIGateway.Utilities.ShowMessage("SEENGCore", "No addon selected or failed to apply speed.");
                        }
                    } 
                    else
                    {
                        MyAPIGateway.Utilities.ShowMessage("SEENGCore", "Invalid speed value. Use /seeng speed {number}");
                    }
                }
                else
                {
                    MyAPIGateway.Utilities.ShowMessage("SEENGCore", "Usage: /seeng speed {number}");
                }
            }
        }

        private bool ApplyCustomFullSpeed(WorkshopMod mod, float newSpeed)
        {
            try
            {
                string shipSoundGroupsPath = Path.Combine(mod.ModPath, "Data", "ShipSoundGroups.sbc");
                if (!File.Exists(shipSoundGroupsPath))
                {
                    MyLog.Default.WriteLine($"SEENGCore: ShipSoundGroups.sbc not found at {shipSoundGroupsPath}");
                    return false;
                }

                // xxmmll
                XDocument doc = XDocument.Load(shipSoundGroupsPath);
                bool modified = false;
                foreach (var element in doc.Descendants("FullSpeed"))
                {
                    element.Value = newSpeed.ToString();
                    modified = true;
                }

                if (modified)
                {
                    doc.Save(shipSoundGroupsPath);
                    MyLog.Default.WriteLine($"SEENGCore: Updated FullSpeed to {newSpeed} in {shipSoundGroupsPath}");
                    return true;
                }
                else
                {
                    MyLog.Default.WriteLine($"SEENGCore: No <FullSpeed> tag found in {shipSoundGroupsPath}");
                    return false;
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"SEENGCore: Failed to apply custom FullSpeed: {e.Message}");
                return false;
            }
        }

        protected override void UnloadData()
        {
            MyAPIGateway.Utilities.MessageEntered -= OnMessageEntered;
            base.UnloadData();
        }
    }
}