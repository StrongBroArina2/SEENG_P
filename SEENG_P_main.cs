using Sandbox.Graphics.GUI;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Plugins;

namespace SEENG_Core
{
    public class PluginMain : IPlugin
    {
        private SessionHandler _sessionHandler;
        private bool _isComponentRegistered = false;

        public void Init(object gameInstance)
        {
            _sessionHandler = new SessionHandler();
        }

        public void Dispose()
        {
            if (MyAPIGateway.Session != null && _sessionHandler != null && _isComponentRegistered)
            {
                MyAPIGateway.Session.UnregisterComponent(_sessionHandler);
            }
            _sessionHandler = null;
        }

        public void Update()
        {
            if (!_isComponentRegistered && MyAPIGateway.Session != null)
            {
                MyAPIGateway.Session.RegisterComponent(_sessionHandler, MyUpdateOrder.BeforeSimulation, 0);
                _isComponentRegistered = true;
            }
        }
    }
}