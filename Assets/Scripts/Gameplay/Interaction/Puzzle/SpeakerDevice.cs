using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay.Interaction.Interface;
using Gameplay.Player.Interaction;
using UnityEngine;

namespace Gameplay.Interaction.Puzzle
{
    public enum SpeakerAxis
    {
        Horizontal, // 左右
        Vertical,   // 上下
    }

    public enum SpeakerTurn
    {
        Left,
        Right,
        Up,
        Down,
    }

    /// <summary>
    /// 喇叭：以初始朝向为基准，按步进偏移旋转。
    /// 只负责自身状态；四个转向作为交互选项交给 UI 选择。
    /// </summary>
    public class SpeakerDevice : InteractableBehaviour
    {
        [Header("旋转")]
        [Tooltip("旋转速度，单位：度/秒")]
        [SerializeField] private float rotateSpeed = 180f;
        
        [Header("步进")]
        [Tooltip("旋转轴：Horizontal 左右，Vertical 上下")]
        [SerializeField] private SpeakerAxis axis = SpeakerAxis.Horizontal;

        [Tooltip("每格偏移角度")]
        [SerializeField] private float offsetPerStep = 30f;

        [Tooltip("最小 step")]
        [SerializeField] private int minStep = -1;

        [Tooltip("最大 step")]
        [SerializeField] private int maxStep = 1;
        
        [Header("对准")]
        [Tooltip("alignedStep 对应的目标方向；留空则使用场景里当前的朝向")]
        [SerializeField] private Transform defaultTarget;

        [Tooltip("对准的 step；defaultTarget 就是这一步的方向")]
        [SerializeField] private int alignedStep;

        [Header("初始状态")]
        [Tooltip("场景初始 step；等于 alignedStep 会开局对准")]
        [SerializeField] private int initialStep = -1;

        [Header("交互")]
        [Tooltip("不可调喇叭：固定朝向，不响应交互，也不参与谜题判定")]
        [SerializeField] private bool isAdjustable = true;

        private Quaternion _baseRotation;
        private int _step;
        private bool _rotating;
        private bool _locked;
        private Coroutine _rotateRoutine;

        public bool IsAdjustable => isAdjustable;
        public bool IsAligned { get; private set; }
        public int Step => _step;

        public event Action<int> StepChanged;
        public event Action<SpeakerTurn> TurnRejected;

        private void Awake()
        {
            // 初始化基准方向和初始方向
            _baseRotation = defaultTarget != null
                ? CalculateBaseRotation()
                : transform.rotation;
            
            if (!isAdjustable)
            {
                _step = Mathf.Clamp(alignedStep, minStep, maxStep);
                transform.rotation = _baseRotation;
                IsAligned = true;
                return;
            }

            _step = Mathf.Clamp(initialStep, minStep, maxStep);

            if (alignedStep < minStep || alignedStep > maxStep)
            {
                Debug.LogWarning(
                    $"[SpeakerDevice] {name} 的 alignedStep 不在 step 范围内，永远无法对准", this);
            }

            if (_step == alignedStep)
            {
                Debug.LogWarning(
                    $"[SpeakerDevice] {name} 的 initialStep 等于 alignedStep，开局即对准", this);
            }

            transform.rotation = _baseRotation * GetStepRotation(_step);

            IsAligned = _step == alignedStep;
        }

        private Quaternion CalculateBaseRotation()
        {
            Vector3 direction = defaultTarget.position - transform.position;

            if (axis == SpeakerAxis.Horizontal)
                direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f)
                return transform.rotation;

            return Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        private Quaternion GetStepRotation(int step)
        {
            int delta = step - alignedStep;
            float angle = delta * offsetPerStep;

            return axis == SpeakerAxis.Horizontal
                ? Quaternion.Euler(0f, angle, 0f)
                : Quaternion.Euler(-angle, 0f, 0f);
        }

        public override bool CanInteract(PlayerContext ctx)
        {
            return isAdjustable && !_rotating && !_locked;
        }

        public void PuzzleDone()
        {
            _locked = true;
        }

        public override IReadOnlyList<InteractionOption> GetOptions(PlayerContext ctx)
        {
            if (!isAdjustable || _locked)
                return Array.Empty<InteractionOption>();

            if(axis == SpeakerAxis.Horizontal)
            {
                return new[]
                {
                    Option("向左转", _ => TryTurn(SpeakerTurn.Left)),
                    Option("向右转", _ => TryTurn(SpeakerTurn.Right)),
                };
            }
            else
            {
                return new[]
                {
                    Option("向上转", _ => TryTurn(SpeakerTurn.Up)),
                    Option("向下转", _ => TryTurn(SpeakerTurn.Down)),
                };
            }
        }

        public bool TryTurn(SpeakerTurn turn)
        {
            if (!isAdjustable || _rotating || _locked)
                return false;

            bool horizontalTurn = turn == SpeakerTurn.Left || turn == SpeakerTurn.Right;
            bool axisIsHorizontal = axis == SpeakerAxis.Horizontal;

            if (axisIsHorizontal != horizontalTurn)
            {
                TurnRejected?.Invoke(turn);
                return false;
            }

            int delta = turn switch
            {
                SpeakerTurn.Left => -1,
                SpeakerTurn.Right => 1,
                SpeakerTurn.Up => -1,
                SpeakerTurn.Down => 1,
                _ => 0,
            };

            int nextStep = _step + delta;
            if (nextStep < minStep || nextStep > maxStep)
            {
                TurnRejected?.Invoke(turn);
                return false;
            }

            _step = nextStep;
            StartRotate();
            return true;
        }

        private void StartRotate()
        {
            if (_rotateRoutine != null)
                StopCoroutine(_rotateRoutine);

            _rotating = true;

            _rotateRoutine = StartCoroutine(
                RotateRoutine(_baseRotation * GetStepRotation(_step)));
        }

        private IEnumerator RotateRoutine(Quaternion targetRotation)
        {
            while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotateSpeed * Time.deltaTime);

                yield return null;
            }

            transform.rotation = targetRotation;
            _rotating = false;
            _rotateRoutine = null;

            IsAligned = _step == alignedStep;
            StepChanged?.Invoke(_step);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, transform.forward * 3f);
        }
    }
}
