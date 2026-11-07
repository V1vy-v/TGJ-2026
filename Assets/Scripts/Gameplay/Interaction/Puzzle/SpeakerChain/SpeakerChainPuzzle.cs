using System;
using System.Collections;
using System.Collections.Generic;
using Framework.Bootstrap;
using Gameplay.Audio;
using Gameplay.Flow;
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

        [Tooltip("谜题完成后自动播放的 Tips key，可留空")]
        [SerializeField] private string solvedTipsKey;

        [Tooltip("谜题完成后解锁的复活点 ID，可留空")]
        [SerializeField] private string checkpointOnSolved;

        [Tooltip("目标被射中后延迟多少秒判定成功")]
        [SerializeField, Min(0f)] private float solveDelay = 3f;

        [Tooltip("初始化时给链条内所有喇叭通电；用于共用喇叭、需要独立发射的谜题")]
        [SerializeField] private bool powerAllSpeakers;

        [Header("前置谜题")]
        [Tooltip("该谜题完成后才初始化本链条；用于共用喇叭的连续谜题")]
        [SerializeField] private SpeakerChainPuzzle prerequisite;

        [Header("完成后的音频")]
        [SerializeField] private bool playAmbientOnSolved;
        [SerializeField] private GameSfx ambientOnSolved = GameSfx.Water;

        private bool _solved;
        private bool _initialized;
        private Coroutine _solveRoutine;

        public bool IsSolved => _solved;

        public event Action OnPuzzleDone;

        private void Start()
        {
            if (prerequisite != null)
            {
                prerequisite.OnPuzzleDone += Initialize;
                if (prerequisite.IsSolved)
                    Initialize();
                return;
            }

            Initialize();
        }

        private void OnDisable()
        {
            if (target != null)
                target.HitChanged -= OnTargetHitChanged;

            if (prerequisite != null)
                prerequisite.OnPuzzleDone -= Initialize;

            CancelSolve();
        }

        private void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;
            SetupChain();

            if (target == null)
            {
                Debug.LogError($"[SpeakerChainPuzzle] {name} 的 target 未赋值", this);
                return;
            }

            target.HitChanged += OnTargetHitChanged;

            if (target.IsHit)
                BeginSolve();
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

                speaker.SetPowered(powerAllSpeakers || i == 0);
            }
        }

        private void OnTargetHitChanged(bool hit)
        {
            if (hit)
                BeginSolve();
            else
                CancelSolve();
        }

        private void BeginSolve()
        {
            if (_solved || _solveRoutine != null)
                return;

            _solveRoutine = StartCoroutine(SolveAfterDelay());
        }

        private IEnumerator SolveAfterDelay()
        {
            yield return new WaitForSeconds(solveDelay);

            _solveRoutine = null;

            if (target != null && target.IsHit)
                Activate();
        }

        private void CancelSolve()
        {
            if (_solveRoutine == null)
                return;

            StopCoroutine(_solveRoutine);
            _solveRoutine = null;
        }

        private void Activate()
        {
            if (_solved) return;

            _solved = true;
            CancelSolve();

            if (target != null)
                target.HitChanged -= OnTargetHitChanged;

            for (int i = 0; i < speakerDevices.Count; i++)
                speakerDevices[i]?.PuzzleDone();

            for (int i = 0; i < liftPlatforms.Count; i++)
                liftPlatforms[i]?.Unlock();

            OnPuzzleDone?.Invoke();
            if (!string.IsNullOrWhiteSpace(checkpointOnSolved) &&
                GameRoot.Instance.Services.TryGet<ICheckpointService>(out var checkpoints))
            {
                checkpoints.Unlock(checkpointOnSolved);
            }
            GameRoot.Instance.Services.Get<IGameAudio>().PlaySfx(GameSfx.SpeakerSolved);
            if (playAmbientOnSolved)
                GameRoot.Instance.Services.Get<IGameAudio>().PlayAmbient(ambientOnSolved);
            Gameplay.Flow.TipsTrigger.Show(solvedTipsKey);
        }
    }
}
