using UnityEngine;

namespace Framework.Services.UI
{
    public class UIRoot : MonoBehaviour
    {
        [SerializeField] private Transform screenLayer;
        [SerializeField] private Transform panelLayer;
        [SerializeField] private Transform popupLayer;
        [SerializeField] private Transform overlayLayer;
        [SerializeField] private GameObject popupBlocker;

        public Transform GetLayerRoot(UILayer layer) => layer switch
        {
            UILayer.Screen  => screenLayer,
            UILayer.Panel   => panelLayer,
            UILayer.Popup   => popupLayer,
            UILayer.Overlay => overlayLayer,
            _ => null,
        };

        public GameObject PopupBlocker => popupBlocker;
    }
}