using System;
using Framework.Bootstrap;
using Framework.Services.Input;
using Framework.Services.UI;
using Gameplay.Audio;
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
        private DialogueInputReader _dialogueInput;
        private PerformanceMode _performanceMode;
        private InputModeController _inputMode;
        private PlayerCameraRig _cameraRig;
        private PlayerInteractor _interactor;
        private PlayerMotor _motor;
        private PlayerGlasses _glasses;
        private PlayerVision _vision;
        private PlayerEnergy _energy;
        private PlayerPulse _pulse;
        private PlayerDialogue _dialogue;
        private PlayerAnimator _animator;
        private IGameAudio _gameAudio;

        private const string NarratorSpeakerKey = "speaker.narrator";
        private const string PlayerSpeakerKey = "speaker.player";

        public PlayerDialogue Dialogue => _dialogue;
        public event Action DialogueAdvanceRequested;
        public event Action KeyboardAcquired;
        
        public PerformanceMode PerformanceMode => _performanceMode;
        public PlayerInteractor Interactor => _interactor;
        public PlayerEnergy Energy => _energy;
        public bool HasMazeKey { get; private set; }

        public void CollectMazeKey()
        {
            HasMazeKey = true;
        }

        [SerializeField] private PlayerConfig playerConfig;
        [SerializeField] private CharacterController controller;
        [SerializeField] private CinemachineOrbitalFollow orbitalFollow;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float lookSensitivity = 0.15f;
        [SerializeField] private bool invertY;

        [Header("动画")]
        [SerializeField] private Animator animator;

        [Header("交互")] 
        [SerializeField] private Transform interactOrigin;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private float interactRadius = 1f;

        [Header("视野")]
        [SerializeField] private GameObject mazeVolume;
        [SerializeField] private Light selfLight;
        [SerializeField] private CinemachineCamera cinemachineCamera;

        private bool _inMaze;
        private bool _footstepActive;
        
        #endregion

        #region 生命周期

        private void Awake()
        {
            var input = GameRoot.Instance.Services.Get<IInputService>();
            GameRoot.Instance.Services.TryGet<IGameAudio>(out _gameAudio);

            _inputMode = new InputModeController(input);
            _inputReader = new PlayerInputReader(input.GetMap("Player"));
            _performanceMode = new PerformanceMode(_inputMode, input.GetMap("Performance"));
            _dialogueInput = new DialogueInputReader(input.GetMap("Dialogue"));

            _cameraRig = new PlayerCameraRig(
                _inputReader, orbitalFollow, mainCamera.transform, lookSensitivity, invertY);
            _motor = new PlayerMotor(
                controller, _inputReader, playerConfig, mainCamera.transform);
            _motor.Jumped += OnJumped;
            _energy = new PlayerEnergy(
                playerConfig.InitialEnergy, playerConfig.EnergyCapacity);
            _dialogue = new PlayerDialogue();

            var modelAnimator = animator != null ? animator : GetComponentInChildren<Animator>(true);
            _animator = new PlayerAnimator(modelAnimator);
            if (modelAnimator == null)
                Debug.LogError("[PlayerController] 未找到 Animator，主角动画不会播放");

            var context = new PlayerContext(transform, _energy, this, StartDialogue);
            _interactor = new PlayerInteractor(
                interactOrigin, context, interactableLayer, interactRadius);

            _glasses = new PlayerGlasses();
            _pulse = new PlayerPulse(transform, playerConfig.Pulse, _gameAudio);
            if (selfLight != null && cinemachineCamera != null)
            {
                _vision = new PlayerVision(
                    selfLight, cinemachineCamera, orbitalFollow, _glasses, playerConfig.Vision);
            }
            else
            {
                Debug.LogError("[PlayerController] selfLight 或 cinemachineCamera 未赋值，视野切换不会生效");
            }

            _dialogue.Ended += OnDialogueEnded;
            _dialogue.LineChanged += OnDialogueLineChanged;
            _dialogueInput.AdvancePressed += OnDialogueAdvancePressed;
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

            // 暂停时不再驱动相机和移动，避免暂停界面下方镜头继续响应鼠标。
            if (Time.timeScale <= 0f)
            {
                SetFootstep(false);
                _animator?.Tick(false);
                return;
            }

            if (_dialogue.IsActive)
            {
                SetFootstep(false);
                _animator?.Tick(false);
                _vision?.Tick(Time.deltaTime);
                return;
            }

            _cameraRig.Tick();
            _motor.Tick(Time.deltaTime);
            _interactor.Tick(_inputReader.ScrollValue);
            SetFootstep(_inputReader.MoveValue.sqrMagnitude > 0.01f);
            _animator?.Tick(_inputReader.MoveValue.sqrMagnitude > 0.01f);
            _vision?.Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            SetFootstep(false);
            _inputReader.InteractPressed -= OnInteractPressed;
            _dialogue.Ended -= OnDialogueEnded;
            _dialogue.LineChanged -= OnDialogueLineChanged;
            _dialogueInput.AdvancePressed -= OnDialogueAdvancePressed;

            _glasses?.Dispose();
            _vision?.Dispose();
            _motor.Jumped -= OnJumped;
            _motor?.Dispose();
            _performanceMode?.Dispose();
            _inputReader?.Dispose();
            _inputMode?.Dispose();
            _dialogueInput.Dispose();
        }

        #endregion

        #region 外部方法

        public void NotifyKeyboardAcquired()
        {
            KeyboardAcquired?.Invoke();
        }

        public void PlayFootstep()
        {
            // 动画事件兼容入口；脚步声由 Update 中的移动状态统一控制。
        }

        private void SetFootstep(bool active)
        {
            if (_footstepActive == active)
                return;

            _footstepActive = active;
            if (active)
                _gameAudio?.StartFootstep();
            else
                _gameAudio?.StopFootstep();
        }

        /// <summary>物品被拾取时由交互物调用，播放一次拾取动作。</summary>
        public void NotifyItemPickedUp()
        {
            _animator?.PlayPickup();
        }

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
        public void TeleportTo(Vector3 position, Quaternion rotation)
        {
            if (controller != null)
                controller.enabled = false;

            transform.SetPositionAndRotation(position, rotation);

            if (controller != null)
                controller.enabled = true;
        }

        public bool StartDialogue(TextAsset script)
        {
            return StartDialogue(script, false);
        }

        public bool StartStory(TextAsset script)
        {
            return StartDialogue(script, true);
        }

        private bool StartDialogue(TextAsset script, bool isStory)
        {
            if (_dialogue.IsActive)
                return false;

            if (!_dialogue.Start(script))
                return false;

            _interactor.Suspend();
            _inputMode.SwitchTo(InputMode.Dialogue);

            var ui = GameRoot.Instance.Services.Get<IUIService>();

            if (isStory)
                ui.OpenPopup<StoryPopup>(this);
            else
                ui.OpenPopup<DialoguePopup>(this);

            return true;
        }

        #endregion

        #region 内部实现

        private void OnInteractPressed()
        {
            if (_dialogue.IsActive)
                return;

            _interactor.TryInteract();
        }
        private void OnDialogueAdvancePressed()
        {
            if (!_dialogue.IsActive)
                return;

            DialogueAdvanceRequested?.Invoke();
        }

        private void OnDialogueEnded()
        {
            var ui = GameRoot.Instance.Services.Get<IUIService>();
            ui.Close<DialoguePopup>();
            ui.Close<StoryPopup>();

            _inputMode.SwitchTo(InputMode.Player);
            _interactor.Resume();
        }

        private void OnDialogueLineChanged(PlayerDialogue.Line line)
        {
            if (line.SpeakerKey == NarratorSpeakerKey || line.SpeakerKey == PlayerSpeakerKey)
                return;

            _animator?.PlayNod();
        }
        
        private void OnJumped()
        {
            _animator?.PlayJump();
        }

        private void OnPulsePressed()
        {
            if (!_inMaze) return;
            _pulse.Fire();
        }
        #endregion
    }
}
