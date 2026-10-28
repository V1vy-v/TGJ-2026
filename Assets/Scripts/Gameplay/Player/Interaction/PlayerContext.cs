using UnityEngine;

namespace Gameplay.Player.Interaction
{
    public sealed class PlayerContext
    {
        public Transform Transform { get; }
        public PlayerContext(Transform transform)
        {
            Transform = transform;
        }
    }
}