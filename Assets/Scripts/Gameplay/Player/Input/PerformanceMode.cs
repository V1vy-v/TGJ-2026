using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Gameplay.Player.Input
{
    /// <summary>
    /// 玩家的演奏模式：接收 A/S/D 输入，与 NPC 给出的音符序列比对。
    /// 由 PlayerController 持有，Begin/End 控制一次演奏会话。
    /// </summary>
    public sealed class PerformanceMode : IDisposable
    {
        #region 状态

        private readonly InputModeController _inputMode;
        private readonly PerformanceInputReader _reader;

        private readonly List<int> _expectedNotes = new();
        private int _nextIndex;
        private bool _disposed;

        public bool IsActive { get; private set; }

        /// <summary>true 表示完整演奏成功，false 表示失败或退出。</summary>
        public event Action<bool> Finished;

        #endregion

        #region 生命周期

        public PerformanceMode(InputModeController inputMode, InputActionMap performanceMap)
        {
            _inputMode = inputMode ?? throw new ArgumentNullException(nameof(inputMode));
            _reader = new PerformanceInputReader(performanceMap);

            _reader.NotePressed += OnNotePressed;
            _reader.ExitPressed += OnExitPressed;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _reader.NotePressed -= OnNotePressed;
            _reader.ExitPressed -= OnExitPressed;
            _reader.Dispose();

            if (IsActive)
                _inputMode.SwitchTo(InputMode.Player);

            IsActive = false;
        }

        #endregion

        #region 对外接口

        public void Begin(IReadOnlyList<int> expectedNotes)
        {
            if (IsActive)
                return;

            if (expectedNotes == null || expectedNotes.Count == 0)
                throw new ArgumentException("演奏序列不能为空", nameof(expectedNotes));

            _expectedNotes.Clear();
            for (int i = 0; i < expectedNotes.Count; i++)
                _expectedNotes.Add(expectedNotes[i]);

            _nextIndex = 0;
            IsActive = true;

            _inputMode.SwitchTo(InputMode.NpcPerformance);
        }

        public void Cancel()
        {
            if (IsActive)
                Finish(false);
        }

        #endregion

        #region 内部实现

        private void OnNotePressed(int note)
        {
            if (!IsActive)
                return;

            if (_expectedNotes[_nextIndex] != note)
            {
                Finish(false);
                return;
            }

            _nextIndex++;
            if (_nextIndex >= _expectedNotes.Count)
                Finish(true);
        }

        private void OnExitPressed()
        {
            Cancel();
        }

        private void Finish(bool success)
        {
            if (!IsActive)
                return;

            IsActive = false;
            _inputMode.SwitchTo(InputMode.Player);
            Finished?.Invoke(success);
        }

        #endregion
    }
}
