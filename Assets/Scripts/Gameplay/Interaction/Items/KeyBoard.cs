using System;
using System.Collections.Generic;
using Framework.Bootstrap;
using Framework.Services.Localization;
using Gameplay.Audio;
using Gameplay.Flow;
using Gameplay.Interaction.Interface;
using Gameplay.Player.Interaction;
using UnityEngine;

namespace Gameplay.Interaction.Items
{
    public class KeyBoard : InteractableBehaviour
    {
        [SerializeField] private int energy = 3;
        [SerializeField] private string pickupTipsKey;
        private string _interactionText;
        private bool _isGained = false;
        public event Action ViolinGained;

        private void Start()
        {
            _interactionText = 
                GameRoot.Instance.Services.Get<ILocalizationService>().GetText(
                    "interaction.Pickup");
        }

        public override bool CanInteract(PlayerContext ctx)
        {
            return base.CanInteract(ctx) && !_isGained;
        }

        public override IReadOnlyList<InteractionOption> GetOptions(PlayerContext ctx)
        {
            return SingleOption(_interactionText, TryPickUp);
        }

        private bool TryPickUp(PlayerContext ctx)
        {
            if(!ctx.Energy.TryRecycle(energy))
                return false;
            ViolinGained?.Invoke();
            _isGained = true;
            ctx.Player.NotifyKeyboardAcquired();
            ctx.Player.NotifyItemPickedUp();
            gameObject.SetActive(false);

            GameRoot.Instance.Services.Get<IGameAudio>().PlaySfx(GameSfx.Pickup);
            TipsTrigger.Show(pickupTipsKey);

            return true;
        }
    }
}
