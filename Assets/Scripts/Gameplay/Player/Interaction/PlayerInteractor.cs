using System;
using System.Collections.Generic;
using Gameplay.Interaction.Interface;
using UnityEngine;

namespace Gameplay.Player.Interaction
{
    public class PlayerInteractor
    {
        private const float CheckInterval = 0.1f;
        private const float ScrollThreshold = 1f;
        private const float ScrollCooldown = 0.12f;

        private readonly Transform _origin;
        private readonly PlayerContext _context;
        private readonly LayerMask _interactableLayer;
        private readonly float _radius;

        private readonly Collider[] _buffer = new Collider[16];

        private float _nextCheckTime;
        private float _scrollAccum;
        private float _nextScrollTime;

        private IInteractable _interactable;
        private IReadOnlyList<InteractionOption> _options;
        private int _highlightedIndex;

        #region 事件

        public event Action<IInteractable> InteractableChanged;
        public event Action<IReadOnlyList<InteractionOption>, int> OptionsChanged;
        public event Action<int> HighlightChanged;
        public event Action<InteractionOption> InteractionRejected;

        #endregion

        public PlayerInteractor(Transform origin, PlayerContext context,
            LayerMask interactableLayer, float radius)
        {
            _origin = origin;
            _context = context;
            _interactableLayer = interactableLayer;
            _radius = radius;
        }

        public IReadOnlyList<InteractionOption> CurrentOptions => _options;

        public void Tick(float scrollValue)
        {
            if (_options != null && _options.Count > 0)
            {
                _scrollAccum += scrollValue;

                if (Mathf.Abs(_scrollAccum) >= ScrollThreshold)
                {
                    if (Time.unscaledTime >= _nextScrollTime)
                    {
                        int direction = _scrollAccum > 0f ? -1 : 1;
                        ChangeOption(direction);
                        _nextScrollTime = Time.unscaledTime + ScrollCooldown;
                    }

                    _scrollAccum = 0f;
                }
            }
            else
            {
                _scrollAccum = 0f;
            }

            if (Time.time < _nextCheckTime) return;

            _nextCheckTime = Time.time + CheckInterval;
            Detect();
        }

        public bool TryInteract()
        {
            if (_options == null || _options.Count == 0)
                return false;

            ExecuteOption();
            return true;
        }

        private void ExecuteOption()
        {
            var option = _options[_highlightedIndex];
            bool success = option.Execute(_context);

            if (success)
            {
                _nextCheckTime = Time.time + CheckInterval;
                Detect();
            }
            else
            {
                InteractionRejected?.Invoke(option);
            }
        }

        private void Detect()
        {
            int count = Physics.OverlapSphereNonAlloc(
                _origin.position, _radius, _buffer, _interactableLayer);

            IInteractable nearest = null;
            IReadOnlyList<InteractionOption> nearestOptions = null;
            float nearestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var target = _buffer[i].GetComponentInParent<IInteractable>();
                if (target == null) continue;
                if (!target.CanInteract(_context)) continue;

                var options = target.GetOptions(_context);
                if (options == null || options.Count == 0) continue;

                float distance = Vector3.Distance(
                    _origin.position, _buffer[i].transform.position);

                if (distance >= nearestDistance) continue;

                nearest = target;
                nearestOptions = options;
                nearestDistance = distance;
            }

            if (ReferenceEquals(nearest, _interactable)) return;

            _interactable = nearest;
            _options = nearestOptions;
            _highlightedIndex = 0;

            InteractableChanged?.Invoke(_interactable);
            OptionsChanged?.Invoke(_options, 0);
        }

        private void ChangeOption(int direction)
        {
            _highlightedIndex = Mathf.Clamp(
                _highlightedIndex + direction, 0, _options.Count - 1);

            HighlightChanged?.Invoke(_highlightedIndex);
        }
    }
}
