using UnityEngine;

namespace Logic.Unit
{
    public abstract class Buff
    {
        public virtual int ModifyAttack(int baseValue) => baseValue;

        public virtual int ModifyMaxHealth(int baseValue) => baseValue;

        public virtual float ModifyMoveSpeed(float baseValue) => baseValue;
    }

    public class AttackPercentBuff : Buff
    {
        public AttackPercentBuff(float percent) => multiplier = percent;

        private readonly float multiplier;

        public override int ModifyAttack(int baseValue) => baseValue + Mathf.RoundToInt(baseValue * multiplier);
    }

    public class HealthPercentBuff : Buff
    {
        public HealthPercentBuff(float percent) => multiplier = percent;

        private readonly float multiplier;

        public override int ModifyMaxHealth(int baseValue) => baseValue + Mathf.RoundToInt(baseValue * multiplier);
    }
}