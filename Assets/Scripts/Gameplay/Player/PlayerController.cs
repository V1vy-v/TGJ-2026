using Framework.Bootstrap;
using Framework.Services.Input;
using Framework.Services.UI;
using Gameplay.Player.Act;
using Gameplay.Player.Input;
using Gameplay.Player.Interaction;
using Gameplay.Player.Pulse;
using Gameplay.Rendering;
using Gameplay.UI;
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
        private PlayerEnergy _energy;
        private PlayerPulse _pulse;
        
        public PerformanceMode PerformanceMode => _performanceMode;
        public PlayerInteractor Interactor => _interactor;
        public PlayerEnergy Energy => _energy;

        [SerializeField] private PlayerConfig playerConfig;
        [SerializeField] private CharacterController controller;
        [SerializeField] private CinemachineOrbitalFollow orbitalFollow;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float lookSensitivity = 0.15f;
        [SerializeField] private bool invertY;

        [Header("交互")] 
        [SerializeField] private Transform interactOrigin;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private float interactRadius = 1f;

        [Header("视野")]
        [SerializeField] private GameObject mazeVolume;
        [SerializeField] private Light selfLight;
        [SerializeField] private CinemachineCamera cinemachineCamera;
        
        private bool _inMaze;

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
            _energy = new PlayerEnergy(
                playerConfig.InitialEnergy, playerConfig.EnergyCapacity);
            
            var context = new PlayerContext(transform, _energy);
            _interactor = new PlayerInteractor(
                interactOrigin, context, interactableLayer, interactRadius);

            _glasses = new PlayerGlasses(_inputReader);
            _pulse = new PlayerPulse(transform, playerConfig.Pulse);
            if (selfLight != null && cinemachineCamera != null)
            {
                _vision = new PlayerVision(
                    selfLight, cinemachineCamera, orbitalFollow, _glasses, playerConfig.Vision);
            }
            else
            {
                Debug.LogError("[PlayerController] selfLight 或 cinemachineCamera 未赋值，视野切换不会生效");
            }

            _inputReader.InteractPressed += OnInteractPressed;
            _inputReader.PulsePressed += OnPulsePressed;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        private void Start()
        {
            var ui = GameRoot.Instance.Services.Get<IUIService>();
            ui.OpenPanel<HUD>(this);
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

        #region 外部方法

        public void EnterMaze()
        {
            if (_inMaze) return;
            _inMaze = true;

            _glasses.SetWearing(false);
            _glasses.ToggleEnabled = false;

            if (mazeVolume != null)
                mazeVolume.SetActive(true);

            MazeBlackoutFeature.SetEnabled(true);
        }

        public void ExitMaze()
        {
            if (!_inMaze) return;
            _inMaze = false;

            _glasses.ToggleEnabled = true;
            _glasses.SetWearing(true);

            if (mazeVolume != null)
                mazeVolume.SetActive(false);
            
            ScanRendererFeature.Instance?.StopPulse();
            MazeBlackoutFeature.SetEnabled(false);
        }

        #endregion

        #region 内部实现

        private void OnInteractPressed()
        {
            _interactor.TryInteract();
        }
        
        private void OnPulsePressed()
        {
            if (!_inMaze) return;
            _pulse.Fire();
        }
        #endregion
    }
}
