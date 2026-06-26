using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Castle;

namespace Logic.Monster
{
    public class MonsterSystem
    {
        private readonly List<MonsterModel> monsters = new();

        public event Action<MonsterModel> OnMonsterCreated;
        public event Action<MonsterModel> OnMonsterDied;

        public void AddMonster(MonsterModel monster)
        {
            monsters.Add(monster);
            OnMonsterCreated?.Invoke(monster);
        }

        public IReadOnlyList<MonsterModel> GetAllMonsters()
        {
            return monsters;
        }

        public void SubscribeToCastle(CastleModel castle)
        {
            if (castle != null)
                castle.OnCastleDestroyed += HandleCastleDestroyed;
        }

        public void Tick()
        {
            for (var i = monsters.Count - 1; i >= 0; i--)
            {
                var monster = monsters[i];

                monster.Tick();

                if (!monster.IsDead)
                    continue;
                OnMonsterDied?.Invoke(monster);
                monsters.RemoveAt(i);
            }
        }

        private void HandleCastleDestroyed()
        {
            foreach (var monster in monsters.Where(monster => !monster.IsDead))
                monster.StopAndIdle();
        }
    }
}