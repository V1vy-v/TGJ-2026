using Framework.Bootstrap;
using Framework.Services.Input;
using Gameplay.Player.Act;
using Gameplay.Player.Input;
using UnityEngine;

namespace Gameplay.Player
{
    /// <summary>
    /// 主角的组装根：持有输入模式、输入读取和演奏模式，协调各子系统。
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        #region 状态

        private PlayerInputReader _inputReader;
        private PerformanceMode _performanceMode;
        private InputModeController _inputMode;
        public PerformanceMode PerformanceMode => _performanceMode;
        
        private PlayerMotor  _playerMotor;
        [SerializeField] private PlayerConfig playerConfig;

        #endregion

        #region 生命周期

        private void Awake()
        {
            var input = GameRoot.Instance.Services.Get<IInputService>();

            _inputMode = new InputModeController(input);
            _inputReader = new PlayerInputReader(input.GetMap("Player"));
            _performanceMode = new PerformanceMode(_inputMode, input.GetMap("Performance"));
            
            _playerMotor = new PlayerMotor(GetComponent<Rigidbody>(), _inputReader, playerConfig);
        }

        private void Update()
        {
            _inputReader.Poll();
            _playerMotor.Tick();
        }

        private void OnDestroy()
        {
            _performanceMode?.Dispose();
            _inputReader?.Dispose();
            _inputMode?.Dispose();
        }

        #endregion
    }
}
