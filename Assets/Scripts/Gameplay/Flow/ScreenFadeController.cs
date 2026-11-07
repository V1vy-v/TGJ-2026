using System.Collections;
using Framework.Bootstrap;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Flow
{
    /// <summary>
    /// 全屏黑场：运行时创建，不依赖 UI Prefab。
    /// 传送点通过 FadeOut / FadeIn 控制黑屏，不参与迷宫本身的黑色渲染。
    /// </summary>
    public sealed class ScreenFadeController : MonoBehaviour
    {
        #region 状态

        private static ScreenFadeController _instance;

        public static ScreenFadeController Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                if (GameRoot.Instance == null)
                    return null;

                _instance = GameRoot.Instance.gameObject
                    .AddComponent<ScreenFadeController>();
                return _instance;
            }
        }

        private CanvasGroup _canvasGroup;
        private bool _isFading;

        public bool IsFading => _isFading;

        #endregion

        #region 生命周期

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }

            _instance = this;
            BuildCanvas();
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        #endregion

        #region 对外接口

        public IEnumerator FadeOut(float duration = 0.25f)
        {
            yield return FadeTo(1f, duration);
        }

        public IEnumerator FadeIn(float duration = 0.25f)
        {
            yield return FadeTo(0f, duration);
        }

        #endregion

        #region 内部实现

        private void BuildCanvas()
        {
            var canvasObject = new GameObject(
                "ScreenFadeCanvas",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10000;

            _canvasGroup = canvasObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            var imageObject = new GameObject(
                "Black",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));

            imageObject.transform.SetParent(canvasObject.transform, false);

            var rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = imageObject.GetComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = false;
        }

        private IEnumerator FadeTo(float target, float duration)
        {
            _isFading = true;
            float start = _canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(
                    start, target, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            _canvasGroup.alpha = target;
            _isFading = false;
        }

        #endregion
    }
}
