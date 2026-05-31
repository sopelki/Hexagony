using System;
using System.Collections.Generic;
using System.Linq;
using Audio;
using Core;
using Interfaces;
using Logic.Castle;
using Logic.Monster;
using Logic.Projectile;
using UnityEngine;

namespace Logic.Tower
{
    public class TowerSystem : ITickable
    {
        private readonly CastleSystem castleSystem;
        private readonly MonsterSystem monsterSystem;
        private readonly ProjectileSystem projectileSystem;
        private readonly SoundData soundData;
        private readonly TowersModel towersModel;

        private bool firstTowerPlaced;

        public TowerSystem(
            CastleSystem castleSystem,
            TowersModel towersModel,
            MonsterSystem monsterSystem,
            ProjectileSystem projectileSystem,
            SoundData soundData)
        {
            this.castleSystem = castleSystem;
            this.towersModel = towersModel;
            this.monsterSystem = monsterSystem;
            this.projectileSystem = projectileSystem;
            this.soundData = soundData;

            Instance = this;
        }

        public static TowerSystem Instance { get; private set; }


        public void Tick()
        {
            var monsters = monsterSystem.GetAllMonsters();
            var step = TickManager.Instance.tickInterval;

            foreach (var tower in towersModel.Towers)
            {
                if (tower.CooldownTimer > 0)
                    tower.CooldownTimer -= step;

                if (tower.CooldownTimer > 0)
                    continue;

                var targets = FindTargets(tower, monsters);
                if (targets.Count == 0)
                {
                    tower.ShotsLeft = 0;
                    continue;
                }

                var target = targets[0];
                Shoot(tower, target);

                if (tower.ShotsLeft > 0)
                {
                    tower.ShotsLeft--;
                    tower.CooldownTimer = 0.2f;
                }
                else
                {
                    tower.ShotsLeft = tower.Data.targetsCount - 1;
                    tower.CooldownTimer = 1f / tower.Data.fireRate;
                }
            }
        }

        public event Action OnFirstTowerPlaced;

        public bool CanPlaceTower(TowerData data, Vector3Int cellPos) =>
            !IsCellOccupied(cellPos) && CanAffordTower(data);

        public bool CanAffordTower(TowerData data) => castleSystem.CanAfford(data.baseCost);

        public bool IsCellOccupied(Vector3Int cellPos) => towersModel.Towers.Any(t => t.GridPosition == cellPos);

        public bool TryPlaceTower(TowerData data, Vector3Int cellPos, Vector3 worldPos)
        {
            if (!CanPlaceTower(data, cellPos))
                return false;


            castleSystem.TrySpendGold(data.baseCost);
            var tower = new TowerModel(data, cellPos, worldPos);
            towersModel.AddTower(tower);

            if (soundData != null && soundData.towerPlaceSound != null)
                AudioManager.Instance.PlaySfx(soundData.towerPlaceSound);

            if (!firstTowerPlaced)
            {
                firstTowerPlaced = true;
                OnFirstTowerPlaced?.Invoke();
                Debug.Log("First tower placed!. Game can start.");
            }

            return true;
        }

        private void Shoot(TowerModel tower, MonsterModel target)
        {
            AudioClip[] sound;
            float volume;

            if (tower.Data.type == TowerType.Mage)
            {
                sound = soundData.mageTowerShootSounds;
                volume = soundData.mageShootVolume;
            }
            else
            {
                sound = soundData.archerTowerShootSounds;
                volume = soundData.archerShootVolume;
            }

            if (soundData != null &&
                soundData.archerTowerShootSounds is { Length: > 0 })
                AudioManager.Instance.PlayRandomSfx(sound, volume);

            var firePoint = tower.WorldPosition +
                            new Vector3(tower.Data.projectileData.xOffset, tower.Data.projectileData.yOffset, 0);

            var interceptPoint = CalculateInterceptPoint(
                firePoint,
                target.HitPosition,
                target.CurrentVelocity,
                tower.Data.projectileData.speed
            );

            var projectile = new ProjectileModel(
                tower.WorldPosition,
                target,
                tower.Data.projectileData,
                interceptPoint
            );

            projectileSystem.CreateProjectile(projectile);
        }

        private static Vector3 CalculateInterceptPoint(Vector3 shooterPos, Vector3 targetPos, Vector3 targetVelocity,
            float projectileSpeed)
        {
            var toTarget = targetPos - shooterPos;

            var a = Vector3.Dot(targetVelocity, targetVelocity) - projectileSpeed * projectileSpeed;
            var b = 2f * Vector3.Dot(targetVelocity, toTarget);
            var c = Vector3.Dot(toTarget, toTarget);

            var discriminant = b * b - 4f * a * c;

            if (discriminant < 0f || Mathf.Abs(a) < 0.001f)
                return targetPos;

            var sqrt = Mathf.Sqrt(discriminant);

            var t1 = (-b + sqrt) / (2f * a);
            var t2 = (-b - sqrt) / (2f * a);

            var t = Mathf.Min(t1, t2);

            if (t < 0f)
                t = Mathf.Max(t1, t2);

            if (t < 0f)
                return targetPos;

            return targetPos + targetVelocity * t;
        }

        private static List<MonsterModel> FindTargets(TowerModel tower, IReadOnlyList<MonsterModel> monsters)
        {
            return monsters
                .Where(m => !m.IsDead)
                .Select(m => new { Monster = m, Distance = Vector3.Distance(tower.WorldPosition, m.WorldPosition) })
                .Where(x => x.Distance <= tower.Data.range)
                .OrderBy(x => x.Distance)
                .Take(tower.Data.targetsCount)
                .Select(x => x.Monster)
                .ToList();
        }

        public List<TowerModel> GetTowers() => (List<TowerModel>)towersModel.Towers;

        public void Clear()
        {
            towersModel.Clear();
            firstTowerPlaced = false;
        }
    }
}