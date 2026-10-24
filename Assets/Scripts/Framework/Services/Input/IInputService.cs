using System;
using UnityEngine.InputSystem;

namespace Framework.Services.Input
{
    /// <summary>
    /// 通用输入服务接口
    /// </summary>
    public interface IInputService
    {
        InputActionAsset InputAsset { get; }
        IDisposable EnableMap(string mapName);
        bool IsMapEnabled(string mapName);
        InputAction GetAction(string mapName, string actionName);
    }
}
