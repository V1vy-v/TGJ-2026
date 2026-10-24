using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

namespace Framework.Bootstrap
{
    /// <summary>启动配置：框架需要的资源引用集中在这里，GameRoot 注入给管理器。</summary>
    [CreateAssetMenu(menuName = "Framework/Bootstrap Config", fileName = "BootstrapConfig")]
    public class BootstrapConfig : ScriptableObject
    {
        [SerializeField][Header("输入系统配置")]
        private InputActionAsset _inputAsset;
        public InputActionAsset InputAsset => _inputAsset;

        [SerializeField][Header("音频系统配置")]
        private AudioMixer _mainAudioMixer;
        public AudioMixer MainAudioMixer => _mainAudioMixer;
    }
}