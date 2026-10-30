using UnityEngine;

namespace Gameplay.Player.Interaction
{
    public sealed class PlayerContext
    {
        public Transform Transform { get; }
        public PlayerEnergy Energy { get; }
        public PlayerContext(Transform transform, PlayerEnergy energy)
        {
            Transform = transform;
            Energy = energy;
        }
    }
}