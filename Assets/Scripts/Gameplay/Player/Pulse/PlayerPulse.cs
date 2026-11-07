using Framework.Bootstrap;
using Gameplay.Audio;
using UnityEngine;

namespace Gameplay.Player.Pulse
{
    public sealed class PlayerPulse
    {
        private readonly Transform _origin;
        private readonly PulseSettings _settings;
        private readonly IGameAudio _gameAudio;
        private float _nextFireTime;

        public PlayerPulse(Transform origin, PulseSettings settings, IGameAudio gameAudio)
        {
            _origin = origin;
            _settings = settings;
            _gameAudio = gameAudio;
        }

        public bool CanFire => Time.time >= _nextFireTime;

        public void Fire()
        {
            if (!CanFire) return;
            if (ScanRendererFeature.Instance == null) return;

            _nextFireTime = Time.time + _settings.Cooldown;

            Vector3 origin = _origin.position + Vector3.up * 1.6f;
            ScanRendererFeature.Instance.Play(origin, _settings);
            _gameAudio?.PlaySfx(GameSfx.Pulse);
        }
    }
}