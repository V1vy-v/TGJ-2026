using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Services.UI
{
    [CreateAssetMenu(fileName = "UIRegistry", menuName = "Framework/UIRegistry")]
    public class UIRegistry : ScriptableObject
    {
        // UI静态配置：存储项目已有UI
        [SerializeField] private List<UIGroup> uiGroups;
        
        // 配置UI存储字典：启动时由uiGroups注入，方便查询
        private readonly Dictionary<Type, UIResolvedEntry> _lookup = new();

        private void OnEnable()
        {
            _lookup.Clear();
            foreach (var uiGroup in uiGroups)
            {
                foreach (var prefab in uiGroup.Prefabs)
                {
                    if (prefab == null)
                    {
                        Debug.LogError($"[UIRegistry] Prefab {prefab.name} is null");
                        continue;
                    }
                    
                    var type = prefab.GetType();
                    if (_lookup.ContainsKey(type))
                    {
                        Debug.LogError($"[UIRegistry] UI group {prefab.name} 已被注册");
                        continue;
                    }
                    
                    _lookup[type] = new UIResolvedEntry(prefab, uiGroup.Layer);
                }
            }
        }

        public bool TryGetUI<T>(out UIResolvedEntry entry) where T : UIBase
        {
            return _lookup.TryGetValue(typeof(T), out entry);
        }
    }
}
