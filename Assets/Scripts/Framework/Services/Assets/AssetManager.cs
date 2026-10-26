using System;
using System.Collections.Generic;
using Framework.Core;
using UnityEngine;

namespace Framework.Services.Assets
{
    public class AssetManager : IManager, IAssetService
    {
        #region 状态

        public int Order => 5;
        
        private readonly IAssetProvider _provider;
        private readonly Dictionary<string, AssetEntry> _cache = new();

        private sealed class AssetEntry
        {
            public UnityEngine.Object Asset;
            public Type AssetType;
            public int RefCount;
        }

        #endregion

        #region 生命周期

        public AssetManager(IAssetProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public void Initialize(ServiceRegistry services, MonoBehaviour host)
        {
            services.Register<IAssetService>(this);
        }

        public void Shutdown()
        {
            ReleaseAll();
        }

        #endregion

        #region 对外接口

        public async Awaitable<AssetHandle<T>> LoadAsync<T>(string address) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentException("资源地址不能为空", nameof(address));

            if (!_provider.IsInitialized)
                await _provider.InitializeAsync();

            if (_cache.TryGetValue(address, out var entry))
            {
                if (entry.AssetType != typeof(T))
                {
                    throw new InvalidOperationException(
                        $"[AssetManager] {address} 已按 {entry.AssetType.Name} 加载，不能再按 {typeof(T).Name} 加载");
                }

                entry.RefCount++;
                return new AssetHandle<T>((T)entry.Asset, address, this);
            }

            var asset = await _provider.LoadAsync<T>(address);
            if (asset == null)
                throw new InvalidOperationException($"[AssetManager] 资源加载失败: {address}");

            _cache.Add(address, new AssetEntry
            {
                Asset = asset,
                AssetType = typeof(T),
                RefCount = 1,
            });

            return new AssetHandle<T>(asset, address, this);
        }

        public void Release(string address)
        {
            if (!_cache.TryGetValue(address, out var entry))
            {
                Debug.LogError($"[AssetManager] 释放了未加载的资源: {address}");
                return;
            }

            entry.RefCount--;
            if (entry.RefCount > 0) return;

            if (entry.RefCount < 0)
            {
                Debug.LogError($"[AssetManager] 引用计数为负: {address}");
                return;
            }

            _cache.Remove(address);
            _provider.Release(address);
        }

        public void ReleaseAll()
        {
            foreach (var pair in _cache)
                _provider.Release(pair.Key);

            _cache.Clear();
        }

        #endregion
    }
}