using Gameplay.Player.Interaction;
using UnityEngine;

namespace Gameplay.Interaction.Items
{
    public class TestSphere : MonoBehaviour, IInteractable
    {
        public string Prompt { get; } = "RED BALL";
        public bool CanInteract(PlayerContext ctx)
        {
            return true;
        }

        public void Interact(PlayerContext ctx)
        {
            Debug.Log(Prompt);
        }
    }
}
