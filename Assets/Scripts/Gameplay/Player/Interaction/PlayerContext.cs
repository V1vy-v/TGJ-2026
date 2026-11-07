using System;
using UnityEngine;

namespace Gameplay.Player.Interaction
{
    public sealed class PlayerContext
    {
        public Transform Transform { get; }
        public PlayerEnergy Energy { get; }
        public PlayerController Player { get; }
        public Func<TextAsset, bool> StartDialogue { get; }

        public PlayerContext(
            Transform transform,
            PlayerEnergy energy,
            PlayerController player,
            Func<TextAsset, bool> startDialogue)
        {
            Transform = transform;
            Energy = energy;
            Player = player;
            StartDialogue = startDialogue;
        }
    }
}
