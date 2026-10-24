using UnityEngine;

namespace Framework.Core
{
    /// <summary>管理器生命周期。Order 小的先初始化、后关闭。</summary>
    public interface IManager
    {
        int Order { get; }

        /// <param name="services">用于注册自己，也用于取别的服务。</param>
        /// <param name="host">需要协程、创建 GameObject 时用（GameRoot 自身）。</param>
        void Initialize(ServiceRegistry services, MonoBehaviour host);

        void Shutdown();
    }
}