using System;

namespace Framework.Services.Input
{
    internal sealed class MapScope : IDisposable
    {
        private InputManager _owner;
        private string _mapName;
        private bool _disposed;

        public MapScope(InputManager owner, string mapName)
        {
            _owner = owner;
            _mapName = mapName;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _owner.ReleaseMap(_mapName);
        }
    }
}
