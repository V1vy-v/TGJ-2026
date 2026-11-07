using UnityEngine;

namespace Gameplay.Player
{
    /// <summary>
    /// 主角动画表现层：把移动、对话、拾取转成 Animator 参数。
    /// 参数名与 Assets/Configs/Animation/Player.controller 对应：
    /// Walk(float) / Nod(trigger) / PickUp(trigger)。
    /// </summary>
    public sealed class PlayerAnimator
    {
        #region 状态

        private static readonly int WalkId = Animator.StringToHash("Walk");
        private static readonly int NodId = Animator.StringToHash("Nod");
        private static readonly int PickUpId = Animator.StringToHash("PickUp");
        private static readonly int JumpId = Animator.StringToHash("Jump");

        private readonly Animator _animator;

        #endregion

        #region 生命周期

        public PlayerAnimator(Animator animator)
        {
            _animator = animator;
        }

        #endregion

        #region 对外接口

        /// <summary>每帧调用，驱动 idle / walk 切换。</summary>
        public void Tick(bool isMoving)
        {
            if (_animator == null)
                return;

            // 每帧都写，避免状态机在其他动画结束后没有重新读取 Walk。
            _animator.SetBool(WalkId, isMoving);
        }

        /// <summary>对话时对方每说一句触发一次。</summary>
        public void PlayNod()
        {
            if (_animator == null)
                return;

            _animator.SetTrigger(NodId);
        }

        /// <summary>拾取物品时触发一次。</summary>
        public void PlayPickup()
        {
            if (_animator == null)
                return;

            _animator.SetTrigger(PickUpId);
        }

        /// <summary>起跳时触发一次。</summary>
        public void PlayJump()
        {
            if (_animator == null)
                return;

            _animator.SetTrigger(JumpId);
        }

        #endregion
    }
}
