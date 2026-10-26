using UnityEngine;

namespace Framework.Services.Assets
{
    public interface IAssetProvider
    {
        bool IsInitialized { get; }
        Awaitable InitializeAsync();
        Awaitable<T> LoadAsync<T>(string address) where T : Object;
        void Release(string address);
    }
}
