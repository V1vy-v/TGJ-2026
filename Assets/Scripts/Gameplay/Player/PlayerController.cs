using Framework.Bootstrap;
using Framework.Services.Input;
using Gameplay.Player.Act;
using Gameplay.Player.Input;
using Unity.Cinemachine;
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
        private PlayerCameraRig _cameraRig;
        public PerformanceMode PerformanceMode => _performanceMode;
        
        private PlayerMotor _motor;
        [SerializeField] private PlayerConfig playerConfig;
        [SerializeField] private CharacterController controller;
        [SerializeField] CinemachineOrbitalFollow orbitalFollow;
        [SerializeField] Camera mainCamera;
        [SerializeField] float lookSensitivity = 0.15f;
        [SerializeField] bool invertY = false;
        #endregion

        #region 生命周期

        private void Awake()
        {
            var input = GameRoot.Instance.Services.Get<IInputService>();

            _inputMode = new InputModeController(input);
            _inputReader = new PlayerInputReader(input.GetMap("Player"));
            _performanceMode = new PerformanceMode(_inputMode, input.GetMap("Performance"));

            _cameraRig = new PlayerCameraRig(_inputReader, orbitalFollow, mainCamera.transform, lookSensitivity, invertY);
            _motor = new PlayerMotor(controller, _inputReader, playerConfig, mainCamera.transform);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            _inputReader.Poll();
            _cameraRig.Tick();
            _motor.Tick(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            
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
