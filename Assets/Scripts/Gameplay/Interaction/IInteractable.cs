using Gameplay.Player.Interaction;

namespace Gameplay.Interaction
{
    public interface IInteractable
    {
        string Prompt { get; }                      // 显示给玩家的提示
        bool CanInteract(PlayerContext ctx);        // 现在能不能交互
        void Interact(PlayerContext ctx);           // 真正执行
    }
}