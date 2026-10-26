using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using YooAsset;
using YooHandle = YooAsset.AssetHandle;

namespace Framework.Services.Assets.Providers
{
    public class YooAssetProvider : IAssetProvider
    {
        #region 状态

        private readonly string _packageName;
        private readonly Dictionary<string, YooHandle> _handles = new();

        private ResourcePackage _package;
        private Task _initTask;

        #endregion

        #region 生命周期

        public YooAssetProvider(string packageName)
        {
            _packageName = packageName ?? throw new ArgumentNullException(nameof(packageName));
        }

        #endregion

        #region 对外接口

        public bool IsInitialized =>
            _package != null && _package.InitializeStatus == EOperationStatus.Succeeded;

        public async Awaitable InitializeAsync()
        {
            if (IsInitialized) return;

            _initTask ??= InitializeInternalAsync();
            await _initTask;
        }

        public async Awaitable<T> LoadAsync<T>(string address) where T : UnityEngine.Object
        {
            await InitializeAsync();

            var handle = _package.LoadAssetAsync<T>(address);
            await handle;

            if (handle.Status != EOperationStatus.Succeeded)
                throw new InvalidOperationException($"[YooAssetProvider] 加载失败 {address}: {handle.Error}");

            _handles[address] = handle;
            return handle.GetAssetObject<T>();
        }

        public void Release(string address)
        {
            if (_handles.Remove(address, out var handle))
                handle.Release();
        }

        #endregion

        #region 内部实现

        private async Task InitializeInternalAsync()
        {
            if (!YooAssets.IsInitialized)
                YooAssets.Initialize();

            if (!YooAssets.TryGetPackage(_packageName, out _package))
                _package = YooAssets.CreatePackage(_packageName);

#if UNITY_EDITOR
            var buildResult = EditorSimulateBuildInvoker.Build(
                _packageName, (int)EBundleType.VirtualAssetBundle);

            var options = new EditorSimulateModeOptions
            {
                EditorFileSystemParameters =
                    FileSystemParameters.CreateDefaultEditorFileSystemParameters(buildResult.PackageRootDirectory),
            };
#else
            var options = new OfflinePlayModeOptions
            {
                BuiltinFileSystemParameters =
                    FileSystemParameters.CreateDefaultBuiltinFileSystemParameters(),
            };
#endif

            var initOp = _package.InitializePackageAsync(options);
            await initOp;
            if (initOp.Status != EOperationStatus.Succeeded)
                throw new InvalidOperationException($"[YooAssetProvider] 包初始化失败: {initOp.Error}");

            var versionOp = _package.RequestPackageVersionAsync();
            await versionOp;
            if (versionOp.Status != EOperationStatus.Succeeded)
                throw new InvalidOperationException($"[YooAssetProvider] 请求版本失败: {versionOp.Error}");

            var manifestOp = _package.LoadPackageManifestAsync(
                new LoadPackageManifestOptions(versionOp.PackageVersion, 60));
            await manifestOp;
            if (manifestOp.Status != EOperationStatus.Succeeded)
                throw new InvalidOperationException($"[YooAssetProvider] 加载清单失败: {manifestOp.Error}");
        }
        
        #endregion
    }
}