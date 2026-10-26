using System;
using Framework.Services.Input;
using UnityEngine.InputSystem;

namespace Gameplay.Player.Input
{
    public enum InputMode
    {
        None,
        Player,
        NpcPerformance,
    }

    /// <summary>
    /// 只负责“当前哪张 map 生效”。同一时间只启用一张，不持有任何 Reader。
    /// </summary>
    public sealed class InputModeController : IDisposable
    {
        #region 状态

        private readonly InputActionMap _playerMap;
        private readonly InputActionMap _npcPerformanceMap;

        private InputActionMap _currentMap;
        private bool _disposed;

        public InputMode CurrentMode { get; private set; } = InputMode.None;

        public event Action<InputMode> ModeChanged;

        #endregion

        #region 生命周期

        public InputModeController(IInputService inputService)
        {
            if (inputService == null)
                throw new ArgumentNullException(nameof(inputService));

            _playerMap = inputService.GetMap("Player");
            _npcPerformanceMap = inputService.GetMap("Performance");

            SwitchTo(InputMode.Player);
        }

        #endregion

        #region 对外接口

        public void SwitchTo(InputMode mode)
        {
            if (_disposed || mode == CurrentMode)
                return;

            _currentMap?.Disable();

            _currentMap = mode switch
            {
                InputMode.Player => _playerMap,
                InputMode.NpcPerformance => _npcPerformanceMap,
                _ => null,
            };

            _currentMap?.Enable();
            CurrentMode = mode;

            ModeChanged?.Invoke(mode);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _currentMap?.Disable();
            _currentMap = null;
            CurrentMode = InputMode.None;
        }

        #endregion
    }
}
