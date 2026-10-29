using UnityEngine;

namespace Gameplay.Player
{
    public class PlayerPulse
    {
        private readonly Transform _player;

        public PlayerPulse(Transform player)
        {
            _player = player;
        }
        public void Fire()
        {
            if (ScanRendererFeature.Instance != null)
                ScanRendererFeature.Instance.TriggerPulse(_player.position);   // 从玩家位置发射
        }
    }
}
