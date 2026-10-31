using System;
using UnityEngine;

namespace Gameplay.Interaction.Puzzle.SpeakerChain
{
    /// <summary>
    /// 最终接收端：由命中它的喇叭调用 SetHit。
    /// 只记录开关状态，不依赖任何场景引用。
    /// </summary>
    public class SpeakerTarget : MonoBehaviour
    {
        public bool IsHit { get; private set; }

        public event Action<bool> HitChanged;

        public void SetHit(bool hit)
        {
            if (IsHit == hit)
                return;

            IsHit = hit;
            HitChanged?.Invoke(hit);
        }
    }
}
