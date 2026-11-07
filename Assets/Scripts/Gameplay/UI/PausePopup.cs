using Framework.Bootstrap;
using Framework.Services.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Gameplay.UI
{
    public class PausePopup : UIBase
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;

        private IUIService _ui;

        protected override void OnCreate()
        {
            base.OnCreate();

            if (GameRoot.Instance != null)
                _ui = GameRoot.Instance.Services.Get<IUIService>();

            FindBindings();

            if (resumeButton != null)
                resumeButton.onClick.AddListener(Resume);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OpenSettings);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OpenLeaveWarning);
        }

        protected override void OnDestroyed()
        {
            if (resumeButton != null) resumeButton.onClick.RemoveListener(Resume);
            if (settingsButton != null) settingsButton.onClick.RemoveListener(OpenSettings);
            if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(OpenLeaveWarning);
        }

        private void Update()
        {
            if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
                return;

            if (_ui != null && (_ui.IsOpen<SettingPanel>() || _ui.IsOpen<WarningOverlay>()))
                return;

            Resume();
        }

        private void FindBindings()
        {
            if (resumeButton == null) resumeButton = FindButton("btnContinue");
            if (settingsButton == null) settingsButton = FindButton("btnSetting");
            if (mainMenuButton == null) mainMenuButton = FindButton("btnBack2Menu");

            if (resumeButton != null && settingsButton != null && mainMenuButton != null)
                return;

            var buttons = GetComponentsInChildren<Button>(true);
            if (resumeButton == null && buttons.Length > 0) resumeButton = buttons[0];
            if (settingsButton == null && buttons.Length > 1) settingsButton = buttons[1];
            if (mainMenuButton == null && buttons.Length > 2) mainMenuButton = buttons[2];
        }

        private Button FindButton(string objectName)
        {
            foreach (var button in GetComponentsInChildren<Button>(true))
            {
                if (button.name == objectName)
                    return button;
            }

            return null;
        }

        private void Resume()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (_ui != null)
            {
                _ui.Close<SettingPanel>();
                _ui.Close<PausePopup>();
            }
        }

        private void OpenSettings()
        {
            _ui?.OpenOverlay<SettingPanel>();
        }

        private void OpenLeaveWarning()
        {
            _ui?.OpenOverlay<WarningOverlay>();
        }
    }
}
