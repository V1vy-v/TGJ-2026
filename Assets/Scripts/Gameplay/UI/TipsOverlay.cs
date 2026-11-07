using System;
using System.Collections;
using System.Collections.Generic;
using Framework.Bootstrap;
using Framework.Services.UI;
using TMPro;
using UnityEngine;

namespace Gameplay.UI
{
    public readonly struct TipsContent
    {
        public string Text { get; }
        public float Duration { get; }

        public TipsContent(string text, float duration)
        {
            Text = text;
            Duration = duration;
        }
    }

    /// <summary>
    /// 顶部提示视图：一次性显示整段文字，计时结束后抛 Finished。
    /// </summary>
    public class TipsOverlay : UIBase
    {
        #region 组件

        [SerializeField] private TextMeshProUGUI tips;

        private CanvasGroup _tipsCanvasGroup;

        #endregion

        #region 状态

        private TipsContent _content;
        private Coroutine _hideRoutine;

        public event Action Finished;

        #endregion

        #region 生命周期

        protected override void OnCreate()
        {
            base.OnCreate();
            _tipsCanvasGroup = GetComponent<CanvasGroup>();

            if (tips == null)
                tips = GetComponentInChildren<TextMeshProUGUI>(true);
        }

        protected override void OnSetup(object args)
        {
            base.OnSetup(args);

            if (args is TipsContent content)
                _content = content;
        }

        protected override void OnShow()
        {
            base.OnShow();

            if (_tipsCanvasGroup != null)
            {
                _tipsCanvasGroup.interactable = false;
                _tipsCanvasGroup.blocksRaycasts = false;
            }

            if (tips != null)
                tips.text = _content.Text;

            StopHide();
            _hideRoutine = StartCoroutine(HideRoutine());
        }

        protected override void OnHide()
        {
            StopHide();
            _content = default;
        }

        #endregion

        #region 内部实现

        private IEnumerator HideRoutine()
        {
            if (_content.Duration > 0f)
                yield return new WaitForSecondsRealtime(_content.Duration);

            _hideRoutine = null;

            if (GameRoot.Instance != null)
            {
                var ui = GameRoot.Instance.Services.Get<IUIService>();
                ui.Close<TipsOverlay>();
            }
            else
            {
                Hide();
            }

            Finished?.Invoke();
        }

        private void StopHide()
        {
            if (_hideRoutine != null)
            {
                StopCoroutine(_hideRoutine);
                _hideRoutine = null;
            }
        }

        #endregion
    }
}
