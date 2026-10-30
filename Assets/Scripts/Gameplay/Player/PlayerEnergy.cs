using System;
using UnityEngine;

namespace Gameplay.Player
{
    public class PlayerEnergy
    {
        private readonly int _capacity;
        private int _current;

        public int Current => _current;
        public int Capacity => _capacity;

        public event Action<int> EnergyChanged;

        public PlayerEnergy(int initial, int capacity)
        {
            _capacity = Mathf.Max(0, capacity);
            _current = Mathf.Clamp(initial, 0, _capacity);
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0)
                return false;

            if (_current - amount < 0)
                return false;

            _current -= amount;
            EnergyChanged?.Invoke(_current);
            return true;
        }

        public bool TryRecycle(int amount)
        {
            if (amount <= 0)
                return false;

            if (_current + amount > _capacity)
                return false;

            _current += amount;
            EnergyChanged?.Invoke(_current);
            return true;
        }
    }
}
