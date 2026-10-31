using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay.Interaction.Interface;
using Gameplay.Player.Interaction;
using UnityEngine;

namespace Gameplay.Interaction.Puzzle.SpeakerChain
{
    public enum SpeakerAxis
    {
        Horizontal,
        Vertical,
    }

    public enum SpeakerTurn
    {
        Left,
        Right,
        Up,
        Down,
    }

    /// <summary>
    /// 喇叭：按 step 旋转，只有通电时发射射线。
    /// 射线命中下一个喇叭会让它通电；命中最终目标会通知 SpeakerTarget。
    /// </summary>
    public class SpeakerDevice : InteractableBehaviour
    {
        #region 状态

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
        [Tooltip("由 SpeakerChainPuzzle 自动设置；也可手动指定")]
        [SerializeField] private Transform aimTarget;

        [Tooltip("对准的 step；aimTarget 就是这一步的方向")]
        [SerializeField] private int alignedStep;

        [Header("初始状态")]
        [Tooltip("场景初始 step；等于 alignedStep 会开局对准")]
        [SerializeField] private int initialStep = -1;

        [Header("交互")]
        [Tooltip("不可调喇叭：固定朝向，不响应交互")]
        [SerializeField] private bool isAdjustable = true;

        [Header("射线")]
        [Tooltip("射线最大长度")]
        [SerializeField] private float beamRange = 20f;

        [Tooltip("射线会命中的层：墙、石台、喇叭、最终目标")]
        [SerializeField] private LayerMask beamMask;

        [Tooltip("光束渲染器")]
        [SerializeField] private LineRenderer beamRenderer;

        [Tooltip("未命中目标时的颜色")]
        [SerializeField] private Color blockColor;

        [Tooltip("命中目标时的颜色")]
        [SerializeField] private Color alignColor;

        private Quaternion _baseRotation;
        private int _step;
        private bool _rotating;
        private bool _powered;
        private bool _locked;
        private Coroutine _rotateRoutine;
        private SpeakerDevice _poweredSpeaker;
        private SpeakerTarget _hitReceiver;
        private readonly RaycastHit[] _hits = new RaycastHit[16];

        public bool IsAdjustable => isAdjustable;
        public bool IsPowered => _powered;
        public bool IsAligned { get; private set; }
        public bool IsConnected { get; private set; }
        public int Step => _step;

        public event Action<int> StepChanged;
        public event Action<SpeakerTurn> TurnRejected;

        #endregion

        #region 生命周期

        private void Awake()
        {
            _baseRotation = CalculateBaseRotation();

            if (!isAdjustable)
            {
                _step = alignedStep;
                transform.rotation = _baseRotation;
                IsAligned = true;
                SetBeamVisible(false);
                return;
            }

            _step = Mathf.Clamp(initialStep, minStep, maxStep);
            transform.rotation = GetTargetRotation(_step);

            IsAligned = _step == alignedStep;
            SetBeamVisible(false);
        }

        private void OnDisable()
        {
            ClearBeam();
        }

        private void Update()
        {
            if (!_powered)
            {
                ClearBeam();
                return;
            }

            UpdateBeam();
        }

        #endregion

        #region 对外接口

        public void SetAimTarget(Transform target)
        {
            aimTarget = target;
            _baseRotation = CalculateBaseRotation();
            transform.rotation = isAdjustable ? GetTargetRotation(_step) : _baseRotation;
        }

        public void SetPowered(bool powered)
        {
            if (_powered == powered)
                return;

            _powered = powered;

            if (!powered)
            {
                ClearBeam();
                return;
            }

            SetBeamVisible(true);
        }

        public void PuzzleDone()
        {
            _locked = true;
        }

        public override bool CanInteract(PlayerContext ctx)
        {
            return isAdjustable && !_rotating && !_locked;
        }

        public override IReadOnlyList<InteractionOption> GetOptions(PlayerContext ctx)
        {
            if (!isAdjustable || _locked)
                return Array.Empty<InteractionOption>();

            if (axis == SpeakerAxis.Horizontal)
            {
                return new[]
                {
                    Option("向左转", _ => TryTurn(SpeakerTurn.Left)),
                    Option("向右转", _ => TryTurn(SpeakerTurn.Right)),
                };
            }

            return new[]
            {
                Option("向上转", _ => TryTurn(SpeakerTurn.Up)),
                Option("向下转", _ => TryTurn(SpeakerTurn.Down)),
            };
        }

        #endregion

        #region 内部实现

        private Quaternion CalculateBaseRotation()
        {
            if (aimTarget == null)
                return transform.rotation;

            Vector3 direction = aimTarget.position - transform.position;

            if (direction.sqrMagnitude < 0.0001f)
                return transform.rotation;

            return Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        private Quaternion GetTargetRotation(int step)
        {
            float angle = (step - alignedStep) * offsetPerStep;

            return axis == SpeakerAxis.Horizontal
                ? Quaternion.Euler(0f, angle, 0f) * _baseRotation
                : _baseRotation * Quaternion.Euler(-angle, 0f, 0f);
        }

        private bool TryTurn(SpeakerTurn turn)
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
            _rotateRoutine = StartCoroutine(RotateRoutine(GetTargetRotation(_step)));
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

        private void UpdateBeam()
        {
            if (beamRenderer == null)
                return;

            Vector3 origin = transform.position;
            Vector3 direction = transform.forward;

            int count = Physics.RaycastNonAlloc(
                origin,
                direction,
                _hits,
                beamRange,
                beamMask,
                QueryTriggerInteraction.Ignore);

            float nearestDistance = beamRange;
            SpeakerDevice hitSpeaker = null;
            SpeakerTarget hitReceiver = null;

            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = _hits[i];

                if (hit.collider.GetComponentInParent<SpeakerDevice>() == this)
                    continue;

                if (hit.distance >= nearestDistance)
                    continue;

                nearestDistance = hit.distance;
                hitSpeaker = hit.collider.GetComponentInParent<SpeakerDevice>();
                hitReceiver = hit.collider.GetComponentInParent<SpeakerTarget>();
            }

            bool aligned = IsAligned;

            if (!aligned)
            {
                hitSpeaker = null;
                hitReceiver = null;
            }

            if (hitSpeaker != null && !IsAimTarget(hitSpeaker.transform))
                hitSpeaker = null;

            if (hitReceiver != null && !IsAimTarget(hitReceiver.transform))
                hitReceiver = null;

            SetPoweredSpeaker(hitSpeaker);
            SetHitReceiver(hitReceiver);

            IsConnected = hitSpeaker != null || hitReceiver != null;

            Vector3 end = origin + direction * nearestDistance;

            beamRenderer.SetPosition(0, origin);
            beamRenderer.SetPosition(1, end);
            beamRenderer.startColor = beamRenderer.endColor =
                IsConnected ? alignColor : blockColor;
        }

        private bool IsAimTarget(Transform hit)
        {
            if (aimTarget == null || hit == null)
                return false;

            return hit == aimTarget || hit.IsChildOf(aimTarget);
        }

        private void SetPoweredSpeaker(SpeakerDevice newSpeaker)
        {
            if (_poweredSpeaker == newSpeaker)
                return;

            _poweredSpeaker?.SetPowered(false);
            _poweredSpeaker = newSpeaker;
            _poweredSpeaker?.SetPowered(true);
        }

        private void SetHitReceiver(SpeakerTarget newReceiver)
        {
            if (_hitReceiver == newReceiver)
                return;

            _hitReceiver?.SetHit(false);
            _hitReceiver = newReceiver;
            _hitReceiver?.SetHit(true);
        }

        private void ClearBeam()
        {
            SetPoweredSpeaker(null);
            SetHitReceiver(null);

            IsConnected = false;
            SetBeamVisible(false);
        }

        private void SetBeamVisible(bool visible)
        {
            if (beamRenderer != null)
                beamRenderer.enabled = visible;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, transform.forward * 3f);
        }

        #endregion
    }
}
