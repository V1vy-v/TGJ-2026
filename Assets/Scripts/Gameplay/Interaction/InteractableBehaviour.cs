using System;
using System.Collections.Generic;
using Gameplay.Interaction.Interface;
using Gameplay.Player.Interaction;
using UnityEngine;

namespace Gameplay.Interaction
{
    /// <summary>
    /// 所有交互物的基类：只关心“能不能交互”和“返回哪些选项”，不预设选项数量。
    /// </summary>
    public abstract class InteractableBehaviour : MonoBehaviour, IInteractable
    {
        public virtual bool CanInteract(PlayerContext ctx)
        {
            return isActiveAndEnabled;
        }

        public abstract IReadOnlyList<InteractionOption> GetOptions(PlayerContext ctx);

        /// <summary>
        /// 构造一条交互选项。
        /// </summary>
        protected static InteractionOption Option(
            string prompt, Func<PlayerContext, bool> execute)
        {
            return new InteractionOption(prompt, execute);
        }

        /// <summary>
        /// 构造只有一条选项的列表，供单动作交互物使用。
        /// </summary>
        protected static IReadOnlyList<InteractionOption> SingleOption(
            string prompt, Func<PlayerContext, bool> execute)
        {
            return new[] { Option(prompt, execute) };
        }
    }
}
