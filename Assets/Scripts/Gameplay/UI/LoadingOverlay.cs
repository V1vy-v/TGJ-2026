using Framework.Services.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI
{
    public class LoadingOverlay : UIBase
    {
        #region 控件

        [SerializeField]private Image loadingBar;

        #endregion

        protected override void OnCreate()
        {
            base.OnCreate();
            
        }

        protected override void OnSetup(object args)
        {
            base.OnSetup(args);
            
        }
    }
}
