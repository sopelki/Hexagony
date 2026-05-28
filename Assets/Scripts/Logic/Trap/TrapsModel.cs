using System;
using System.Collections.Generic;
using UnityEngine;

namespace Logic.Trap
{
    public class TrapsModel
    {
        private readonly List<TrapModel> traps = new();
        public IReadOnlyList<TrapModel> Traps => traps;
        public event Action<TrapModel> OnTrapAdded;
        public event Action<TrapModel> OnTrapRemoved;

        public void AddTrap(TrapModel trap)
        {
            traps.Add(trap);
            OnTrapAdded?.Invoke(trap);
        }

        public void RemoveTrap(TrapModel trap)
        {
            if (traps.Remove(trap))
            {
                Debug.Log("2. МОДЕЛЬ: Капкан успешно удален из списка. Рассылаю событие!");
                OnTrapRemoved?.Invoke(trap);
            }
        }

        public void Clear()
        {
            traps.Clear();
        }
    }
}