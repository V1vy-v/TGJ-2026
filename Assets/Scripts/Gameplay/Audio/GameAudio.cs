using Framework.Bootstrap;
using Framework.Services.Audio;
using Framework.Services.SceneFlow;
using UnityEngine;

namespace Gameplay.Audio
{
    public interface IGameAudio
    {
        void PlayBgm(GameBgm bgm, float fade = 0.5f);
        void PlaySfx(GameSfx sfx, float volume = 1f);
        void PlaySfxAt(GameSfx sfx, Vector3 position, float volume = 1f);
        void PlayAmbient(GameSfx ambient, float fade = 0.5f);
        void StopAmbient(float fade = 0.5f);
        void PlayFootstep();
    }

    /// <summary>
    /// Gameplay 音频映射：把 GameBgm / GameSfx 映射到框架 IAudioService。
    /// 不反向引用任何 Gameplay 业务逻辑。
    /// </summary>
    [DefaultExecutionOrder(-900)]
    public class GameAudio : MonoBehaviour, IGameAudio
    {
        [SerializeField] private GameAudioConfig config;

        private IAudioService _audio;
        private ISceneFlowService _sceneFlow;
        private int _footstepIndex;

        private void Awake()
        {
            if (GameRoot.Instance == null)
                return;

            if (GameRoot.Instance.Services.TryGet<IGameAudio>(out _))
            {
                enabled = false;
                return;
            }

            _audio = GameRoot.Instance.Services.Get<IAudioService>();
            _sceneFlow = GameRoot.Instance.Services.Get<ISceneFlowService>();

            if (_audio == null)
                return;

            GameRoot.Instance.Services.Register<IGameAudio>(this);

            _sceneFlow.LoadCompleted += OnSceneLoaded;
        }

        private void Start()
        {
            PlayBgmForScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        private void OnDestroy()
        {
            if (_sceneFlow != null)
                _sceneFlow.LoadCompleted -= OnSceneLoaded;
        }

        public void PlayBgm(GameBgm bgm, float fade = 0.5f)
        {
            if (_audio == null || config == null)
                return;

            _audio.PlayBgm(config.GetBgm(bgm), fade);
        }

        public void PlaySfx(GameSfx sfx, float volume = 1f)
        {
            if (_audio == null || config == null)
                return;

            _audio.PlaySfx(config.GetSfx(sfx), volume);
        }

        public void PlaySfxAt(GameSfx sfx, Vector3 position, float volume = 1f)
        {
            if (_audio == null || config == null)
                return;

            _audio.PlaySfxAt(config.GetSfx(sfx), position, volume);
        }

        public void PlayAmbient(GameSfx ambient, float fade = 0.5f)
        {
            if (_audio == null || config == null)
                return;

            _audio.PlayAmbient(config.GetSfx(ambient), fade);
        }

        public void StopAmbient(float fade = 0.5f)
        {
            _audio?.StopAmbient(fade);
        }

        public void PlayFootstep()
        {
            PlaySfx(_footstepIndex++ % 2 == 0 ? GameSfx.Footstep : GameSfx.FootstepAlt);
        }

        private void OnSceneLoaded(string sceneName)
        {
            PlayBgmForScene(sceneName);
        }

        private void PlayBgmForScene(string sceneName)
        {
            if (sceneName == "GameScene")
                PlayBgm(GameBgm.Forest);
            else
                PlayBgm(GameBgm.Ui);
        }
    }
}
