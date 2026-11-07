using System;
using System.Collections.Generic;
using Framework.Bootstrap;
using Gameplay.Audio;
using Gameplay.Interaction.Interface;
using Gameplay.Player.Interaction;
using UnityEngine;

namespace Gameplay.Interaction.Puzzle.Energy
{
    public class EnergyDevice : InteractableBehaviour
    {
        [Header("能量")]
        [SerializeField] [Tooltip("能量上限")] private int maxEnergy = 6;
        [SerializeField] [Tooltip("能量下限")] private int minEnergy = 0;
        [SerializeField] [Tooltip("初始能量")] private int initialEnergy = 0;

        [Header("是否可交互")]
        [SerializeField] private bool isAdjustable = true;

        private int _energy;
        private bool _locked;

        public int Energy => _energy;

        public event Action EnergyChanged;

        private void Awake()
        {
            _energy = Mathf.Clamp(initialEnergy, minEnergy, maxEnergy);
        }

        public override bool CanInteract(PlayerContext ctx)
        {
            return base.CanInteract(ctx) && isAdjustable && !_locked;
        }

        public override IReadOnlyList<InteractionOption> GetOptions(PlayerContext ctx)
        {
            if (!isAdjustable || _locked)
                return Array.Empty<InteractionOption>();

            var localization = GameRoot.Instance.Services.Get<Framework.Services.Localization.ILocalizationService>();
            return new[]
            {
                Option(localization.GetText("interaction.injectOne"), _ => TryReceiveEnergy(ctx, 1)),
                Option(localization.GetText("interaction.recoverOne"), _ => TryReleaseEnergy(ctx, 1)),
            };
        }

        public void SetLock(bool locked)
        {
            _locked = locked;
        }

        private bool TryReceiveEnergy(PlayerContext ctx, int amount)
        {
            if (_energy + amount > maxEnergy)
                return false;

            if (!ctx.Energy.TrySpend(amount))
                return false;

            _energy += amount;
            EnergyChanged?.Invoke();
            GameRoot.Instance.Services.Get<IGameAudio>().PlaySfx(GameSfx.EnergyInject);
            return true;
        }

        private bool TryReleaseEnergy(PlayerContext ctx, int amount)
        {
            if (_energy - amount < minEnergy)
                return false;

            if (!ctx.Energy.TryRecycle(amount))
                return false;

            _energy -= amount;
            EnergyChanged?.Invoke();
            GameRoot.Instance.Services.Get<IGameAudio>().PlaySfx(GameSfx.EnergyInject);
            return true;
        }
    }
}
