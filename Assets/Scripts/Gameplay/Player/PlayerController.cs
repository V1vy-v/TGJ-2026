using Framework.Bootstrap;
using Framework.Services.Input;
using Gameplay.Player.Act;
using Gameplay.Player.Input;
using Gameplay.Player.Interaction;
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
        private PlayerInteractor _interactor;
        public PerformanceMode PerformanceMode => _performanceMode;
        
        private PlayerMotor _motor;
        [SerializeField] private PlayerConfig playerConfig;
        [SerializeField] private CharacterController controller;
        [SerializeField] CinemachineOrbitalFollow orbitalFollow;
        [SerializeField] Camera mainCamera;
        [SerializeField] float lookSensitivity = 0.15f;
        [SerializeField] bool invertY = false;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private float interactRadius = 1f;
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
            var context = new PlayerContext(transform);
            _interactor = new PlayerInteractor(transform, context, interactableLayer, interactRadius);
            
            _inputReader.InteractPressed += OnInteractPressed;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }


        private void Update()
        {
            _inputReader.Poll();
            _cameraRig.Tick();
            _motor.Tick(Time.deltaTime);
            _interactor.Tick();
        }
        
        private void OnDestroy()
        {
            _inputReader.InteractPressed -= OnInteractPressed;

            _performanceMode?.Dispose();
            _inputReader?.Dispose();
            _inputMode?.Dispose();
        }

        #endregion
        
        private void OnInteractPressed()
        {
            _interactor.TryInteract();
        }
    }
}
