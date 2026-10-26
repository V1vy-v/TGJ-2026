using System;
using Gameplay.Player.Input;
using UnityEngine;

namespace Gameplay.Player.Act
{
    public class PlayerMotor
    {
        private Rigidbody _body;
        private PlayerInputReader _playerInput;
        private PlayerConfig _playerConfig;

        public PlayerMotor(Rigidbody body, PlayerInputReader playerInput,  PlayerConfig playerConfig)
        {
            _body = body ?? throw new ArgumentNullException(nameof(body));
            _playerInput = playerInput ?? throw new ArgumentNullException(nameof(playerInput));
            _playerConfig = playerConfig ?? throw new ArgumentNullException(nameof(playerConfig));
            
            _playerInput.JumpPressed += Jump;
        }
        
        public void Tick()
        {
            Move();
        }

        private void Move()
        {
            _body.linearVelocity = new Vector3(_playerInput.MoveValue.x * _playerConfig.MoveSpeed, _body.linearVelocity.y, _playerInput.MoveValue.y * _playerConfig.MoveSpeed);
        }

        private void Jump()
        {
            _body.AddForce(new Vector3(0, 300, 0));
        }
    }
}
