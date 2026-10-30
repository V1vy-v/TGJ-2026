using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Interaction.Puzzle.SpeakerChain
{
    /// <summary>
    /// 喇叭链谜题：所有可调喇叭对准后，持续若干秒，解锁平台。
    /// 只放需要调整的喇叭；接收端喇叭不要放进列表。
    /// </summary>
    public class SpeakerChainPuzzle : MonoBehaviour
    {
        [SerializeField] private List<SpeakerDevice> speakerDevices = new();
        [SerializeField] private List<SpeakerPlatform> liftPlatforms = new();
        [SerializeField, Min(0f)] private float solveDelay = 3f;

        private bool _solved;
        private float _alignedTimer;

        public event Action OnPuzzleDone;

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
            if (speakerDevices == null || speakerDevices.Count == 0)
                return false;

            int adjustableCount = 0;

            for (int i = 0; i < speakerDevices.Count; i++)
            {
                var speaker = speakerDevices[i];
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

            for (int i = 0; i < speakerDevices.Count; i++)
                speakerDevices[i]?.PuzzleDone();

            if (liftPlatforms == null) return;

            for (int i = 0; i < liftPlatforms.Count; i++)
                liftPlatforms[i]?.Unlock();
        }
    }
}
