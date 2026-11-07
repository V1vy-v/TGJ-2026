using System.Collections.Generic;
using Framework.Bootstrap;
using Gameplay.Flow;
using Gameplay.Interaction;
using Gameplay.Interaction.Interface;
using Gameplay.Player;
using Gameplay.Player.Interaction;
using UnityEngine;

public class Door2 : InteractableBehaviour
{
    [SerializeField] private string lockedTipsKey = "tips.door";
    [SerializeField] private string openTipsKey = "";

    private bool _open;

    public override bool CanInteract(PlayerContext ctx)
    {
        return base.CanInteract(ctx) && !_open;
    }

    public override IReadOnlyList<InteractionOption> GetOptions(PlayerContext ctx)
    {
        var localization = GameRoot.Instance.Services.Get<Framework.Services.Localization.ILocalizationService>();
        return SingleOption(localization.GetText("interaction.openDoor"), TryOpen);
    }

    private bool TryOpen(PlayerContext ctx)
    {
        if (_open || ctx.Player == null)
            return false;

        if (!ctx.Player.HasMazeKey)
        {
            TipsTrigger.Show(lockedTipsKey);
            return true;
        }

        _open = true;
        if (!string.IsNullOrWhiteSpace(openTipsKey))
            TipsTrigger.Show(openTipsKey);

        gameObject.SetActive(false);
        return true;
    }
}
