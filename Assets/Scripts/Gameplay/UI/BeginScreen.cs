using Framework.Bootstrap;
using Framework.Services.SceneFlow;
using Framework.Services.UI;
using Gameplay.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI
{
    /// <summary>
    /// 开始界面：进入游戏场景。
    /// 设置和继续功能尚未接入，暂时禁用对应按钮。
    /// </summary>
    public class BeginScreen : UIBase
    {
        #region 组件

        [SerializeField] private Button startBtn;
        [SerializeField] private Button settingsBtn;
        [SerializeField] private Button quitBtn;

        #endregion

        #region 状态

        private bool _starting;
        private IGameAudio _audio;

        #endregion

        #region 生命周期

        protected override void OnCreate()
        {
            base.OnCreate();

            _audio = GameRoot.Instance.Services.TryGet<IGameAudio>(out var audio) ? audio : null;

            if (startBtn != null)
                startBtn.onClick.AddListener(OnStart);

            if (quitBtn != null)
                quitBtn.onClick.AddListener(OnQuit);

            if (settingsBtn != null)
            {
                settingsBtn.interactable = true;
                settingsBtn.onClick.AddListener(OpenSettings);
            }
        }

        private void OpenSettings()
        {
            _audio?.PlaySfx(GameSfx.UiClick);
            GameRoot.Instance.Services.Get<IUIService>().OpenOverlay<SettingPanel>();
        }

        #endregion

        #region 内部实现

        private void OnStart()
        {
            if (_starting) return;
            _starting = true;

            _audio?.PlaySfx(GameSfx.UiClick);

            var ui = GameRoot.Instance.Services.Get<IUIService>();
            ui.Close<BeginScreen>();

            var sceneFlow = GameRoot.Instance.Services.Get<ISceneFlowService>();
            sceneFlow.LoadScene("GameScene");
        }

        private void OnQuit()
        {
            _audio?.PlaySfx(GameSfx.UiClick);
            Application.Quit();
        }

        #endregion
    }
}
