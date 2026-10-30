using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Interaction.Puzzle.Energy
{
    public class EnergyPlatform : MonoBehaviour
    {
        [Header("绑定装置")]
        [SerializeField] private EnergyDevice energyDevice1;
        [SerializeField] private EnergyDevice energyDevice2;

        [Header("高度")]
        [SerializeField] private float heightPerUnit = 0.5f;
        [SerializeField] private float maxOffset = 3f;

        [Header("移动")]
        [SerializeField] private float moveSpeed = 1f;

        [Header("乘客")]
        [SerializeField] private Vector3 passengerCheckSize = new(3f, 0.4f, 3f);
        [SerializeField] private Vector3 passengerCheckOffset = new(0f, 0.2f, 0f);
        [SerializeField] private LayerMask passengerMask = ~0;

        private readonly Collider[] _hits = new Collider[8];
        private readonly List<CharacterController> _passengers = new();

        private float _baseHeight;
        private float _targetHeight;
        private int _initialDiff;
        private bool _devicesLocked;

        public float BaseHeight => _baseHeight;
        public float TargetHeight => _targetHeight;

        private void OnEnable()
        {
            if (energyDevice1 == null || energyDevice2 == null)
            {
                Debug.LogError(
                    $"[EnergyPlatform] {name} 的 energyDevice1/energyDevice2 未赋值", this);
                enabled = false;
                return;
            }

            energyDevice1.EnergyChanged += RefreshTarget;
            energyDevice2.EnergyChanged += RefreshTarget;
        }

        private void Start()
        {
            _baseHeight = transform.position.y;
            _initialDiff = energyDevice1.Energy - energyDevice2.Energy;

            RefreshTarget();

            Vector3 position = transform.position;
            transform.position = new Vector3(position.x, _targetHeight, position.z);

            UnlockDevices();
        }

        private void OnDisable()
        {
            if (energyDevice1 != null)
                energyDevice1.EnergyChanged -= RefreshTarget;

            if (energyDevice2 != null)
                energyDevice2.EnergyChanged -= RefreshTarget;

            UnlockDevices();
        }

        private void Update()
        {
            Vector3 before = transform.position;
            float y = Mathf.MoveTowards(
                before.y, _targetHeight, moveSpeed * Time.deltaTime);

            transform.position = new Vector3(before.x, y, before.z);

            Vector3 delta = transform.position - before;
            if (delta.sqrMagnitude > 0f)
                CarryPassengers(delta);

            if (Mathf.Abs(transform.position.y - _targetHeight) < 0.001f)
                UnlockDevices();
        }

        private void RefreshTarget()
        {
            int diff = energyDevice1.Energy - energyDevice2.Energy;
            int offsetUnits = diff - _initialDiff;

            _targetHeight = _baseHeight +
                            Mathf.Clamp(offsetUnits * heightPerUnit, -maxOffset, maxOffset);

            if (Mathf.Abs(_targetHeight - transform.position.y) > 0.001f)
                LockDevices();
        }

        private void LockDevices()
        {
            if (_devicesLocked)
                return;

            _devicesLocked = true;
            energyDevice1.SetLock(true);
            energyDevice2.SetLock(true);
        }

        private void UnlockDevices()
        {
            if (!_devicesLocked)
                return;

            _devicesLocked = false;

            if (energyDevice1 != null)
                energyDevice1.SetLock(false);

            if (energyDevice2 != null)
                energyDevice2.SetLock(false);
        }

        private void CarryPassengers(Vector3 delta)
        {
            int count = Physics.OverlapBoxNonAlloc(
                transform.TransformPoint(passengerCheckOffset),
                passengerCheckSize * 0.5f,
                _hits,
                Quaternion.identity,
                passengerMask,
                QueryTriggerInteraction.Ignore);

            _passengers.Clear();

            for (int i = 0; i < count; i++)
            {
                var controller = _hits[i].GetComponentInParent<CharacterController>();
                if (controller == null) continue;
                if (_passengers.Contains(controller)) continue;

                _passengers.Add(controller);
            }

            for (int i = 0; i < _passengers.Count; i++)
                _passengers[i].Move(delta);
        }
    }
}
