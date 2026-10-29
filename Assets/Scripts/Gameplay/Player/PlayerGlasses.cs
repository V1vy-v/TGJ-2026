using System;
using Gameplay.Player.Input;

namespace Gameplay.Player
{
    public sealed class PlayerGlasses : IDisposable
    {
        private readonly PlayerInputReader _reader;

        public bool IsWearing { get; private set; } = true;
        public event Action<bool> Changed;

        public PlayerGlasses(PlayerInputReader reader)
        {
            _reader = reader;
            _reader.GlassesPressed += Toggle;
        }

        public void Toggle()
        {
            IsWearing = !IsWearing;
            Changed?.Invoke(IsWearing);
        }

        public void Dispose()
        {
            _reader.GlassesPressed -= Toggle;
        }
    }
}