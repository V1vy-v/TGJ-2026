using System.Collections.Generic;
using Gameplay.Interaction;
using Gameplay.Interaction.Interface;
using Gameplay.Player.Interaction;
using UnityEngine;

namespace Gameplay.Interaction.Items
{
    public class TestSphere : InteractableBehaviour
    {
        public override IReadOnlyList<InteractionOption> GetOptions(PlayerContext ctx)
        {
            return SingleOption("RED BALL", _ =>
            {
                Debug.Log("RED BALL");
                return true;
            });
        }
    }
}
