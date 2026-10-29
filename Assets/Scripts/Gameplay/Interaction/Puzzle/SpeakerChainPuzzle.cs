using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Interaction.Puzzle
{
    /// <summary>
    /// 喇叭链谜题：所有可调喇叭对准后，持续若干秒，解锁平台。
    /// 只放需要调整的喇叭；接收端喇叭不要放进列表。
    /// </summary>
    public class SpeakerChainPuzzle : MonoBehaviour
    {
        [SerializeField] private List<SpeakerDevice> speakerDeviceList = new();
        [SerializeField] private List<LiftPlatform> liftPlatformList = new();
        [SerializeField, Min(0f)] private float solveDelay = 3f;

        private bool _solved;
        private float _alignedTimer;

        public event Action OnPuzzleDone;

        private void Awake()
        {
            if (speakerDeviceList == null) return;

            foreach (var speaker in speakerDeviceList)
            {
                if (speaker != null && !speaker.IsAdjustable)
                {
                    Debug.LogWarning(
                        $"[SpeakerChainPuzzle] {speaker.name} 是不可调喇叭，判定时会忽略", this);
                }
            }
        }

        private void Update()
        {
            if (_solved) return;

            if (!AreAllAligned())
            {
                _alignedTimer = 0f;
                return;
            }

            _alignedTimer += Time.deltaTime;
            if (_alignedTimer < solveDelay) return;

            Solve();
        }

        private bool AreAllAligned()
        {
            if (speakerDeviceList == null || speakerDeviceList.Count == 0)
                return false;

            int adjustableCount = 0;

            for (int i = 0; i < speakerDeviceList.Count; i++)
            {
                var speaker = speakerDeviceList[i];
                if (speaker == null) return false;
                if (!speaker.IsAdjustable) continue;

                adjustableCount++;

                if (!speaker.IsAligned)
                    return false;
            }

            return adjustableCount > 0;
        }

        private void Solve()
        {
            _solved = true;

            OnPuzzleDone?.Invoke();

            for (int i = 0; i < speakerDeviceList.Count; i++)
                speakerDeviceList[i]?.PuzzleDone();

            if (liftPlatformList == null) return;

            for (int i = 0; i < liftPlatformList.Count; i++)
                liftPlatformList[i]?.Unlock();
        }
    }
}
