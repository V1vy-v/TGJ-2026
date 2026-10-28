using System;
using System.Collections.Generic;
using Gameplay.Player.Interaction;

namespace Gameplay.Interaction
{
    public interface IMultiOptionInteractable
    {
        IReadOnlyList<InteractionOption> GetOptions(PlayerContext ctx);
    }

    public readonly struct InteractionOption
    {
        public string Prompt { get; }
        public Action<PlayerContext> Execute { get; }
    }
}