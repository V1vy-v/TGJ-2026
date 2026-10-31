
using System.Collections.Generic;
using Gameplay.Interaction.Interface;
using Gameplay.Player.Interaction;

namespace Gameplay.Interaction.NPC
{
    public class Npc:InteractableBehaviour
    {
        public override IReadOnlyList<InteractionOption> GetOptions(PlayerContext ctx)
        {
            return new List<InteractionOption>();
        }
    }
}