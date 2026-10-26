
using System;

namespace Framework.Services.Assets
{
    public class AssetHandle<T>: IDisposable
    {
        private bool _disposed;
        private readonly IAssetService _service;
        public string Address { get; }
        public T Asset { get;private set; }
        
        public AssetHandle(T asset, string address,IAssetService service)
        {
            Asset = asset;
            Address = address;
            _service = service;
        }
        
        public void Dispose()
        {
            if(_disposed)return;
            _disposed = true;
            _service.Release(Address);
        }
    }
}
