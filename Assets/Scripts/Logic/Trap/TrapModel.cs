using System.Collections.Generic;
using UnityEngine;
using Logic.Monster;
using Logic.Trap.Logic.Trap;

namespace Logic.Trap
{
    public class TrapModel
    {
        public TrapData Data { get; }
        public Vector2Int Hex { get; }

        public bool IsTriggered { get; private set; }

        // Для DamageZone
        public float TickTimer;

        // Для SlowZone — храним конкретные дебаффы
        public readonly Dictionary<MonsterModel, SlowDebuff> ActiveSlowDebuffs = new();

        public TrapModel(TrapData data, Vector2Int hex)
        {
            Data = data;
            Hex = hex;
        }

        public void Trigger()
        {
            IsTriggered = true;
        }
    }
}