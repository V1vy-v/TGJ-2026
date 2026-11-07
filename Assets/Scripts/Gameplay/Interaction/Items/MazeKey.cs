using System.Collections.Generic;
using Framework.Bootstrap;
using Gameplay.Audio;
using Gameplay.Flow;
using Gameplay.Interaction;
using Gameplay.Interaction.Interface;
using Gameplay.Player.Interaction;
using UnityEngine;

public class MazeKey : InteractableBehaviour
{
    [SerializeField] private string pickupTipsKey = "tips.Pickup";

    private bool _taken;

    public override bool CanInteract(PlayerContext ctx)
    {
        return base.CanInteract(ctx) && !_taken;
    }

    public override IReadOnlyList<InteractionOption> GetOptions(PlayerContext ctx)
    {
        var localization = GameRoot.Instance.Services.Get<Framework.Services.Localization.ILocalizationService>();
        return SingleOption(localization.GetText(pickupTipsKey), TryPickUp);
    }

    private bool TryPickUp(PlayerContext ctx)
    {
        if (_taken || ctx.Player == null)
            return false;

        _taken = true;
        ctx.Player.CollectMazeKey();

        if (GameRoot.Instance.Services.TryGet<IGameAudio>(out var audio))
            audio.PlaySfx(GameSfx.Pickup);

        TipsTrigger.Show(pickupTipsKey);
        gameObject.SetActive(false);
        return true;
    }
}
