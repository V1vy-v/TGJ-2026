using Gameplay.Player.Input;
using Unity.Cinemachine;
using UnityEngine;

namespace Gameplay.Player
{
    public class PlayerCameraRig
    {
        private readonly PlayerInputReader _playerInput;
        private readonly CinemachineOrbitalFollow _orbital;
        private readonly float _sensitivity;
        private readonly bool _invertY;
        public Transform CameraTransform { get; private set; }

        public PlayerCameraRig(PlayerInputReader playerInput, CinemachineOrbitalFollow orbital, Transform cameraTransform, float sensitivity, bool invertY)
        {
            _playerInput = playerInput;
            _orbital = orbital;
            _sensitivity = sensitivity;
            _invertY = invertY;
            CameraTransform = cameraTransform;
        }

        public void Tick()
        {
            var look = _playerInput.LookValue;
            
            // 水平
            var h = _orbital.HorizontalAxis.Value + look.x * _sensitivity;
            h = Mathf.Repeat(h + 180f, 360f) - 180f;
            _orbital.HorizontalAxis.Value = h;
            
            // 垂直
            var v = _orbital.VerticalAxis.Value
                    + look.y * _sensitivity * (_invertY ? 1f : -1f);
            _orbital.VerticalAxis.Value = Mathf.Clamp(
                v,
                _orbital.VerticalAxis.Range.x,
                _orbital.VerticalAxis.Range.y);
        }
    }
}