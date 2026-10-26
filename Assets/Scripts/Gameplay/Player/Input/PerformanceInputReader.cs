using System;
using UnityEngine.InputSystem;

namespace Gameplay.Player.Input
{
    /// <summary>
    /// 绑定 Performance map，把三个音符和退出键转成事件。
    /// 由 PerformanceMode 持有，生命周期与演奏模式一致。
    /// </summary>
    public sealed class PerformanceInputReader : IDisposable
    {
        #region 状态

        private readonly InputAction _noteA;
        private readonly InputAction _noteB;
        private readonly InputAction _noteC;
        private readonly InputAction _exit;

        public event Action<int> NotePressed;
        public event Action ExitPressed;

        #endregion

        #region 生命周期

        public PerformanceInputReader(InputActionMap map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            _noteA = map.FindAction("NoteA", true);
            _noteB = map.FindAction("NoteB", true);
            _noteC = map.FindAction("NoteC", true);
            _exit = map.FindAction("Exit", true);

            _noteA.performed += OnNoteA;
            _noteB.performed += OnNoteB;
            _noteC.performed += OnNoteC;
            _exit.performed += OnExit;
        }

        public void Dispose()
        {
            _noteA.performed -= OnNoteA;
            _noteB.performed -= OnNoteB;
            _noteC.performed -= OnNoteC;
            _exit.performed -= OnExit;
        }

        #endregion

        #region 内部实现

        private void OnNoteA(InputAction.CallbackContext _) => NotePressed?.Invoke(0);
        private void OnNoteB(InputAction.CallbackContext _) => NotePressed?.Invoke(1);
        private void OnNoteC(InputAction.CallbackContext _) => NotePressed?.Invoke(2);
        private void OnExit(InputAction.CallbackContext _) => ExitPressed?.Invoke();

        #endregion
    }
}
