using UnityEngine.InputSystem;

namespace Framework.Services.Input
{
    /// <summary>
    /// 输入服务：只负责提供 map 和兜底关闭，不维护 map 的启停状态。
    /// </summary>
    public interface IInputService
    {
        InputActionMap GetMap(string mapName);
        void DisableAll();
    }
}
