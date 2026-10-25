using System;
using System.Collections.Generic;
using Framework.Core;
using UnityEngine;

namespace Framework.Services.UI
{
    public class UIManager : IManager, IUIService
    {
        #region 状态

        public int Order => 40;
        
        private readonly GameObject _uiRootPrefab;
        private readonly UIRegistry _uiRegistry;

        private UIRoot _uiRootInstance;
        private GameObject _blocker;

        private readonly Dictionary<UILayer, Transform> _layerRoots = new();
        private readonly Dictionary<UILayer, List<UIBase>> _openViews = new();
        private readonly Dictionary<Type, UIBase> _uiCache = new();
        private readonly Dictionary<UIBase, Action> _popupCallbacks = new();

        private UIBase _screen;

        #endregion

        #region 生命周期

        public UIManager(GameObject uiRootPrefab, UIRegistry registry)
        {
            _uiRootPrefab = uiRootPrefab;
            _uiRegistry = registry;
        }

        public void Initialize(ServiceRegistry services, MonoBehaviour host)
        {
            if (services == null || host == null)
            {
                Debug.LogError("[UIManager] services 或 host 为空");
                return;
            }

            if (_uiRootPrefab == null || _uiRegistry == null)
            {
                Debug.LogError("[UIManager] UIRoot 预制体或 UIRegistry 未配置");
                return;
            }

            var instance = UnityEngine.Object.Instantiate(_uiRootPrefab, host.transform);
            _uiRootInstance = instance.GetComponent<UIRoot>();
            if (_uiRootInstance == null)
            {
                Debug.LogError("[UIManager] UIRoot 预制体上缺少 UIRoot 组件");
                return;
            }

            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                _openViews[layer] = new List<UIBase>();
                _layerRoots[layer] = _uiRootInstance.GetLayerRoot(layer);

                if (_layerRoots[layer] == null)
                    Debug.LogError($"[UIManager] 层 {layer} 没有绑定父节点");
            }

            _blocker = _uiRootInstance.PopupBlocker;
            if (_blocker != null)
                _blocker.SetActive(false);

            services.Register<IUIService>(this);
        }

        public void Shutdown()
        {
            CloseAll();

            if (_uiRootInstance != null)
                UnityEngine.Object.Destroy(_uiRootInstance.gameObject);

            _uiRootInstance = null;
            _blocker = null;
            _layerRoots.Clear();
            _openViews.Clear();
            _uiCache.Clear();
            _popupCallbacks.Clear();
        }

        #endregion

        #region 对外接口
        
        public event Action<UIBase> Opened;
        public event Action<UIBase> Closed;
        
        public void OpenScreen<T>(object args = null) where T : UIBase
        {
            if (!TryGetEntry<T>(out var entry)) return;
            if (!CheckLayer(entry, UILayer.Screen)) return;

            CloseAllPanelsAndPopups();

            if (_screen != null)
                CloseView(_screen, UILayer.Screen);

            var view = UnityEngine.Object.Instantiate(entry.Prefab, _layerRoots[UILayer.Screen]);
            OpenView(view, UILayer.Screen, args);
        }

        public void OpenPanel<T>(object args = null) where T : UIBase
        {
            if (!TryGetEntry<T>(out var entry)) return;
            if (!CheckLayer(entry, UILayer.Panel)) return;

            var view = GetOrCreate<T>(entry, UILayer.Panel);
            if (view != null)
                OpenView(view, UILayer.Panel, args);
        }

        public void OpenPopup<T>(object args = null, Action onClosed = null) where T : UIBase
        {
            if (!TryGetEntry<T>(out var entry)) return;
            if (!CheckLayer(entry, UILayer.Popup)) return;

            CloseCurrentPopup();

            var view = GetOrCreate<T>(entry, UILayer.Popup);
            if (view == null) return;

            _popupCallbacks[view] = onClosed;
            OpenView(view, UILayer.Popup, args);
        }

        public void Close<T>() where T : UIBase
        {
            if (_screen != null && _screen.GetType() == typeof(T))
            {
                CloseView(_screen, UILayer.Screen);
                return;
            }

            foreach (var pair in _openViews)
            {
                if (pair.Key == UILayer.Screen) continue;

                for (int i = pair.Value.Count - 1; i >= 0; i--)
                {
                    if (pair.Value[i].GetType() != typeof(T)) continue;

                    CloseView(pair.Value[i], pair.Key);
                    return;
                }
            }
        }

        public void CloseTop(UILayer layer)
        {
            if (layer == UILayer.Screen)
            {
                if (_screen != null)
                    CloseView(_screen, UILayer.Screen);
                return;
            }

            var list = _openViews[layer];
            if (list.Count == 0)
            {
                Debug.LogWarning($"[UIManager] 层 {layer} 没有打开的界面");
                return;
            }

            CloseView(list[list.Count - 1], layer);
        }

        public void CloseAll()
        {
            CloseLayer(UILayer.Overlay);
            CloseLayer(UILayer.Popup);
            CloseLayer(UILayer.Panel);

            if (_screen != null)
                CloseView(_screen, UILayer.Screen);
        }

        public bool IsOpen<T>() where T : UIBase
        {
            if (_screen != null && _screen.GetType() == typeof(T))
                return true;

            foreach (var list in _openViews.Values)
            {
                foreach (var view in list)
                {
                    if (view != null && view.GetType() == typeof(T))
                        return true;
                }
            }

            return false;
        }

        #endregion

        #region 内部实现

        private bool TryGetEntry<T>(out UIResolvedEntry entry) where T : UIBase
        {
            if (_uiRegistry.TryGetUI<T>(out entry))
                return true;

            Debug.LogError($"[UIManager] 界面 {typeof(T).Name} 未在注册表中找到");
            return false;
        }

        private bool CheckLayer(UIResolvedEntry entry, UILayer expected)
        {
            if (entry.Layer == expected)
                return true;

            Debug.LogError($"[UIManager] 界面 {entry.Prefab.GetType().Name} 注册在 {entry.Layer}，不能按 {expected} 打开");
            return false;
        }

        private UIBase GetOrCreate<T>(UIResolvedEntry entry, UILayer layer) where T : UIBase
        {
            if (_uiCache.TryGetValue(typeof(T), out var cached) && cached != null)
                return cached;

            _uiCache.Remove(typeof(T));

            var created = UnityEngine.Object.Instantiate(entry.Prefab, _layerRoots[layer]);
            _uiCache[typeof(T)] = created;
            return created;
        }

        private void OpenView(UIBase view, UILayer layer, object args)
        {
            if (view == null) return;

            if (layer == UILayer.Screen)
            {
                if (_screen == view) return;
                _screen = view;
            }
            else
            {
                var list = _openViews[layer];
                if (list.Contains(view)) return;
                list.Add(view);
            }

            view.Setup(args);
            view.Show();

            if (layer == UILayer.Popup && _blocker != null)
                _blocker.SetActive(true);

            Opened?.Invoke(view);
        }

        private void CloseView(UIBase view, UILayer layer)
        {
            if (view == null) return;

            if (layer == UILayer.Screen)
            {
                if (_screen != view) return;
                _screen = null;
            }
            else
            {
                if (!_openViews[layer].Remove(view)) return;
            }

            if (layer == UILayer.Screen)
                view.HideImmediate();
            else
                view.Hide();

            if (layer == UILayer.Popup)
            {
                if (_blocker != null)
                    _blocker?.SetActive(false);

                if (_popupCallbacks.Remove(view, out var callback))
                {
                    callback?.Invoke();
                }
            }

            Closed?.Invoke(view);

            if (layer == UILayer.Screen)
                UnityEngine.Object.Destroy(view.gameObject);
        }

        private void CloseLayer(UILayer layer)
        {
            var list = _openViews[layer];
            for (var i = list.Count - 1; i >= 0; i--)
                CloseView(list[i], layer);
        }

        private void CloseCurrentPopup()
        {
            var list = _openViews[UILayer.Popup];
            if (list.Count > 0)
                CloseView(list[list.Count - 1], UILayer.Popup);
        }

        private void CloseAllPanelsAndPopups()
        {
            CloseCurrentPopup();
            CloseLayer(UILayer.Panel);
        }

        #endregion
    }
}
