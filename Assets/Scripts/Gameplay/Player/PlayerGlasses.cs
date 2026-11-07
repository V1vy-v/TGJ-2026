using System;

namespace Gameplay.Player
{
    public sealed class PlayerGlasses : IDisposable
    {
        public bool IsWearing { get; private set; } = true;

        public bool ToggleEnabled { get; set; } = true;
        public event Action<bool> Changed;

        public void Toggle()
        {
            if (!ToggleEnabled) return;
            SetWearing(!IsWearing);
        }

        public void Dispose()
        {
        }

        public void SetWearing(bool wearing)
        {
            if (IsWearing == wearing) return;
            IsWearing = wearing;
            Changed?.Invoke(IsWearing);
        }
    }
}
