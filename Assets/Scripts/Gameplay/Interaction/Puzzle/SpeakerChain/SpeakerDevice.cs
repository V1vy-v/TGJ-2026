using System;
using System.Collections;
using System.Collections.Generic;
using Framework.Bootstrap;
using Framework.Services.Localization;
using Gameplay.Interaction.Interface;
using Gameplay.Player.Interaction;
using UnityEngine;

namespace Gameplay.Interaction.Puzzle.SpeakerChain
{
    public enum SpeakerTurn
    {
        Left,
        Right,
    }

    /// <summary>
    /// 平放的喇叭：只改变 Y 轴旋转，X/Z 保持初始角度。
    /// 普通喇叭只有一个主接收者，额外接收者只用于共享喇叭等特殊情况。
    /// </summary>
    public class SpeakerDevice : InteractableBehaviour
    {
        #region 状态

        [Header("旋转")]
        [Tooltip("没有指定接收者的 step，每格旋转的 Y 角度")]
        [SerializeField] private float anglePerStep = 30f;

        [Tooltip("旋转速度，单位：度/秒")]
        [SerializeField] private float rotateSpeed = 180f;

        [Header("主接收者")]
        [Tooltip("大多数喇叭只需要这一个接收者")]
        [SerializeField] private Transform primaryReceiver;

        [Tooltip("主接收者对应的 step；范围 -1..1")]
        [SerializeField] private int primaryReceiverStep;

        [Header("额外接收者")]
        [Tooltip("只有共享喇叭或一个喇叭需要连接多个目标时才添加")]
        [SerializeField] private List<StepReceiver> extraReceivers = new();

        [Header("初始状态")]
        [Tooltip("初始 step；范围 -1..1")]
        [SerializeField] private int initialStep;

        [Header("交互")]
        [Tooltip("不可调喇叭：固定朝向，不响应交互")]
        [SerializeField] private bool isAdjustable = true;

        [Tooltip("谜题完成后是否锁定交互；需要参与其它谜题时关闭")]
        [SerializeField] private bool lockAfterPuzzle = true;

        [Header("射线")]
        [Tooltip("射线最大长度")]
        [SerializeField] private float beamRange = 100f;

        [Tooltip("射线渲染器")]
        [SerializeField] private LineRenderer beamRenderer;

        [Tooltip("射线起点；留空时自动查找 BeamStart")]
        [SerializeField] private Transform beamStart;

        [Tooltip("未连接时的颜色")]
        [SerializeField] private Color blockColor;

        [Tooltip("连接时的颜色")]
        [SerializeField] private Color alignColor;

        [Serializable]
        private class StepReceiver
        {
            [SerializeField] private int step;
            [SerializeField] private Transform receiver;

            public int Step => step;
            public Transform Receiver => receiver;

            public void Set(int valueStep, Transform valueReceiver)
            {
                step = valueStep;
                receiver = valueReceiver;
            }
        }

        private Vector3 _initialEuler;
        private float _baseYaw;
        private int _baseStep;
        private float _resolvedAnglePerStep;
        private int _step;
        private bool _rotating;
        private bool _powered;
        private bool _locked;
        private Coroutine _rotateRoutine;
        private SpeakerDevice _poweredSpeaker;
        private SpeakerTarget _hitReceiver;

        public bool IsAdjustable => isAdjustable;
        public bool IsPowered => _powered;
        public bool IsAligned { get; private set; }
        public bool IsConnected { get; private set; }
        public int Step => _step;
        public Transform BeamStart => beamStart;

        public event Action<int> StepChanged;
        public event Action<SpeakerTurn> TurnRejected;

        #endregion

        #region 生命周期

        private void Awake()
        {
            if (beamStart == null)
                beamStart = transform.Find("BeamStart");

            _initialEuler = transform.localEulerAngles;
            ResolveRotation();
            _step = Mathf.Clamp(initialStep, -1, 1);
            SetLocalYaw(GetYawForStep(_step));
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
            _locked = lockAfterPuzzle;
        }

        public override bool CanInteract(PlayerContext ctx)
        {
            return isAdjustable && !_rotating && !_locked;
        }

        public override IReadOnlyList<InteractionOption> GetOptions(PlayerContext ctx)
        {
            if (!isAdjustable || _locked)
                return Array.Empty<InteractionOption>();

            var localization = GameRoot.Instance.Services.Get<ILocalizationService>();
            return new[]
            {
                Option(localization.GetText("interaction.turnLeft"), _ => TryTurn(SpeakerTurn.Left)),
                Option(localization.GetText("interaction.turnRight"), _ => TryTurn(SpeakerTurn.Right)),
            };
        }

        #endregion

        #region 内部实现

        private Transform GetReceiver(int step)
        {
            if (step == primaryReceiverStep && primaryReceiver != null)
                return primaryReceiver;

            if (extraReceivers == null)
                return null;

            for (int i = 0; i < extraReceivers.Count; i++)
            {
                StepReceiver item = extraReceivers[i];
                if (item != null && item.Step == step && item.Receiver != null)
                    return item.Receiver;
            }

            return null;
        }

        private bool TryTurn(SpeakerTurn turn)
        {
            if (!isAdjustable || _rotating || _locked)
                return false;

            int delta = turn == SpeakerTurn.Left ? -1 : 1;
            int nextStep = Mathf.Clamp(_step + delta, -1, 1);
            if (nextStep == _step)
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
            _rotateRoutine = StartCoroutine(RotateRoutine());
        }

        private IEnumerator RotateRoutine()
        {
            float targetYaw = GetYawForStep(_step);
            float currentYaw = NormalizeAngle(transform.localEulerAngles.y);

            while (Mathf.Abs(Mathf.DeltaAngle(currentYaw, targetYaw)) > 0.1f)
            {
                currentYaw = Mathf.MoveTowardsAngle(
                    currentYaw,
                    targetYaw,
                    rotateSpeed * Time.deltaTime);

                SetLocalYaw(currentYaw);
                yield return null;
            }

            SetLocalYaw(targetYaw);
            _rotating = false;
            _rotateRoutine = null;

            IsAligned = _step == initialStep;
            StepChanged?.Invoke(_step);
        }

        private void ResolveRotation()
        {
            _baseStep = initialStep;
            _baseYaw = _initialEuler.y;
            _resolvedAnglePerStep = anglePerStep;

            StepReceiver primary = new StepReceiver();
            primary.Set(primaryReceiverStep, primaryReceiver);
            StepReceiver first = primary.Receiver != null ? primary : FindFirstExtra();

            if (first == null || first.Receiver == null)
                return;

            _baseStep = first.Step;
            _baseYaw = SolveReceiverYaw(first.Receiver);

            StepReceiver second = FindDifferentExtra(_baseStep);
            if (second == null || second.Receiver == null)
                return;

            int stepDelta = second.Step - _baseStep;
            if (stepDelta == 0)
                return;

            float secondYaw = SolveReceiverYaw(second.Receiver);
            float yawDelta = Mathf.DeltaAngle(_baseYaw, secondYaw);
            _resolvedAnglePerStep = yawDelta / stepDelta;
        }

        private StepReceiver FindFirstExtra()
        {
            if (extraReceivers == null)
                return null;

            for (int i = 0; i < extraReceivers.Count; i++)
            {
                if (extraReceivers[i] != null && extraReceivers[i].Receiver != null)
                    return extraReceivers[i];
            }

            return null;
        }

        private StepReceiver FindDifferentExtra(int step)
        {
            if (extraReceivers == null)
                return null;

            for (int i = 0; i < extraReceivers.Count; i++)
            {
                StepReceiver item = extraReceivers[i];
                if (item != null && item.Receiver != null && item.Step != step)
                    return item;
            }

            return null;
        }

        private float SolveReceiverYaw(Transform receiver)
        {
            if (receiver == null)
                return _initialEuler.y;

            Vector3 anchor = GetReceiverAnchor(receiver);
            Quaternion original = transform.localRotation;
            float yaw = NormalizeAngle(_initialEuler.y);

            // BeamStart 不在根节点中心，转动后它的位置也会变。
            // 用几次迭代求出真正让 BeamStart 对准接收者的 Y。
            for (int i = 0; i < 8; i++)
            {
                SetLocalYaw(yaw);

                Vector3 origin = beamStart != null
                    ? beamStart.position
                    : transform.position;
                Vector3 direction = anchor - origin;

                if (direction.sqrMagnitude < 0.0001f)
                    break;

                float nextYaw = YawFromDirection(direction);
                if (Mathf.Abs(Mathf.DeltaAngle(yaw, nextYaw)) < 0.01f)
                {
                    yaw = nextYaw;
                    break;
                }

                yaw = nextYaw;
            }

            transform.localRotation = original;
            return yaw;
        }

        private float GetYawForStep(int step)
        {
            return NormalizeAngle(
                _baseYaw + (step - _baseStep) * _resolvedAnglePerStep);
        }

        private float YawFromDirection(Vector3 direction)
        {
            Transform parent = transform.parent;
            Vector3 localDirection = parent != null
                ? parent.InverseTransformDirection(direction)
                : direction;

            return Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
        }

        private Vector3 GetReceiverAnchor(Transform receiver)
        {
            SpeakerDevice receiverSpeaker = receiver.GetComponentInParent<SpeakerDevice>();
            if (receiverSpeaker != null && receiverSpeaker != this &&
                receiverSpeaker.BeamStart != null)
                return receiverSpeaker.BeamStart.position;

            return receiver.position;
        }

        private void SetLocalYaw(float yaw)
        {
            transform.localRotation = Quaternion.Euler(
                _initialEuler.x,
                yaw,
                _initialEuler.z);
        }

        private static float NormalizeAngle(float angle)
        {
            return Mathf.Repeat(angle + 180f, 360f) - 180f;
        }

        private void UpdateBeam()
        {
            if (beamRenderer == null || beamStart == null)
                return;

            // 旋转过程中不允许判定命中，必须等转到目标方向。
            if (_rotating)
            {
                SetPoweredSpeaker(null);
                SetHitReceiver(null);
                IsConnected = false;
                IsAligned = false;

                Vector3 rotatingOrigin = beamStart.position;
                SetBeamVisible(true);
                beamRenderer.SetPosition(0, rotatingOrigin);
                beamRenderer.SetPosition(1, rotatingOrigin + transform.forward * beamRange);
                beamRenderer.startColor = beamRenderer.endColor = blockColor;
                return;
            }

            Transform receiver = GetReceiver(_step);

            // 当前 step 没有接收者：显示一条未连接的射线，而不是直接隐藏。
            if (receiver == null)
            {
                SetPoweredSpeaker(null);
                SetHitReceiver(null);

                IsConnected = false;
                IsAligned = false;

                Vector3 origin = beamStart.position;
                Vector3 direction = transform.forward;
                SetBeamVisible(true);
                beamRenderer.SetPosition(0, origin);
                beamRenderer.SetPosition(1, origin + direction * beamRange);
                beamRenderer.startColor = beamRenderer.endColor = blockColor;
                return;
            }

            SpeakerDevice receiverSpeaker = receiver.GetComponentInParent<SpeakerDevice>();
            SpeakerTarget receiverTarget = receiver.GetComponentInParent<SpeakerTarget>();

            if (receiverSpeaker == this)
                receiverSpeaker = null;

            Vector3 beamOrigin = beamStart.position;
            Vector3 anchor = GetReceiverAnchor(receiver);
            Vector3 beamDirection = anchor - beamOrigin;
            float distance = beamDirection.magnitude;
            bool inRange = distance <= beamRange;

            if (!inRange)
                anchor = beamOrigin + beamDirection.normalized * beamRange;

            bool connected = inRange &&
                             (receiverSpeaker != null || receiverTarget != null);

            SetPoweredSpeaker(connected ? receiverSpeaker : null);
            SetHitReceiver(connected ? receiverTarget : null);

            IsConnected = connected;
            IsAligned = connected;

            SetBeamVisible(true);
            beamRenderer.SetPosition(0, beamOrigin);
            beamRenderer.SetPosition(1, anchor);
            beamRenderer.startColor = beamRenderer.endColor =
                connected ? alignColor : blockColor;
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
            IsAligned = false;
            SetBeamVisible(false);
        }

        private void SetBeamVisible(bool visible)
        {
            if (beamRenderer != null)
                beamRenderer.enabled = visible;
        }

        private void OnDrawGizmosSelected()
        {
            if (beamStart == null)
                return;

            if (primaryReceiver != null)
            {
                Gizmos.color = _step == primaryReceiverStep ? Color.green : Color.yellow;
                Gizmos.DrawLine(beamStart.position, primaryReceiver.position);
            }

            if (extraReceivers == null)
                return;

            for (int i = 0; i < extraReceivers.Count; i++)
            {
                StepReceiver item = extraReceivers[i];
                if (item == null || item.Receiver == null)
                    continue;

                Gizmos.color = _step == item.Step ? Color.green : Color.yellow;
                Gizmos.DrawLine(beamStart.position, item.Receiver.position);
            }
        }

        #endregion
    }
}
