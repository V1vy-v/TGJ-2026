using System.Collections;
using Framework.Bootstrap;
using Framework.Services.SceneFlow;
using Framework.Services.UI;
using UnityEngine;

namespace Gameplay.UI
{
    /// <summary>
    /// 场景切换期间显示 LoadingOverlay，并保证至少显示一段时间。
    /// </summary>
    public class LoadingScreenController : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float minDisplayDuration = 6f;

        private IUIService _ui;
        private ISceneFlowService _sceneFlow;
        private Coroutine _closeRoutine;
        private float _startedAt;

        private void Start()
        {
            if (GameRoot.Instance == null)
                return;

            _ui = GameRoot.Instance.Services.Get<IUIService>();
            _sceneFlow = GameRoot.Instance.Services.Get<ISceneFlowService>();

            _sceneFlow.LoadStarted += OnLoadStarted;
            _sceneFlow.LoadCompleted += OnLoadCompleted;
            _sceneFlow.LoadFailed += OnLoadFailed;
        }

        private void OnDestroy()
        {
            if (_sceneFlow == null)
                return;

            _sceneFlow.LoadStarted -= OnLoadStarted;
            _sceneFlow.LoadCompleted -= OnLoadCompleted;
            _sceneFlow.LoadFailed -= OnLoadFailed;
        }

        private void OnLoadStarted(string sceneName)
        {
            if (_closeRoutine != null)
            {
                StopCoroutine(_closeRoutine);
                _closeRoutine = null;
            }

            _startedAt = Time.unscaledTime;
            _ui.OpenOverlay<LoadingOverlay>();
        }

        private void OnLoadCompleted(string sceneName)
        {
            if (_closeRoutine != null)
                StopCoroutine(_closeRoutine);

            _closeRoutine = StartCoroutine(CloseAfterMinimum());
        }

        private void OnLoadFailed(string sceneName)
        {
            if (_closeRoutine != null)
                StopCoroutine(_closeRoutine);

            _closeRoutine = StartCoroutine(CloseAfterMinimum());
        }

        private IEnumerator CloseAfterMinimum()
        {
            float remaining = minDisplayDuration - (Time.unscaledTime - _startedAt);
            if (remaining > 0f)
                yield return new WaitForSecondsRealtime(remaining);

            _ui.Close<LoadingOverlay>();
            _closeRoutine = null;
        }
    }
}
