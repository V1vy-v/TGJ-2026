using Framework.Bootstrap;
using Framework.Services.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.UI
{
    public class PauseController : MonoBehaviour
    {
        private IUIService _ui;

        private void Start()
        {
            if (GameRoot.Instance != null)
                _ui = GameRoot.Instance.Services.Get<IUIService>();
        }

        private void Update()
        {
            if (_ui == null || Keyboard.current == null)
                return;

            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "GameScene")
                return;

            if (!Keyboard.current.escapeKey.wasPressedThisFrame)
                return;

            // Story 对话正在显示时，不允许被暂停界面顶掉。
            if (_ui.IsOpen<StoryPopup>() || _ui.IsOpen<DialoguePopup>())
                return;

            if (_ui.IsOpen<SettingPanel>())
            {
                _ui.Close<SettingPanel>();
                return;
            }

            if (_ui.IsOpen<PausePopup>())
                return;

            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _ui.OpenPopup<PausePopup>();
        }
    }
}
