using System;
using System.Collections.Generic;
using Logic.Monster;
using UnityEngine;

namespace Logic.Trap
{
    public class TrapModel
    {
        public readonly Dictionary<MonsterModel, SlowDebuff> ActiveSlowDebuffs = new();

        public float TickTimer;

        public TrapModel(TrapData data, List<Vector2Int> hexes)
        {
            Data = data;
            Hexes = hexes;
        }

        public TrapData Data { get; }
        public List<Vector2Int> Hexes { get; }

        public bool IsTriggered { get; private set; }
        public event Action<TrapModel> OnTriggered;

        public void Trigger()
        {
            if (IsTriggered)
                return;

            IsTriggered = true;
            OnTriggered?.Invoke(this);
        }
    }
}