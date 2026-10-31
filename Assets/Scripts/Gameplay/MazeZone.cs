using Gameplay.Player;
using UnityEngine;

namespace Gameplay
{
    public class MazeZone : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player != null)
                player.EnterMaze();
        }

        private void OnTriggerExit(Collider other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player != null)
                player.ExitMaze();
        }
    }
}