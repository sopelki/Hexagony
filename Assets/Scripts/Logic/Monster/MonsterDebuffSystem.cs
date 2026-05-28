using System.Collections.Generic;
using System.Linq;

namespace Logic.Monster
{
    public class MonsterDebuffSystem
    {
        private readonly List<MonsterDebuff> buffs = new();
        private readonly MonsterModel monster;

        public MonsterDebuffSystem(MonsterModel monster)
        {
            this.monster = monster;
        }

        public void AddBuff(MonsterDebuff buff)
        {
            if (buff == null)
                return;

            buffs.Add(buff);
            buff.OnApply(monster);
        }

        public void RemoveBuff(MonsterDebuff buff)
        {
            if (buff == null)
                return;

            if (buffs.Remove(buff))
                buff.OnRemove(monster);
        }

        public void Tick(float deltaTime)
        {
            foreach (var buff in buffs.ToList())
            {
                buff.Tick(monster, deltaTime);

                if (buff.IsFinished)
                {
                    buff.OnRemove(monster);
                    buffs.Remove(buff);
                }
            }
        }

        public float ModifyMoveSpeed(float baseSpeed)
        {
            return buffs.Aggregate(baseSpeed,
                (current, buff) => buff.ModifyMoveSpeed(current));
        }

        public int ModifyOutgoingDamage(int baseValue)
        {
            return buffs.Aggregate(baseValue,
                (current, buff) => buff.ModifyOutgoingDamage(current));
        }
    }
}