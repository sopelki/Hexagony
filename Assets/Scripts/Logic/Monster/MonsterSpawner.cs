﻿using System.Collections.Generic;
using UnityEngine;
using Logic.Unit;
using Interfaces;
using Logic.Castle;
using UnityEngine.Tilemaps;
using View;

namespace Logic.Monster
{
    public class MonsterSpawner : ITickable
    {
        private readonly List<Vector2Int> spawnHexes;
        // private CastleView castleView;

        private readonly Field.Field field;
        private readonly MonsterSystem monsterSystem;
        private readonly UnitSystem unitSystem;
        private readonly List<MonsterData> availableMonsters;
        private readonly Tilemap tilemap;

        private float spawnTimer;
        private readonly float spawnInterval = 3f; // каждые 3 секунды

        public MonsterSpawner(
            List<Vector2Int> spawnHexes,
            Field.Field field,
            MonsterSystem monsterSystem,
            UnitSystem unitSystem,
            List<MonsterData> availableMonsters,
            Tilemap tilemap)
        {
            this.spawnHexes = spawnHexes;
            this.field = field;
            this.monsterSystem = monsterSystem;
            this.unitSystem = unitSystem;
            this.availableMonsters = availableMonsters;
            this.tilemap = tilemap;
        }

        public void Tick()
        {
            spawnTimer += Core.TickManager.Instance.tickInterval;

            if (spawnTimer < spawnInterval)
                return;

            spawnTimer = 0f;

            Spawn();
        }

        private void Spawn()
        {
            var castle = CastleSystem.Instance;
            
            // if (castleView == null)
            // {
            //     castleView = Object.FindAnyObjectByType<CastleView>();
            // }
            
            var hex = spawnHexes[Random.Range(0, spawnHexes.Count)];

            var data = availableMonsters[Random.Range(0, availableMonsters.Count)];
            var hexObj = field.GetHex(hex);
            if (hexObj == null)
                return;
            
            var world = tilemap.GetCellCenterWorld(hexObj.offset);

            // создаём монстра без стратегий
            var monster = new MonsterModel(
                world,
                hex,
                data,
                null,
                null
            );

            // создаём стратегии
            var movement = new HexMoveToTargetStrategy(
                monster,
                field,
                tilemap
            );

            var attack = new MonsterAttackStrategy(monster, unitSystem);
            
            monster.SetStrategies(movement, attack);

            monsterSystem.AddMonster(monster);
        }
    }
}