using Framework.Bootstrap;
using Framework.Services.SceneFlow;
using Framework.Services.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI
{
    public class MainMenuScreen : UIBase
    {
        #region 组件

        [SerializeField] private Button startBtn;
        [SerializeField] private Button settingBtn;
        [SerializeField] private Button versionBtn;
        [SerializeField] private Button quitBtn;

        #endregion

        protected override void OnCreate()
        {
            base.OnCreate();
            startBtn.onClick.AddListener(OnStart);
            settingBtn.onClick.AddListener(OnSetting);
            versionBtn.onClick.AddListener(OnVersion);
            quitBtn.onClick.AddListener(OnQuit);
        }

        private void OnStart()
        {
            GameRoot.Instance.Services.Get<ISceneFlowService>().LoadScene("GameScene");
        }

        private void OnSetting()
        {
            
        }
        
        private void OnVersion()
        {
            
        }
        
        private void OnQuit()
        {
            Application.Quit();
        }
    }
}
