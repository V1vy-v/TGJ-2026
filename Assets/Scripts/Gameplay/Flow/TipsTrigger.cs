using Framework.Bootstrap;
using Framework.Services.Localization;
using Framework.Services.UI;
using Gameplay.Player;
using Gameplay.UI;
using UnityEngine;

namespace Gameplay.Flow
{
    public enum TipsTriggerMode
    {
        Collision,
        Event,
    }

    /// <summary>
    /// Tips 触发入口
    /// </summary>
    public class TipsTrigger : MonoBehaviour
    {
        #region 配置

        [SerializeField] private TipsTriggerMode mode = TipsTriggerMode.Collision;
        [SerializeField] private string tipsKey;
        [SerializeField] private float duration = 2.5f;
        [SerializeField] private bool oneShot = true;

        #endregion

        #region 状态

        private bool _done;

        #endregion

        #region 生命周期

        private void OnTriggerEnter(Collider other)
        {
            if (mode != TipsTriggerMode.Collision)
                return;

            var player = other.GetComponentInParent<PlayerController>();
            if (player == null)
                return;

            Trigger();
        }

        private void OnTriggerStay(Collider other)
        {
            if (mode != TipsTriggerMode.Collision)
                return;

            var player = other.GetComponentInParent<PlayerController>();
            if (player == null)
                return;

            Trigger();
        }

        #endregion

        #region 对外接口

        public void Trigger()
        {
            if (_done)
                return;

            if (Show(tipsKey, duration) && oneShot)
                _done = true;
        }

        public void ResetTrigger()
        {
            _done = false;
        }

        /// <summary>
        /// 代码事件直接调用，不依赖场景里的 TipsTrigger 实例。
        /// </summary>
        public static bool Show(string key, float duration = 2.5f)
        {
            if (string.IsNullOrWhiteSpace(key) || GameRoot.Instance == null)
                return false;

            var player = Object.FindFirstObjectByType<PlayerController>();
            if (player != null && player.Dialogue != null && player.Dialogue.IsActive)
                return false;

            var localization =
                GameRoot.Instance.Services.Get<ILocalizationService>();
            var ui = GameRoot.Instance.Services.Get<IUIService>();

            string text = localization.GetText(key);
            if (string.IsNullOrEmpty(text) || text == key)
                return false;

            if (ui.IsOpen<TipsOverlay>())
                return false;

            ui.OpenOverlay<TipsOverlay>(new TipsContent(text, duration));
            return true;
        }

        #endregion

        #region 内部实现

        [ContextMenu("Trigger Tips")]
        private void TriggerFromContextMenu()
        {
            Trigger();
        }

        #endregion
    }
}
