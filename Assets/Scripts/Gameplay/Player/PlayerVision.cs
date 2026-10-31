using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Gameplay.Player
{
    [Serializable]
    public struct VisionSettings
    {
        [Tooltip("戴镜时的相机视野角度。")]
        public float normalFov;

        [Tooltip("摘镜时的相机视野角度；通常比戴镜更窄。")]
        public float blindFov;

        [Tooltip("戴镜时第三人称相机与角色的距离。")]
        public float normalRadius;

        [Tooltip("摘镜时第三人称相机与角色的距离；通常更近。")]
        public float blindRadius;

        [Tooltip("戴镜时玩家自身光强度；0 表示关闭。")]
        public float normalLightIntensity;

        [Tooltip("摘镜时玩家自身光强度；迷宫内要纯黑时设为 0。")]
        public float blindLightIntensity;

        [Tooltip("戴镜时自身光的照射范围（米）。")]
        public float normalLightRange;

        [Tooltip("摘镜时自身光的照射范围（米）。")]
        public float blindLightRange;

        [Tooltip("戴镜/摘镜过渡的持续时间（秒）。")]
        public float transitionDuration;
    }

    /// <summary>
    /// 只负责眼镜带来的表现变化：相机 FOV、相机距离和玩家自身光。
    /// 环境光、雾、场景灯光交给其它系统。
    /// </summary>
    public sealed class PlayerVision : IDisposable
    {
        private readonly Light _selfLight;
        private readonly CinemachineCamera _camera;
        private readonly CinemachineOrbitalFollow _orbital;
        private readonly PlayerGlasses _glasses;
        private readonly VisionSettings _settings;

        private float _blend;

        public PlayerVision(Light selfLight, CinemachineCamera camera,
            CinemachineOrbitalFollow orbital, PlayerGlasses glasses, VisionSettings settings)
        {
            _selfLight = selfLight;
            _camera = camera;
            _orbital = orbital;
            _glasses = glasses;
            _settings = settings;

            Apply(0f);
        }

        public void Tick(float deltaTime)
        {
            float target = _glasses.IsWearing ? 0f : 1f;
            float speed = deltaTime / Mathf.Max(_settings.transitionDuration, 0.01f);

            _blend = Mathf.MoveTowards(_blend, target, speed);
            Apply(_blend);
        }

        public void Dispose()
        {
            // 目前没有事件订阅，预留
        }

        private void Apply(float t)
        {
            if (_camera != null)
            {
                var lens = _camera.Lens;
                lens.FieldOfView = Mathf.Lerp(_settings.normalFov, _settings.blindFov, t);
                _camera.Lens = lens;
            }

            if (_orbital != null)
            {
                _orbital.Radius = Mathf.Lerp(
                    _settings.normalRadius,
                    _settings.blindRadius,
                    t);
            }

            if (_selfLight == null)
                return;

            _selfLight.intensity = Mathf.Lerp(
                _settings.normalLightIntensity,
                _settings.blindLightIntensity,
                t);

            _selfLight.range = Mathf.Lerp(
                _settings.normalLightRange,
                _settings.blindLightRange,
                t);

            _selfLight.enabled = _selfLight.intensity > 0.001f;
        }
    }
}
