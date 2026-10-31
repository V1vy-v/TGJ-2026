using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Interaction.Puzzle.SpeakerChain
{
    /// <summary>
    /// 升降台：被 Puzzle 一次性激活后，在 up / down 之间循环，可携带乘客。
    /// </summary>
    public class SpeakerPlatform : MonoBehaviour
    {
        [Header("移动范围（世界 Y）")]
        [SerializeField] private float down;
        [SerializeField] private float up;
        [SerializeField] private float speed = 1f;

        [Header("乘客检测")]
        [SerializeField] private Vector3 passengerCheckSize = new(3f, 0.4f, 3f);
        [SerializeField] private Vector3 passengerCheckOffset = new(0f, 0.2f, 0f);
        [SerializeField] private LayerMask passengerMask = ~0;

        private readonly Collider[] _hits = new Collider[8];
        private readonly List<CharacterController> _passengers = new();

        private bool _unlocked;
        private int _direction = 1;

        public void Unlock()
        {
            _unlocked = true;
        }

        private void Update()
        {
            if (!_unlocked)
                return;

            float targetY = _direction > 0 ? up : down;
            Vector3 before = transform.position;

            Vector3 next = Vector3.MoveTowards(
                before,
                new Vector3(before.x, targetY, before.z),
                speed * Time.deltaTime);

            transform.position = next;

            if (Mathf.Abs(next.y - targetY) < 0.001f)
                _direction = -_direction;

            Vector3 delta = next - before;
            if (delta.sqrMagnitude > 0f)
                CarryPassengers(delta);
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
