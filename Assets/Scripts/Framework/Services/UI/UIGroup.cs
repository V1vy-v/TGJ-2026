using System.Collections.Generic;
using UnityEngine;

namespace Framework.Services.UI
{
    [System.Serializable]
    public class UIGroup
    {
        [SerializeField] private UILayer layer;
        public UILayer Layer => layer;
        [SerializeField] private List<UIBase> prefabs;
        public List<UIBase> Prefabs => prefabs;
    }
}