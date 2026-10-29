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
    /// 主角的组装根：持有输入、移动、相机、交互、眼镜与视野。
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        #region 状态

        private PlayerInputReader _inputReader;
        private PerformanceMode _performanceMode;
        private InputModeController _inputMode;
        private PlayerCameraRig _cameraRig;
        private PlayerInteractor _interactor;
        private PlayerMotor _motor;
        private PlayerGlasses _glasses;
        private PlayerVision _vision;
        
        public PerformanceMode PerformanceMode => _performanceMode;

        [SerializeField] private PlayerConfig playerConfig;
        [SerializeField] private CharacterController controller;
        [SerializeField] private CinemachineOrbitalFollow orbitalFollow;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float lookSensitivity = 0.15f;
        [SerializeField] private bool invertY;

        [Header("交互")]
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private float interactRadius = 1f;

        [Header("视野")]
        [SerializeField] private Light selfLight;
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private VisionSettings visionSettings = new VisionSettings
        {
            normalFov = 60f,
            blindFov = 40f,
            normalRadius = 2.5f,
            blindRadius = 1.2f,
            normalLightIntensity = 0f,
            blindLightIntensity = 1.5f,
            normalLightRange = 0.1f,
            blindLightRange = 2f,
            normalAmbient = 1f,
            blindAmbient = 0.05f,
            normalFogDensity = 0.01f,
            blindFogDensity = 0.15f,
            transitionDuration = 0.3f,
        };

        #endregion

        #region 生命周期

        private void Awake()
        {
            var input = GameRoot.Instance.Services.Get<IInputService>();

            _inputMode = new InputModeController(input);
            _inputReader = new PlayerInputReader(input.GetMap("Player"));
            _performanceMode = new PerformanceMode(_inputMode, input.GetMap("Performance"));

            _cameraRig = new PlayerCameraRig(
                _inputReader, orbitalFollow, mainCamera.transform, lookSensitivity, invertY);

            _motor = new PlayerMotor(
                controller, _inputReader, playerConfig, mainCamera.transform);

            var context = new PlayerContext(transform);
            _interactor = new PlayerInteractor(
                transform, context, interactableLayer, interactRadius);

            _glasses = new PlayerGlasses(_inputReader);

            if (selfLight != null && cinemachineCamera != null)
            {
                _vision = new PlayerVision(
                    selfLight, mainCamera, cinemachineCamera, orbitalFollow, _glasses, visionSettings);
            }
            else
            {
                Debug.LogError("[PlayerController] selfLight 或 cinemachineCamera 未赋值，视野切换不会生效");
            }

            _inputReader.InteractPressed += OnInteractPressed;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            _inputReader.Poll();
            
            _cameraRig.Tick();
            _motor.Tick(Time.deltaTime);
            _interactor.Tick(_inputReader.ScrollValue);
            _vision?.Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            _inputReader.InteractPressed -= OnInteractPressed;

            _glasses?.Dispose();
            _vision?.Dispose();
            _motor?.Dispose();
            _performanceMode?.Dispose();
            _inputReader?.Dispose();
            _inputMode?.Dispose();
        }

        #endregion

        #region 内部实现

        private void OnInteractPressed()
        {
            _interactor.TryInteract();
        }

        #endregion
    }
}
