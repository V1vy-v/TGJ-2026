using System;
using System.Collections.Generic;
using Gameplay.Player.Interaction;

namespace Gameplay.Interaction.Interface
{
    /// <summary>
    /// 所有可交互物：是否可交互 + 提供选项（1 条或多条）。
    /// </summary>
    public interface IInteractable
    {
        bool CanInteract(PlayerContext ctx);
        IReadOnlyList<InteractionOption> GetOptions(PlayerContext ctx);
    }

    /// <summary>
    /// 一条交互选项：提示文本 + 执行结果（true 表示成功）。
    /// </summary>
    public readonly struct InteractionOption
    {
        public string Prompt { get; }
        public Func<PlayerContext, bool> Execute { get; }

        public InteractionOption(string prompt, Func<PlayerContext, bool> execute)
        {
            Prompt = prompt;
            Execute = execute;
        }
    }
}
