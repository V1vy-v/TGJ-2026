using Framework.Bootstrap;
using Framework.Services.Assets;
using Framework.Services.UI;
using Gameplay.UI;
using UnityEngine;

public class test01 : MonoBehaviour
{
    private void Start()
    {
        var awaitable = Test();
    }

    private async Awaitable Test()
    {
        await Awaitable.WaitForSecondsAsync(1f);

        var assets = GameRoot.Instance.Services.Get<IAssetService>();
        var handle = await assets.LoadAsync<GameObject>(AssetKeys.MainMenuScreen);
        Debug.Log($"[Test] loaded: {handle.Asset.name}");
        
        GameRoot.Instance.Services.Get<UIManager>().OpenScreen<MainMenuScreen>();
    }
}