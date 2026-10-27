using System;
using Gameplay.Player.Input;
using UnityEngine;

namespace Gameplay.Player.Act
{
    public class PlayerMotor
    {
        private readonly CharacterController _controller;
        private readonly PlayerInputReader _playerInput;
        private readonly PlayerConfig _playerConfig;
        private readonly Transform _camera;
        private float _verticalVelocity = -2f;

        public PlayerMotor(CharacterController controller, PlayerInputReader playerInput, PlayerConfig playerConfig,
            Transform mainCameraTransform)
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _playerInput = playerInput ?? throw new ArgumentNullException(nameof(playerInput));
            _playerConfig = playerConfig ?? throw new ArgumentNullException(nameof(playerConfig));
            _camera = mainCameraTransform ?? throw new ArgumentNullException(nameof(mainCameraTransform));
            
            _playerInput.JumpPressed += Jump;
        }
        
        public void Tick(float deltaTime)
        {
            MoveAndRotate(deltaTime);
        }

        private void MoveAndRotate(float deltaTime)
        {
            Vector2 move = _playerInput.MoveValue;

            Vector3 forward = Vector3.ProjectOnPlane(_camera.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(_camera.right, Vector3.up).normalized;
            Vector3 dir = forward * move.y + right * move.x;

            if (dir.sqrMagnitude > 0.0001f)
            {
                dir.Normalize();

                Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);
                _controller.transform.rotation = Quaternion.RotateTowards(
                    _controller.transform.rotation,
                    targetRotation,
                    _playerConfig.TurnSpeed * deltaTime);
            }

            _verticalVelocity -= _playerConfig.Gravity * deltaTime;
            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;

            Vector3 motion = dir * (_playerConfig.MoveSpeed * deltaTime)
                             + Vector3.up * (_verticalVelocity * deltaTime);

            _controller.Move(motion);
        }
        
        private void Jump()
        {
            if (!_controller.isGrounded) return;
            _verticalVelocity = Mathf.Sqrt(2f * _playerConfig.Gravity * _playerConfig.JumpHeight);
        }
    }
}
