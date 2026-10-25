using System;
using System.Collections;
using UnityEngine;

namespace Framework.Services.UI
{
    /// <summary>
    /// 所有界面的基类。由 UIManager 调用 Setup / Show / Hide，子类只重写 On* 钩子。
    /// </summary>
    public abstract class UIBase : MonoBehaviour
    {
        #region 状态

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeDuration = 0.15f;

        private Coroutine _fadeRoutine;

        #endregion

        #region 生命周期

        protected virtual void Awake()
        {
            if (canvasGroup == null)
            {
                Debug.LogError($"[UIBase] {name} 缺少 CanvasGroup，组件已禁用", this);
                enabled = false;
            }
        }

        protected virtual void OnDestroy()
        {
            OnDestroyed();
        }

        /// <summary>每次打开时由管理器调用，用于接收本次打开的参数。</summary>
        protected virtual void OnSetup(object args) { }

        /// <summary>淡入完成、界面完全可见时调用。</summary>
        protected virtual void OnShow() { }

        /// <summary>淡出完成、即将隐藏时调用。</summary>
        protected virtual void OnHide() { }

        /// <summary>实例被销毁时调用（缓存的界面关闭时不会触发）。</summary>
        protected virtual void OnDestroyed() { }

        #endregion

        #region 对外接口

        public void Setup(object args)
        {
            OnSetup(args);
        }

        public void Show()
        {
            if (canvasGroup == null) return;

            canvasGroup.gameObject.SetActive(true);
            if (_fadeRoutine != null)
                StopCoroutine(_fadeRoutine);

            _fadeRoutine = StartCoroutine(ShowCoroutine());
            canvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            if (canvasGroup == null) return;
            if (!gameObject.activeInHierarchy) return;

            if (_fadeRoutine != null)
                StopCoroutine(_fadeRoutine);

            _fadeRoutine = StartCoroutine(HideCoroutine());
        }

        /// <summary>跳过淡出，立即隐藏。Screen 关闭时由管理器调用。</summary>
        public void HideImmediate()
        {
            if (canvasGroup == null) return;

            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
                _fadeRoutine = null;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            OnHide();
            canvasGroup.gameObject.SetActive(false);
        }

        #endregion

        #region 内部实现

        private float FadeStep => Time.deltaTime / Mathf.Max(fadeDuration, 0.01f);

        private IEnumerator ShowCoroutine()
        {
            while (canvasGroup.alpha < 0.95f)
            {
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 1f, FadeStep);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            OnShow();

            _fadeRoutine = null;
        }

        private IEnumerator HideCoroutine()
        {
            while (canvasGroup.alpha > 0.05f)
            {
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, FadeStep);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            OnHide();
            canvasGroup.gameObject.SetActive(false);

            _fadeRoutine = null;
        }

        #endregion
    }
}
