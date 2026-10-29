using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gameplay.Player
{
    [Serializable]
    public struct VisionSettings
    {
        public float normalFov;
        public float blindFov;
        public float normalRadius;
        public float blindRadius;
        public float normalLightIntensity;
        public float blindLightIntensity;
        public float normalLightRange;
        public float blindLightRange;
        public float normalAmbient;
        public float blindAmbient;
        public float normalFogDensity;
        public float blindFogDensity;
        public float transitionDuration;
    }

    public sealed class PlayerVision : IDisposable
    {
        private readonly Light _selfLight;
        private readonly Camera _mainCamera;
        private readonly CinemachineCamera _camera;
        private readonly CinemachineOrbitalFollow _orbital;
        private readonly PlayerGlasses _glasses;
        private readonly VisionSettings _settings;

        private readonly Light[] _sceneLights;
        private readonly float[] _sceneLightIntensities;

        private readonly CameraClearFlags _originalClearFlags;
        private readonly Color _originalBackground;
        private readonly AmbientMode _originalAmbientMode;
        private readonly Color _originalAmbientLight;
        private readonly float _originalAmbientIntensity;
        private readonly float _originalFogDensity;
        private readonly Color _originalFogColor;

        private float _blend;

        public PlayerVision(Light selfLight, Camera mainCamera, CinemachineCamera camera,
            CinemachineOrbitalFollow orbital, PlayerGlasses glasses, VisionSettings settings)
        {
            _selfLight = selfLight;
            _mainCamera = mainCamera;
            _camera = camera;
            _orbital = orbital;
            _glasses = glasses;
            _settings = settings;

            _sceneLights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            _sceneLightIntensities = new float[_sceneLights.Length];

            for (int i = 0; i < _sceneLights.Length; i++)
            {
                _sceneLightIntensities[i] = _sceneLights[i] == _selfLight
                    ? 0f
                    : _sceneLights[i].intensity;
            }

            _originalClearFlags = _mainCamera.clearFlags;
            _originalBackground = _mainCamera.backgroundColor;

            _originalAmbientMode = RenderSettings.ambientMode;
            _originalAmbientLight = RenderSettings.ambientLight;
            _originalAmbientIntensity = RenderSettings.ambientIntensity;
            _originalFogDensity = RenderSettings.fogDensity;
            _originalFogColor = RenderSettings.fogColor;

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
            var lens = _camera.Lens;
            lens.FieldOfView = Mathf.Lerp(_settings.normalFov, _settings.blindFov, t);
            _camera.Lens = lens;

            _orbital.Radius = Mathf.Lerp(_settings.normalRadius, _settings.blindRadius, t);

            _selfLight.intensity = Mathf.Lerp(
                _settings.normalLightIntensity, _settings.blindLightIntensity, t);
            _selfLight.range = Mathf.Lerp(
                _settings.normalLightRange, _settings.blindLightRange, t);
            _selfLight.enabled = _selfLight.intensity > 0.001f;

            for (int i = 0; i < _sceneLights.Length; i++)
            {
                if (_sceneLights[i] == _selfLight) continue;
                if (_sceneLights[i] == null) continue;

                _sceneLights[i].intensity = Mathf.Lerp(_sceneLightIntensities[i], 0f, t);
            }

            if (t > 0.5f)
            {
                _mainCamera.clearFlags = CameraClearFlags.SolidColor;
                _mainCamera.backgroundColor = Color.black;

                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = Color.black;
                RenderSettings.ambientIntensity = 0f;
            }
            else
            {
                _mainCamera.clearFlags = _originalClearFlags;
                _mainCamera.backgroundColor = _originalBackground;

                RenderSettings.ambientMode = _originalAmbientMode;
                RenderSettings.ambientLight = _originalAmbientLight;
                RenderSettings.ambientIntensity = Mathf.Lerp(
                    _originalAmbientIntensity, _settings.blindAmbient, t);
            }

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = Color.Lerp(_originalFogColor, Color.black, t);
            RenderSettings.fogDensity = Mathf.Lerp(
                Mathf.Max(_originalFogDensity, _settings.normalFogDensity),
                _settings.blindFogDensity,
                t);
        }
    }
}
