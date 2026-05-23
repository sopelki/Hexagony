using System;
using System.Collections.Generic;
using UnityEngine;
using Logic.Monster;

namespace Logic.Trap
{
    public class TrapModel
    {
        public TrapData Data { get; }
        public List<Vector2Int> Hexes { get; }
        public event Action<TrapModel> OnTriggered;

        public bool IsTriggered { get; private set; }

        public float TickTimer;

        public readonly Dictionary<MonsterModel, SlowDebuff> ActiveSlowDebuffs = new();

        public TrapModel(TrapData data, List<Vector2Int> hexes)
        {
            Data = data;
            Hexes = hexes;
        }

        public void Trigger()
        {
            if (IsTriggered)
                return;

            IsTriggered = true;
            OnTriggered?.Invoke(this);
        }
    }
}