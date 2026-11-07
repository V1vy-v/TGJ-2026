using Gameplay.Player;
using UnityEngine;

namespace Gameplay.Flow
{
    public enum StoryTriggerMode
    {
        Collision,
        Event,
    }

    /// <summary>
    /// Story 触发入口：
    /// Collision：由 Trigger Collider 检测玩家；
    /// Event：由代码或 UnityEvent 调 Trigger()。
    /// </summary>
    public class StoryTrigger : MonoBehaviour
    {
        #region 配置

        [SerializeField] private StoryTriggerMode mode = StoryTriggerMode.Collision;
        [SerializeField] private TextAsset storyScript;
        [SerializeField] private bool oneShot = true;

        [Tooltip("Event 模式下是否在 Start 时自动触发一次；用于第一次进入游戏场景")]
        [SerializeField] private bool triggerOnStart;

        [Tooltip("剧情结束后自动播放的 Tips key，可留空")]
        [SerializeField] private string tipsAfterStory;

        #endregion

        #region 状态

        private bool _done;
        private PlayerController _player;

        #endregion

        #region 生命周期

        private void Start()
        {
            if (triggerOnStart && mode == StoryTriggerMode.Event)
                Trigger();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (mode != StoryTriggerMode.Collision)
                return;

            var player = other.GetComponentInParent<PlayerController>();
            if (player == null)
                return;

            Trigger(player);
        }

        #endregion

        #region 对外接口

        public void Trigger()
        {
            var player = Object.FindFirstObjectByType<PlayerController>();
            if (player == null)
            {
                Debug.LogWarning("[StoryTrigger] 场景里没有 PlayerController", this);
                return;
            }

            Trigger(player);
        }

        public void Trigger(PlayerController player)
        {
            if (_done || storyScript == null || player == null)
                return;

            if (!player.StartStory(storyScript))
                return;

            if (oneShot)
                _done = true;

            if (string.IsNullOrWhiteSpace(tipsAfterStory))
                return;

            _player = player;
            _player.Dialogue.Ended += OnStoryEnded;
        }

        public void ResetTrigger()
        {
            _done = false;
        }

        #endregion

        #region 内部实现

        private void OnStoryEnded()
        {
            Unsubscribe();
            TipsTrigger.Show(tipsAfterStory);
        }

        private void Unsubscribe()
        {
            if (_player == null)
                return;

            _player.Dialogue.Ended -= OnStoryEnded;
            _player = null;
        }

        [ContextMenu("Trigger Story")]
        private void TriggerFromContextMenu()
        {
            Trigger();
        }

        #endregion
    }
}
