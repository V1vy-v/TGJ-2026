using System;
using System.Collections.Generic;
using Framework.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Framework.Services.Input
{
    public class InputManager : IManager, IInputService
    {
        #region 状态

        public int Order { get; } = 10;
        public InputActionAsset InputAsset { get; }
        private readonly Dictionary<string, int> _mapRefCounts = new();

        #endregion

        #region 生命周期

        public InputManager(InputActionAsset asset)
        {
            if(asset == null)
            {
                Debug.LogError($"[InputManager]: {nameof(asset)} is null");
                return;
            }
            InputAsset = asset;
        }
        public void Initialize(ServiceRegistry services, MonoBehaviour host)
        {
            if (services == null)
            {
                Debug.LogError($"[InputManager]: {nameof(services)} is null");
                return;
            }
            
            services.Register<IInputService>(this);
        }

        public void Shutdown()
        {
            InputAsset.Disable();
            _mapRefCounts.Clear();
        }
        
        #endregion

        #region 对外接口

        public IDisposable EnableMap(string mapName)
        {
            var map = InputAsset.FindActionMap(mapName, throwIfNotFound: true);

            _mapRefCounts.TryGetValue(mapName, out var count);
            if (count == 0)
                map.Enable();                    // 第一个占用者才真正打开

            _mapRefCounts[mapName] = count + 1;
            return new MapScope(this, mapName);  // 返回“占用凭证”
        }
        
        public bool IsMapEnabled(string mapName)
        {
            if (!_mapRefCounts.TryGetValue(mapName, out var reference))
                return false;
            return reference > 0;
        }

        public InputAction GetAction(string mapName, string actionName)
        {
            return InputAsset.FindAction($"{mapName}/{actionName}", throwIfNotFound: true);
        }
        #endregion

        #region 内部实现
        
        internal void ReleaseMap(string mapName)
        {
            if (!_mapRefCounts.TryGetValue(mapName, out var count)) return;

            if (count <= 1)
            {
                _mapRefCounts.Remove(mapName);
                InputAsset.FindActionMap(mapName, throwIfNotFound: true).Disable();
            }
            else
            {
                _mapRefCounts[mapName] = count - 1;
            }
        }

        #endregion
    }
}
