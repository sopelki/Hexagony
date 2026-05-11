using System;
using System.Collections.Generic;

namespace Logic.Trap
{
    public class TrapsModel
    {
        public event Action<TrapModel> OnTrapAdded;

        private readonly List<TrapModel> traps = new();
        public IReadOnlyList<TrapModel> Traps => traps;

        public void AddTrap(TrapModel trap)
        {
            traps.Add(trap);
            OnTrapAdded?.Invoke(trap);
        }
        
        public void RemoveTrap(TrapModel trap)
        {
            traps.Remove(trap);
        }
    }
}