using System;
using System.Collections.Generic;

namespace Logic.Tower
{
    public class TowersModel
    {
        private readonly List<TowerModel> towers = new();
        public IReadOnlyList<TowerModel> Towers => towers;

        public event Action<TowerModel> OnChanged;

        public void AddTower(TowerModel tower)
        {
            towers.Add(tower);
            OnChanged?.Invoke(tower);
        }

        public void Clear()
        {
            towers.Clear();
        }
    }
}