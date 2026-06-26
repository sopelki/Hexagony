using UnityEngine;

namespace Logic.Monster
{
    public abstract class MonsterDebuff
    {
        public float Duration { get; protected set; }
        public bool IsFinished => Duration <= 0f;

        public virtual float ModifyMoveSpeed(float baseValue)
        {
            return baseValue;
        }

        public virtual int ModifyOutgoingDamage(int baseValue)
        {
            return baseValue;
        }

        public virtual void OnApply(MonsterModel monster)
        {
        }

        public virtual void OnRemove(MonsterModel monster)
        {
        }

        public virtual void Tick(MonsterModel monster, float deltaTime)
        {
            if (!Mathf.Approximately(Duration, float.MaxValue)) Duration -= deltaTime;
        }
    }

    public class SlowDebuff : MonsterDebuff
    {
        public SlowDebuff(float slowPercent)
        {
            Duration = float.MaxValue;
            this.slowPercent = Mathf.Clamp01(slowPercent);
        }

        private readonly float slowPercent;

        public override float ModifyMoveSpeed(float baseValue)
        {
            return baseValue * (1f - slowPercent);
        }
    }

    public class HealthDebuff : MonsterDebuff
    {
        public HealthDebuff(float duration, float interval, int damagePerTick)
        {
            Duration = duration;
            this.interval = interval;
            this.damagePerTick = damagePerTick;
        }

        private readonly int damagePerTick;
        private readonly float interval;

        private float timer;

        public override void Tick(MonsterModel monster, float deltaTime)
        {
            base.Tick(monster, deltaTime);

            if (monster.IsDead) return;

            timer += deltaTime;

            if (timer >= interval)
            {
                timer = 0f;
                monster.TakeDamage(damagePerTick);
            }
        }
    }
}