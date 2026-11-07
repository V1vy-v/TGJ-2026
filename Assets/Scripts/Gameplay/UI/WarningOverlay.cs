using Framework.Bootstrap;
using Framework.Services.SceneFlow;
using Framework.Services.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Gameplay.UI
{
    public class WarningOverlay : UIBase
    {
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;

        private IUIService _ui;

        protected override void OnCreate()
        {
            base.OnCreate();

            if (GameRoot.Instance != null)
                _ui = GameRoot.Instance.Services.Get<IUIService>();

            FindBindings();

            if (yesButton != null)
                yesButton.onClick.AddListener(Confirm);

            if (noButton != null)
                noButton.onClick.AddListener(CloseWarning);
        }

        protected override void OnDestroyed()
        {
            if (yesButton != null) yesButton.onClick.RemoveListener(Confirm);
            if (noButton != null) noButton.onClick.RemoveListener(CloseWarning);
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                CloseWarning();
        }

        private void FindBindings()
        {
            if (yesButton == null) yesButton = FindButton("BtnYes");
            if (noButton == null) noButton = FindButton("BtnNo");
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

        private void Confirm()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (GameRoot.Instance == null)
                return;

            _ui?.Close<WarningOverlay>();
            _ui?.Close<SettingPanel>();
            _ui?.Close<PausePopup>();
            GameRoot.Instance.Services.Get<ISceneFlowService>().LoadScene("MainScene");
        }

        private void CloseWarning()
        {
            _ui?.Close<WarningOverlay>();
        }
    }
}
