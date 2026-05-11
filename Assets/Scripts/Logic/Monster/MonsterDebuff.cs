using UnityEngine;

namespace Logic.Monster
{
    public abstract class MonsterDebuff
    {
        public float Duration { get; protected set; }
        public bool IsFinished => Duration <= 0f;

        public virtual void OnApply(MonsterModel monster) { }
        public virtual void OnRemove(MonsterModel monster) { }

        public virtual void Tick(MonsterModel monster, float deltaTime)
        {
            if (Duration != float.MaxValue)
                Duration -= deltaTime;
        }

        public virtual float ModifyMoveSpeed(float baseValue) => baseValue;
        public virtual int ModifyOutgoingDamage(int baseValue) => baseValue;
    }
    
    public class SlowDebuff : MonsterDebuff
    {
        private readonly float slowPercent;

        public SlowDebuff(float slowPercent)
        {
            Duration = float.MaxValue; // бесконечный
            this.slowPercent = Mathf.Clamp01(slowPercent);
        }

        public override float ModifyMoveSpeed(float baseValue)
        {
            return baseValue * (1f - slowPercent);
        }
    }
    
    public class HealthDebuff : MonsterDebuff
    {
        private readonly float interval;
        private readonly int damagePerTick;
        private float timer;

        public HealthDebuff(float duration, float interval, int damagePerTick)
        {
            Duration = duration;
            this.interval = interval;
            this.damagePerTick = damagePerTick;
        }

        public override void Tick(MonsterModel monster, float deltaTime)
        {
            base.Tick(monster, deltaTime);

            if (monster.IsDead)
                return;

            timer += deltaTime;

            if (timer >= interval)
            {
                timer = 0f;
                monster.TakeDamage(damagePerTick);
            }
        }
    }
}