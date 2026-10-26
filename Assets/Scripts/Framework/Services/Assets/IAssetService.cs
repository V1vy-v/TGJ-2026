using UnityEngine;

namespace Framework.Services.Assets
{
    public interface IAssetService
    {
        Awaitable<AssetHandle<T>> LoadAsync<T>(string address) where T : Object;
        void Release(string address);
        void ReleaseAll();
    }
}
