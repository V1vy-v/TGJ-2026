using UnityEngine;

namespace Gameplay.Audio
{
    public enum GameBgm
    {
        Ui,
        Forest,
        Maze,
        Stream,
        City,
        GameOver,
    }

    public enum GameSfx
    {
        Pulse,
        Pickup,
        Water,
        Footstep,
        UiClick,
        SpeakerSolved,
        EnergyInject,
    }

    [CreateAssetMenu(menuName = "Game/Audio Config", fileName = "GameAudioConfig")]
    public class GameAudioConfig : ScriptableObject
    {
        [Header("场景 / 区域 BGM")]
        [SerializeField] private AudioClip ui;
        [SerializeField] private AudioClip forest;
        [SerializeField] private AudioClip maze;
        [SerializeField] private AudioClip stream;
        [SerializeField] private AudioClip city;
        [SerializeField] private AudioClip gameOver;

        [Header("SFX")]
        [SerializeField] private AudioClip pulse;
        [SerializeField] private AudioClip pickup;
        [SerializeField] private AudioClip water;
        [SerializeField] private AudioClip footstep;
        [SerializeField] private AudioClip uiClick;
        [SerializeField] private AudioClip speakerSolved;
        [SerializeField] private AudioClip energyInject;

        public AudioClip GetBgm(GameBgm id)
        {
            return id switch
            {
                GameBgm.Ui => ui,
                GameBgm.Forest => forest,
                GameBgm.Maze => maze,
                GameBgm.Stream => stream,
                GameBgm.City => city,
                GameBgm.GameOver => gameOver,
                _ => null,
            };
        }

        public AudioClip GetSfx(GameSfx id)
        {
            return id switch
            {
                GameSfx.Pulse => pulse,
                GameSfx.Pickup => pickup,
                GameSfx.Water => water,
                GameSfx.Footstep => footstep,
                GameSfx.UiClick => uiClick,
                GameSfx.SpeakerSolved => speakerSolved,
                GameSfx.EnergyInject => energyInject,
                _ => null,
            };
        }
    }
}
