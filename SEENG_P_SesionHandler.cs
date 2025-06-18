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
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
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
            _defRecCon.PreloadWavFiles(_gameContentPath, _loader.WorkshopMods);
            _isInitialized = true;
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
            ModManager.UpdateMod(_currentMod); // мэнаджар бургераф
            _defRecCon.ReloadDefinitions(_currentMod); // ориг
        }

        protected override void UnloadData()
        {
            MyAPIGateway.Utilities.MessageEntered -= OnMessageEntered;
            _currentMod = null;
            ModManager.UpdateMod(null); // состояние
            base.UnloadData();
        }
    }
}