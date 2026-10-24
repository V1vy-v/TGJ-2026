using System;
using System.Collections;
using Framework.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework.Services.SceneFlow
{
    public class SceneFlowManager : IManager,ISceneFlowService
    {
        #region 状态

        private MonoBehaviour _host;
        private Coroutine _running;
        public int Order { get; } = 30;
        public bool IsLoading { get; private set; }

        #endregion
        
        #region 生命周期

        public void Initialize(ServiceRegistry services, MonoBehaviour host)
        {
            if (services == null || host == null)
            {
                Debug.LogError($"[SceneFlowManager]: {nameof(services)}/{nameof(host)} is null");
                return;
            }

            _host = host;
            services.Register<ISceneFlowService>(this);
        }
  
        public void Shutdown()
        {
            IsLoading = false;
            if (_running != null)
            {
                _host.StopCoroutine(_running);
                _running = null;
            }
        }

        #endregion

        #region 对外接口

        public event Action<string> LoadStarted;
        public event Action<float> ProgressChanged;
        public event Action<string> LoadCompleted;
        public event Action<string> LoadFailed;
        
        public void LoadScene(string sceneName)
        {
            if (IsLoading)
            {
                Debug.LogWarning($"[SceneFlowManager]: 加载失败: 当前正在加载场景中");
                return;
            }
            if(_host == null)
            {
                Debug.LogError($"[SceneFlowManager]: 加载失败: {nameof(_host)} is null");
                return;
            }
            _running = _host.StartCoroutine(LoadRoutine(sceneName));
        }

        #endregion

        private IEnumerator LoadRoutine(string sceneName)
        {
            IsLoading = true;
            LoadStarted?.Invoke(sceneName);
            AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName);
            
            if (ao == null)
            {
                Debug.LogError($"[SceneFlowManager]: 加载失败: {sceneName}不存在");
                IsLoading = false;
                _running = null;
                LoadFailed?.Invoke(sceneName);
                yield break;
            }

            while (ao.progress < 0.9f)
            {
                ProgressChanged?.Invoke(Mathf.Clamp01(ao.progress / 0.9f));
                yield return null;
            }
            ProgressChanged?.Invoke(1f);
            ao.allowSceneActivation = true;
            
            while(!ao.isDone)
                yield return null;
            
            IsLoading = false;
            _running = null;
            LoadCompleted?.Invoke(sceneName);
        }
        
        
    }
}
