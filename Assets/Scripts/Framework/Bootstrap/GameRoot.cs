using System.Collections.Generic;
using Framework.Core;
using Framework.Services.Input;
using UnityEngine;

namespace Framework.Bootstrap
{
    [DefaultExecutionOrder(-1000)]
    public class GameRoot : MonoBehaviour
    {
        public static GameRoot Instance { get; private set; }
        public ServiceRegistry Services { get; } = new();

        [SerializeField] private BootstrapConfig _bootstrapConfig;
        public BootstrapConfig Config => _bootstrapConfig;

        private readonly List<IManager> _managers = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            BuildManagers();
            _managers.Sort((a, b) => a.Order.CompareTo(b.Order));

            foreach (var manager in _managers)
                manager.Initialize(Services, this);

            Debug.Log($"[GameRoot] 已初始化 {_managers.Count} 个管理器");
        }

        private void BuildManagers()
        {
            // 每做好一个管理器，在这里 new 一行并取消注释
            _managers.Add(new InputManager(Config.InputAsset));   // Order 10
            // _managers.Add(new SceneFlowManager());                   // Order 30
            // _managers.Add(new UIManager());                          // Order 40
        }

        private void OnDestroy()
        {
            if (Instance != this) return;

            for (int i = _managers.Count - 1; i >= 0; i--)
                _managers[i].Shutdown();

            _managers.Clear();
            Instance = null;
        }
    }
}