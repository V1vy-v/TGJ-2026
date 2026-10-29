using Gameplay.Interaction;
using Gameplay.Interaction.Interface;

namespace Gameplay.Player.Interaction
{
    public readonly struct InteractionCandidate
    {
        public IInteractable Target { get; }
        public InteractionOption Option { get; }
        public float Distance { get; }

        public InteractionCandidate(IInteractable target, InteractionOption option, float distance)
        {
            Target = target;
            Option = option;
            Distance = distance;
        }
    }
}