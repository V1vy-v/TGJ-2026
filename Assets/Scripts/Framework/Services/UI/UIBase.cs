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

        private bool _created;
        private bool _invalid;

        private CanvasGroup _canvasGroup;
        private Coroutine _fadeRoutine;

        [SerializeField] private float fadeDuration = 0.15f;

        #endregion

        #region 生命周期

        protected virtual void Awake()
        {
            EnsureCreated();
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

        /// <summary>首次使用前调用一次（Awake 或 Setup，以先到者为准），用于控件缓存与自身监听绑定。</summary>
        protected virtual void OnCreate() { }

        /// <summary>实例被销毁时调用（缓存的界面关闭时不会触发）。</summary>
        protected virtual void OnDestroyed() { }

        #endregion

        #region 对外接口

        public void Setup(object args)
        {
            if (!EnsureCreated()) return;

            OnSetup(args);
        }

        public void Show()
        {
            if (!EnsureCreated()) return;

            _canvasGroup.gameObject.SetActive(true);
            if (_fadeRoutine != null)
                StopCoroutine(_fadeRoutine);

            _fadeRoutine = StartCoroutine(ShowCoroutine());
            _canvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            if (!EnsureCreated()) return;
            if (!gameObject.activeInHierarchy) return;

            if (_fadeRoutine != null)
                StopCoroutine(_fadeRoutine);

            _fadeRoutine = StartCoroutine(HideCoroutine());
        }

        /// <summary>跳过淡出，立即隐藏。Screen 关闭时由管理器调用。</summary>
        public void HideImmediate()
        {
            if (!EnsureCreated()) return;

            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
                _fadeRoutine = null;
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            OnHide();
            _canvasGroup.gameObject.SetActive(false);
        }

        #endregion

        #region 内部实现

        /// <summary>保证 OnCreate 只执行一次；缺少 CanvasGroup 时只报一次错并返回 false。</summary>
        private bool EnsureCreated()
        {
            if (_created) return true;
            if (_invalid) return false;

            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup == null)
            {
                _invalid = true;
                Debug.LogError($"[UIBase] {name} 根节点缺少 CanvasGroup，组件已禁用", this);
                enabled = false;
                return false;
            }

            _created = true;
            OnCreate();
            return true;
        }

        private float FadeStep => Time.deltaTime / Mathf.Max(fadeDuration, 0.01f);

        private IEnumerator ShowCoroutine()
        {
            while (_canvasGroup.alpha < 0.95f)
            {
                _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, 1f, FadeStep);
                yield return null;
            }

            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            OnShow();

            _fadeRoutine = null;
        }

        private IEnumerator HideCoroutine()
        {
            while (_canvasGroup.alpha > 0.05f)
            {
                _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, 0f, FadeStep);
                yield return null;
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            OnHide();
            _canvasGroup.gameObject.SetActive(false);

            _fadeRoutine = null;
        }

        #endregion
    }
}
