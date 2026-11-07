using System.Collections;
using System.Collections.Generic;
using Framework.Core;
using UnityEngine;
using UnityEngine.Audio;

namespace Framework.Services.Audio
{
    /// <summary>
    /// 通用音频服务：BGM 淡入淡出、环境音、SFX 池。
    /// 不引用任何 Gameplay 类型。
    /// </summary>
    public sealed class AudioManager : IManager, IAudioService
    {
        #region 状态

        public int Order => 20;

        private readonly AudioMixer _mixer;
        private MonoBehaviour _host;
        private Transform _root;

        private AudioSource _bgmSource;
        private AudioSource _ambientSource;
        private readonly List<AudioSource> _sfxSources = new();
        private int _sfxIndex;

        private AudioClip _bgmClip;
        private AudioClip _ambientClip;
        private Coroutine _bgmRoutine;
        private Coroutine _ambientRoutine;

        private float _masterVolume = 1f;
        private float _bgmVolume = 0.5f;
        private float _sfxVolume = 0.5f;
        private float _ambientVolume = 0.5f;

        #endregion

        #region 生命周期

        public AudioManager(AudioMixer mixer = null)
        {
            _mixer = mixer;
        }

        public void Initialize(ServiceRegistry services, MonoBehaviour host)
        {
            _host = host;

            var root = new GameObject("[Audio]");
            root.transform.SetParent(host.transform, false);
            _root = root.transform;

            _bgmSource = CreateSource("BGM", 0f, true);
            _ambientSource = CreateSource("Ambient", 0f, true);

            for (int i = 0; i < 12; i++)
                _sfxSources.Add(CreateSource($"SFX_{i}", 0f, false));

            services.Register<IAudioService>(this);
        }

        public void Shutdown()
        {
            if (_bgmRoutine != null && _host != null)
                _host.StopCoroutine(_bgmRoutine);

            if (_ambientRoutine != null && _host != null)
                _host.StopCoroutine(_ambientRoutine);

            if (_root != null)
                Object.Destroy(_root.gameObject);

            _root = null;
            _bgmSource = null;
            _ambientSource = null;
            _sfxSources.Clear();
        }

        #endregion

        #region 对外接口

        public void PlayBgm(AudioClip clip, float fade = 0.5f, bool loop = true)
        {
            if (clip == null)
            {
                StopBgm(fade);
                return;
            }

            if (_bgmClip == clip && _bgmSource != null && _bgmSource.isPlaying)
                return;

            _bgmClip = clip;

            if (_bgmRoutine != null && _host != null)
                _host.StopCoroutine(_bgmRoutine);

            if (_host != null)
                _bgmRoutine = _host.StartCoroutine(SwitchBgm(clip, fade, loop));
        }

        public void StopBgm(float fade = 0.5f)
        {
            _bgmClip = null;

            if (_bgmRoutine != null && _host != null)
                _host.StopCoroutine(_bgmRoutine);

            if (_host != null)
                _bgmRoutine = _host.StartCoroutine(FadeOut(_bgmSource, fade, true));
        }

        public void PlaySfx(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null || _sfxSources.Count == 0)
                return;

            AudioSource source = _sfxSources[_sfxIndex];
            _sfxIndex = (_sfxIndex + 1) % _sfxSources.Count;

            source.pitch = pitch;
            source.spatialBlend = 0f;
            source.volume = volume * _sfxVolume;
            source.PlayOneShot(clip);
        }

        public void PlaySfxAt(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null || _sfxSources.Count == 0)
                return;

            AudioSource source = _sfxSources[_sfxIndex];
            _sfxIndex = (_sfxIndex + 1) % _sfxSources.Count;

            source.transform.position = position;
            source.spatialBlend = 1f;
            source.volume = volume * _sfxVolume;
            source.PlayOneShot(clip);
        }

        public void PlayAmbient(AudioClip clip, float fade = 0.5f)
        {
            if (clip == null)
            {
                StopAmbient(fade);
                return;
            }

            if (_ambientClip == clip && _ambientSource != null && _ambientSource.isPlaying)
                return;

            _ambientClip = clip;

            if (_ambientRoutine != null && _host != null)
                _host.StopCoroutine(_ambientRoutine);

            if (_host != null)
                _ambientRoutine = _host.StartCoroutine(SwitchAmbient(clip, fade));
        }

        public void StopAmbient(float fade = 0.5f)
        {
            _ambientClip = null;

            if (_ambientRoutine != null && _host != null)
                _host.StopCoroutine(_ambientRoutine);

            if (_host != null)
                _ambientRoutine = _host.StartCoroutine(FadeOut(_ambientSource, fade, true));
        }

        public void SetChannelVolume(AudioChannel channel, float volume)
        {
            volume = Mathf.Clamp01(volume);

            switch (channel)
            {
                case AudioChannel.Master:
                    _masterVolume = volume;
                    AudioListener.volume = volume;
                    break;
                case AudioChannel.Bgm:
                    _bgmVolume = volume;
                    if (_bgmSource != null) _bgmSource.volume = volume;
                    break;
                case AudioChannel.Sfx:
                    _sfxVolume = volume;
                    break;
                case AudioChannel.Ambient:
                    _ambientVolume = volume;
                    if (_ambientSource != null) _ambientSource.volume = volume;
                    break;
            }
        }

        #endregion

        #region 内部实现

        private AudioSource CreateSource(string sourceName, float spatialBlend, bool loop)
        {
            var go = new GameObject(sourceName);
            go.transform.SetParent(_root, false);

            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = spatialBlend;
            source.volume = sourceName == "BGM" ? _bgmVolume : _ambientVolume;

            return source;
        }

        private IEnumerator SwitchBgm(AudioClip clip, float fade, bool loop)
        {
            yield return FadeOut(_bgmSource, fade, false);

            _bgmSource.clip = clip;
            _bgmSource.loop = loop;
            _bgmSource.volume = 0f;
            _bgmSource.Play();

            yield return FadeIn(_bgmSource, _bgmVolume, fade);
            _bgmRoutine = null;
        }

        private IEnumerator SwitchAmbient(AudioClip clip, float fade)
        {
            yield return FadeOut(_ambientSource, fade, false);

            _ambientSource.clip = clip;
            _ambientSource.loop = true;
            _ambientSource.volume = 0f;
            _ambientSource.Play();

            yield return FadeIn(_ambientSource, _ambientVolume, fade);
            _ambientRoutine = null;
        }

        private IEnumerator FadeOut(AudioSource source, float duration, bool stop)
        {
            if (source == null) yield break;

            float start = source.volume;
            float time = 0f;

            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(start, 0f, time / Mathf.Max(duration, 0.01f));
                yield return null;
            }

            source.volume = 0f;
            if (stop) source.Stop();
        }

        private IEnumerator FadeIn(AudioSource source, float target, float duration)
        {
            float time = 0f;
            source.volume = 0f;

            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(0f, target, time / Mathf.Max(duration, 0.01f));
                yield return null;
            }

            source.volume = target;
        }

        #endregion
    }
}
