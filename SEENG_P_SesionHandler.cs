using Sandbox.Game;
using VRage.Game.Components;
using VRage.Utils;
using System.IO;
using VRage.Game.ModAPI;
using Sandbox.Graphics.GUI;
using Sandbox.ModAPI;
using System;
using VRage.Game;

namespace SEENG_Core
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)] 
    public class SessionHandler : MySessionComponentBase
    {
        private bool _isInitialized = false;
        private string _gameContentPath;
        private WorkshopMod _currentMod = null;
        private readonly Loader _loader = new Loader();
        private readonly DefRecCon _defRecCon = new DefRecCon();

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            _gameContentPath = Path.Combine(Path.GetDirectoryName(MyAPIGateway.Utilities.GamePaths.ContentPath), "Content");
            MyLog.Default.WriteLine($"SEENGCore: Game Content Path set to: {_gameContentPath}");
            MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
            _loader.PopulateWorkshopMods();
            _isInitialized = true;
        }

        public override void UpdateAfterSimulation()
        {
            if (_isInitialized)
            {
                _defRecCon.Update(); 
            }
        }

        private void OnMessageEntered(string messageText, ref bool sendToOthers)
        {
            if (messageText.Equals("/seeng", StringComparison.OrdinalIgnoreCase))
            {
                sendToOthers = false;
                OpenMenu();
            }
        }

        private void OpenMenu()
        {
            MyGuiSandbox.AddScreen(new MyGuiScreenSEENGCoreMenu(_loader.WorkshopMods, OnModSelected, _loader));
        }

        private void OnModSelected(WorkshopMod selectedMod)
        {
            _currentMod = selectedMod;
            ModManager.UpdateMod(_currentMod);
            if (string.IsNullOrEmpty(_gameContentPath))
            {
                MyLog.Default.WriteLine("SEENGCore: Error: Game content path is null or empty.");
                MyAPIGateway.Utilities.ShowMessage("SEENGCore", "Error: Cannot access game content path.");
                return;
            }
            MyLog.Default.WriteLine($"SEENGCore: Calling ReloadDefinitions for mod: {_currentMod?.ModPath ?? "None"}");
            _defRecCon.ReloadDefinitions(_currentMod, _gameContentPath); 
        }

        protected override void UnloadData()
        {
            MyAPIGateway.Utilities.MessageEntered -= OnMessageEntered;
            _currentMod = null;
            ModManager.UpdateMod(null);

            // Резервная очистка .wav файлов при выгрузке
            string gameAudioPath = Path.Combine(_gameContentPath, "Audio");
            if (Directory.Exists(gameAudioPath))
            {
                foreach (string wavFile in Directory.GetFiles(gameAudioPath, "*.wav"))
                {
                    try
                    {
                        File.Delete(wavFile);
                        MyLog.Default.WriteLine($"SEENGCore: Cleaned up WAV file on unload: {wavFile}");
                    }
                    catch (Exception e)
                    {
                        MyLog.Default.WriteLine($"SEENGCore: Failed to clean up WAV file on unload {wavFile}: {e.Message}");
                    }
                }
            }

            base.UnloadData();
        }
    }
}