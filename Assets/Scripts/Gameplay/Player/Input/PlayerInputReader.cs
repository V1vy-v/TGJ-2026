using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Player.Input
{
    public sealed class PlayerInputReader : IDisposable
    {
        private readonly InputActionMap _map;
        private readonly InputAction _move;
        private readonly InputAction _look;
        private readonly InputAction _jump;
        private readonly InputAction _interact;
        private readonly InputAction _glasses;
        private readonly InputAction _pulse;
        private readonly InputAction _scroll;

        public Vector2 MoveValue { get; private set; }
        public Vector2 LookValue { get; private set; }
        public float ScrollValue { get; private set; }

        public event Action JumpPressed;
        public event Action InteractPressed;
        public event Action GlassesPressed;
        public event Action PulsePressed;
        public event Action RecordPressed;

        public PlayerInputReader(InputActionMap map)
        {
            _map = map;

            _move = _map.FindAction("Move", true);
            _look = _map.FindAction("Look", true);
            _jump = _map.FindAction("Jump", true);
            _interact = _map.FindAction("Interact", true);
            _glasses = _map.FindAction("ToggleGlasses", true);
            _pulse = _map.FindAction("HearingPulse", true);
            _scroll = _map.FindAction("Scroll", true);

            _jump.performed += OnJump;
            _interact.performed += OnInteract;
            _glasses.performed += OnGlasses;
            _pulse.performed += OnPulse;
        }

        public void Poll()
        {
            if (!_map.enabled)
            {
                MoveValue = Vector2.zero;
                LookValue = Vector2.zero;
                ScrollValue = 0f;
                return;
            }

            MoveValue = _move.ReadValue<Vector2>();
            LookValue = _look.ReadValue<Vector2>();
            ScrollValue = _scroll.ReadValue<float>();
        }

        public void Dispose()
        {
            _jump.performed -= OnJump;
            _interact.performed -= OnInteract;
            _glasses.performed -= OnGlasses;
            _pulse.performed -= OnPulse;
        }

        private void OnJump(InputAction.CallbackContext _) => JumpPressed?.Invoke();
        private void OnInteract(InputAction.CallbackContext _) => InteractPressed?.Invoke();
        private void OnGlasses(InputAction.CallbackContext _) => GlassesPressed?.Invoke();
        private void OnPulse(InputAction.CallbackContext _) => PulsePressed?.Invoke();
        private void OnRecord(InputAction.CallbackContext _) => RecordPressed?.Invoke();
    }
}