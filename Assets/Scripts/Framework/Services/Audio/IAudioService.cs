using UnityEngine;

namespace Framework.Services.Audio
{
    public enum AudioChannel
    {
        Master,
        Bgm,
        Sfx,
        Ambient,
    }

    public interface IAudioService
    {
        void PlayBgm(AudioClip clip, float fade = 0.5f, bool loop = true);
        void StopBgm(float fade = 0.5f);
        void PlaySfx(AudioClip clip, float volume = 1f, float pitch = 1f);
        void PlaySfxAt(AudioClip clip, Vector3 position, float volume = 1f);
        void PlayAmbient(AudioClip clip, float fade = 0.5f);
        void StopAmbient(float fade = 0.5f);
        void SetChannelVolume(AudioChannel channel, float volume);
    }
}
