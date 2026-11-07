using System;
using System.Collections;
using System.Collections.Generic;
using Framework.Bootstrap;
using Framework.Services.Localization;
using Gameplay.Audio;
using Gameplay.Flow;
using Gameplay.Interaction.Interface;
using Gameplay.Player;
using Gameplay.Player.Interaction;
using UnityEngine;

namespace Gameplay.Interaction.Teleport
{
    /// <summary>
    /// 通用传送点：黑场、玩家瞬移，可选进入/离开迷宫黑场。
    /// 后续同类传送点直接复用本组件，只改 destination。
    /// </summary>
    public class TeleportPoint : InteractableBehaviour
    {
        #region 配置

        [Header("传送")]
        [Tooltip("玩家传送到的位置和朝向")]
        [SerializeField] private Transform destination;

        [Tooltip("交互提示的本地化 key")]
        [SerializeField] private string promptKey = "interaction.teleport";

        [Header("到达后")]
        [Tooltip("传送完成后是否进入迷宫黑场")]
        [SerializeField] private bool enterMazeOnArrive;

        [Tooltip("进入黑场后自动播放的 Tips key，可留空")]
        [SerializeField] private string enterTipsKey;

        [Tooltip("传送完成后是否离开迷宫黑场")]
        [SerializeField] private bool exitMazeOnArrive;

        [Tooltip("离开黑场后自动播放的 Tips key，可留空")]
        [SerializeField] private string exitTipsKey;

        [Tooltip("使用一次后永久锁定")]
        [SerializeField] private bool disableAfterUse;

        [Tooltip("到达后解锁的复活点 ID，可留空")]
        [SerializeField] private string checkpointOnArrive;

        [Tooltip("到达后补充的能量格数；0 表示不补充")]
        [SerializeField] private int energyOnArrive;

        #endregion

        #region 状态

        private bool _locked;
        private bool _teleporting;

        #endregion

        #region 生命周期

        private void Start()
        {
            if (destination == null)
            {
                Debug.LogError($"[TeleportPoint] {name} 的 destination 未赋值", this);
                _locked = true;
            }
        }

        #endregion

        #region 对外接口

        public override bool CanInteract(PlayerContext ctx)
        {
            return base.CanInteract(ctx) && !_locked && !_teleporting;
        }

        public override IReadOnlyList<InteractionOption> GetOptions(PlayerContext ctx)
        {
            if (!CanInteract(ctx))
                return Array.Empty<InteractionOption>();

            string prompt = promptKey;

            if (GameRoot.Instance != null)
            {
                var localization =
                    GameRoot.Instance.Services.Get<ILocalizationService>();
                prompt = localization.GetText(promptKey);
            }

            return SingleOption(
                string.IsNullOrEmpty(prompt) ? "Teleport" : prompt,
                context => StartTeleport(context));
        }

        #endregion

        #region 内部实现

        private bool StartTeleport(PlayerContext ctx)
        {
            if (_locked || _teleporting || destination == null || ctx.Player == null)
                return false;

            StartCoroutine(TeleportRoutine(ctx.Player));
            return true;
        }

        private IEnumerator TeleportRoutine(PlayerController player)
        {
            _teleporting = true;
            player.Interactor.Suspend();

            ScreenFadeController fade = ScreenFadeController.Instance;
            if (fade != null)
                yield return fade.FadeOut();

            player.TeleportTo(destination.position, destination.rotation);

            if (!string.IsNullOrWhiteSpace(checkpointOnArrive) &&
                GameRoot.Instance.Services.TryGet<ICheckpointService>(out var checkpoints))
            {
                checkpoints.Unlock(checkpointOnArrive);
            }

            if (energyOnArrive > 0 && player.Energy.TryRecycle(energyOnArrive))
            {
                if (GameRoot.Instance.Services.TryGet<IGameAudio>(out var audio))
                    audio.PlaySfx(GameSfx.EnergyInject);
            }

            if (enterMazeOnArrive)
            {
                player.EnterMaze();
                TipsTrigger.Show(enterTipsKey);
            }
            else if (exitMazeOnArrive)
            {
                player.ExitMaze();
                TipsTrigger.Show(exitTipsKey);
            }

            if (fade != null)
                yield return fade.FadeIn();

            player.Interactor.Resume();
            _teleporting = false;

            if (disableAfterUse)
                _locked = true;
        }

        #endregion
    }
}
