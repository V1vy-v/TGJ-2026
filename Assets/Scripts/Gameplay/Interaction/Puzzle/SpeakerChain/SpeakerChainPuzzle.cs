using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Interaction.Puzzle.SpeakerChain
{
    /// <summary>
    /// 喇叭谜题：
    /// 按列表顺序自动把每个喇叭的瞄准目标设为下一个喇叭；
    /// 第一个喇叭开局通电，其余喇叭被前一个射中后通电；
    /// 最终目标第一次被射中时，一次性锁定喇叭并激活平台。
    /// </summary>
    public class SpeakerChainPuzzle : MonoBehaviour
    {
        [Header("链条")]
        [Tooltip("按顺序排列：第 0 个是初始喇叭，最后一个是瞄准最终目标的喇叭")]
        [SerializeField] private List<SpeakerDevice> speakerDevices = new();

        [Header("最终目标")]
        [SerializeField] private SpeakerTarget target;

        [Header("完成后激活")]
        [SerializeField] private List<SpeakerPlatform> liftPlatforms = new();

        private bool _solved;

        public event Action OnPuzzleDone;

        private void Start()
        {
            SetupChain();

            if (target == null)
            {
                Debug.LogError($"[SpeakerChainPuzzle] {name} 的 target 未赋值", this);
                return;
            }

            target.HitChanged += OnTargetHitChanged;

            if (target.IsHit)
                Activate();
        }

        private void OnDisable()
        {
            if (target != null)
                target.HitChanged -= OnTargetHitChanged;
        }

        private void SetupChain()
        {
            if (speakerDevices == null || speakerDevices.Count == 0)
            {
                Debug.LogError($"[SpeakerChainPuzzle] {name} 的 speakerDevices 为空", this);
                return;
            }

            for (int i = 0; i < speakerDevices.Count; i++)
            {
                SpeakerDevice speaker = speakerDevices[i];
                if (speaker == null) continue;

                Transform aim = i + 1 < speakerDevices.Count
                    ? speakerDevices[i + 1].transform
                    : target != null
                        ? target.transform
                        : null;

                speaker.SetAimTarget(aim);
                speaker.SetPowered(i == 0);
            }
        }

        private void OnTargetHitChanged(bool hit)
        {
            if (hit)
                Activate();
        }

        private void Activate()
        {
            if (_solved) return;

            _solved = true;

            if (target != null)
                target.HitChanged -= OnTargetHitChanged;

            for (int i = 0; i < speakerDevices.Count; i++)
                speakerDevices[i]?.PuzzleDone();

            for (int i = 0; i < liftPlatforms.Count; i++)
                liftPlatforms[i]?.Unlock();

            OnPuzzleDone?.Invoke();
        }
    }
}
