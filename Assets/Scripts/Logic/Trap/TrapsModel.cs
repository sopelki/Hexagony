using System;
using System.Collections.Generic;
using UnityEngine;

namespace Logic.Trap
{
    public class TrapsModel
    {
        public event Action<TrapModel> OnTrapAdded;
        public event Action<TrapModel> OnTrapRemoved;

        private readonly List<TrapModel> traps = new();
        public IReadOnlyList<TrapModel> Traps => traps;

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
    }
}