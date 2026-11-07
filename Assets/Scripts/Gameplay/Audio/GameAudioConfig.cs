using UnityEngine;

namespace Gameplay.Audio
{
    public enum GameBgm
    {
        Ui,
        Forest,
        ForestAlt,
        City,
        Wrong,
        WrongAlt,
    }

    public enum GameSfx
    {
        Pulse,
        PulseAlt,
        Pickup,
        Water,
        Footstep,
        FootstepAlt,
        UiClick,
        SpeakerSolved,
        EnergyInject,
    }

    [CreateAssetMenu(menuName = "Game/Audio Config", fileName = "GameAudioConfig")]
    public class GameAudioConfig : ScriptableObject
    {
        [Header("BGM")]
        [SerializeField] private AudioClip ui;
        [SerializeField] private AudioClip forest;
        [SerializeField] private AudioClip forestAlt;
        [SerializeField] private AudioClip city;
        [SerializeField] private AudioClip wrong;
        [SerializeField] private AudioClip wrongAlt;

        [Header("SFX")]
        [SerializeField] private AudioClip pulse;
        [SerializeField] private AudioClip pulseAlt;
        [SerializeField] private AudioClip pickup;
        [SerializeField] private AudioClip water;
        [SerializeField] private AudioClip footstep;
        [SerializeField] private AudioClip footstepAlt;
        [SerializeField] private AudioClip uiClick;
        [SerializeField] private AudioClip speakerSolved;
        [SerializeField] private AudioClip energyInject;

        public AudioClip GetBgm(GameBgm id)
        {
            return id switch
            {
                GameBgm.Ui => ui,
                GameBgm.Forest => forest,
                GameBgm.ForestAlt => forestAlt,
                GameBgm.City => city,
                GameBgm.Wrong => wrong,
                GameBgm.WrongAlt => wrongAlt,
                _ => null,
            };
        }

        public AudioClip GetSfx(GameSfx id)
        {
            return id switch
            {
                GameSfx.Pulse => pulse,
                GameSfx.PulseAlt => pulseAlt,
                GameSfx.Pickup => pickup,
                GameSfx.Water => water,
                GameSfx.Footstep => footstep,
                GameSfx.FootstepAlt => footstepAlt,
                GameSfx.UiClick => uiClick,
                GameSfx.SpeakerSolved => speakerSolved,
                GameSfx.EnergyInject => energyInject,
                _ => null,
            };
        }
    }
}
