using UnityEngine;

namespace Gameplay.Player.Pulse
{
    public sealed class PlayerPulse
    {
        private readonly Transform _origin;
        private readonly PulseSettings _settings;
        private float _nextFireTime;

        public PlayerPulse(Transform origin, PulseSettings settings)
        {
            _origin = origin;
            _settings = settings;
        }

        public bool CanFire => Time.time >= _nextFireTime;

        public void Fire()
        {
            if (!CanFire) return;
            if (ScanRendererFeature.Instance == null) return;

            _nextFireTime = Time.time + _settings.Cooldown;

            Vector3 origin = _origin.position + Vector3.up * 1.6f;
            ScanRendererFeature.Instance.Play(origin, _settings);
        }
    }
}