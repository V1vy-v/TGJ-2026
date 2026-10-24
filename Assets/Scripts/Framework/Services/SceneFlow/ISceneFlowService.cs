using System;

namespace Framework.Services.SceneFlow
{
    /// <summary>
    /// 场景流转服务接口
    /// </summary>
    public interface ISceneFlowService
    {
        bool IsLoading { get; }
        void LoadScene(string sceneName);
        event Action<string> LoadStarted;
        event Action<float> ProgressChanged;
        event Action<string> LoadCompleted;
        event Action<string> LoadFailed;
    }
}
