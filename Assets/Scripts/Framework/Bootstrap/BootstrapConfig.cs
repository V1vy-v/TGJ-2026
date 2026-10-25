using Framework.Services.UI;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Framework.Bootstrap
{
    /// <summary>启动配置：框架需要的资源引用集中在这里，GameRoot 注入给管理器。</summary>
    [CreateAssetMenu(menuName = "Framework/Bootstrap Config", fileName = "BootstrapConfig")]
    public class BootstrapConfig : ScriptableObject
    {
        [FormerlySerializedAs("_inputAsset")]
        [SerializeField][Header("输入系统配置")]
        private InputActionAsset inputAsset;
        public InputActionAsset InputAsset => inputAsset;

        [FormerlySerializedAs("_mainAudioMixer")]
        [SerializeField][Header("音频系统配置")]
        private AudioMixer mainAudioMixer;
        public AudioMixer MainAudioMixer => mainAudioMixer;
        
        [FormerlySerializedAs("_uiRoot")]
        [SerializeField][Header("UI根节点预制体")]
        private GameObject uiRoot;
        public GameObject UIRoot => uiRoot;

        [SerializeField][Header("UI注册表")]
        private UIRegistry uiRegistry;
        public UIRegistry UIRegistry => uiRegistry;
    }
}
