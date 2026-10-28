using System;
using Gameplay.Interaction;
using UnityEngine;

namespace Gameplay.Player.Interaction
{
    public class PlayerInteractor
    {
        private const float CheckInterval = 0.1f;

        private readonly Transform _origin;
        private readonly PlayerContext _context;
        private readonly LayerMask _interactableLayer;
        private readonly float _radius;

        private readonly Collider[] _buffer = new Collider[16];

        private IInteractable _current;
        private float _nextCheckTime;

        public IInteractable Current => _current;
        public event Action<IInteractable> CurrentChanged;

        public PlayerInteractor(Transform origin, PlayerContext context,
            LayerMask interactableLayer, float radius)
        {
            _origin = origin;
            _context = context;
            _interactableLayer = interactableLayer;
            _radius = radius;

            // 测试
            CurrentChanged += interactable =>
            {
                if(interactable == null) return;
                Debug.Log(interactable.Prompt);
            };
        }

        public void Tick()
        {
            if (Time.time < _nextCheckTime) return;

            _nextCheckTime = Time.time + CheckInterval;
            Detect();
        }

        public bool TryInteract()
        {
            if (_current == null) return false;

            _current.Interact(_context);
            return true;
        }

        private void Detect()
        {
            int count = Physics.OverlapSphereNonAlloc(
                _origin.position, _radius, _buffer, _interactableLayer);

            IInteractable nearest = null;
            float nearestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var target = _buffer[i].GetComponentInParent<IInteractable>();
                if (target == null) continue;
                if (!target.CanInteract(_context)) continue;

                float distance = Vector3.Distance(
                    _origin.position, _buffer[i].transform.position);

                if (distance >= nearestDistance) continue;

                nearest = target;
                nearestDistance = distance;
            }

            if (ReferenceEquals(nearest, _current)) return;

            _current = nearest;
            CurrentChanged?.Invoke(_current);
        }
    }
}