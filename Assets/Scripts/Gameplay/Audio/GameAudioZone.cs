using Framework.Bootstrap;
using Gameplay.Player;
using UnityEngine;

namespace Gameplay.Audio
{
    /// <summary>
    /// 区域 BGM：放在道路/关卡区域上，玩家进入时切换音乐。
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
