using Framework.Bootstrap;
using Gameplay.Player;
using UnityEngine;

namespace Gameplay.Audio
{
    /// <summary>
    /// 区域音乐：玩家进入区域时切换 BGM，可选播放环境音。
    /// GameScene 默认 BGM 为森林，其他区域用该组件覆盖。
    /// </summary>
    public class GameAudioZone : MonoBehaviour
    {
        [SerializeField] private GameBgm bgm = GameBgm.Forest;
        [SerializeField] private bool setAmbient;
        [SerializeField] private GameSfx ambient = GameSfx.Water;

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerController>() == null)
                return;

            if (GameRoot.Instance == null)
                return;

            var audio = GameRoot.Instance.Services.Get<IGameAudio>();
            audio.PlayBgm(bgm);

            if (setAmbient)
                audio.PlayAmbient(ambient);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!setAmbient)
                return;

            if (other.GetComponentInParent<PlayerController>() == null)
                return;

            if (GameRoot.Instance == null)
                return;

            GameRoot.Instance.Services.Get<IGameAudio>().StopAmbient();
        }
    }
}
