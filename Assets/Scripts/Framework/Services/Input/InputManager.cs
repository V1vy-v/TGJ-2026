using Framework.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Framework.Services.Input
{
    public class InputManager : IManager, IInputService
    {
        #region 状态

        public int Order { get; } = 10;

        private readonly InputActionAsset _asset;

        #endregion

        #region 生命周期

        public InputManager(InputActionAsset asset)
        {
            _asset = asset;

            if (_asset == null)
                Debug.LogError("[InputManager] InputActionAsset 为空");
        }

        public void Initialize(ServiceRegistry services, MonoBehaviour host)
        {
            if (services == null)
            {
                Debug.LogError("[InputManager] services 为空");
                return;
            }

            if (_asset == null)
                return;

            services.Register<IInputService>(this);
        }

        public void Shutdown()
        {
            DisableAll();
        }

        #endregion

        #region 对外接口

        public InputActionMap GetMap(string mapName)
        {
            if (_asset == null)
                throw new System.InvalidOperationException("[InputManager] 未配置 InputActionAsset");

            return _asset.FindActionMap(mapName, throwIfNotFound: true);
        }

        public void DisableAll()
        {
            if (_asset == null)
                return;

            _asset.Disable();
        }

        #endregion
    }
}
