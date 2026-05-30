using System.Collections.Generic;
using System.Linq;
using Core;
using Interfaces;
using Logic.Monster;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Logic.Unit
{
    public class UnitAStarMoveStrategy : IMovementStrategy
    {
        private const float RepathDelay = 0.5f;

        private const float MinDistanceBetweenUnits = 2.0f;
        private const float SeparationStrength = 2.5f;
        private readonly Field.Field field;
        private readonly Vector3 formationOffset;
        private readonly MonsterSystem monsterSystem;
        private readonly HexAStarPathfinder pathfinder;
        private readonly Vector2Int spawnHex = new(-12, 9);
        private readonly Tilemap tilemap;
        private readonly UnitModel unit;

        private List<Vector2Int> currentPath;
        private MonsterModel currentTarget;

        private Vector2Int currentTargetHex;
        private bool isPatrolling;
        private int pathIndex;

        private float patrolWaitTimer;

        private float repathTimer;

        public UnitAStarMoveStrategy(
            UnitModel unit,
            MonsterSystem monsterSystem,
            Field.Field field,
            Tilemap tilemap)
        {
            this.unit = unit;
            this.monsterSystem = monsterSystem;
            this.field = field;
            this.tilemap = tilemap;
            pathfinder = new HexAStarPathfinder(field);
            formationOffset = new Vector3(
                Random.Range(-0.15f, 0.15f),
                Random.Range(-0.15f, 0.15f),
                0f);
        }

        public void Tick()
        {
            var dt = TickManager.Instance.tickInterval;
            repathTimer -= dt;
            var monsters = monsterSystem.GetAllMonsters()
                .Where(m => !m.IsDead)
                .ToList();

            if (monsters.Count == 0)
            {
                if (currentTarget != null)
                {
                    currentTarget.TargetedByUnits =
                        Mathf.Max(0, currentTarget.TargetedByUnits - 1);
                    currentTarget = null;
                }

                isPatrolling = true;
                HandleIdlePatrol(dt);
                ApplySeparation(dt);
                return;
            }

            if (isPatrolling)
            {
                isPatrolling = false;
                currentPath = null;
            }

            const float crowdPenalty = 2f;

            var target = monsters
                .OrderBy(m =>
                    Vector3.Distance(m.WorldPosition, unit.WorldPosition) +
                    m.TargetedByUnits * crowdPenalty)
                .First();

            if (Vector3.Distance(target.WorldPosition, unit.WorldPosition)
                <= unit.UnitData.attackRadius)
            {
                currentPath = null;
                ApplySeparation(dt);
                return;
            }
            if (repathTimer <= 0f &&
                (currentTarget != target ||
                 Vector2Int.Distance(currentTargetHex, target.CurrentHex) > 1))
            {
                if (currentTarget != null && currentTarget != target)
                {
                    currentTarget.TargetedByUnits =
                        Mathf.Max(0, currentTarget.TargetedByUnits - 1);
                }
                if (currentTarget != target)
                {
                    currentTarget = target;
                    currentTarget.TargetedByUnits++;
                }

                BuildNewPath(target);
                repathTimer = RepathDelay;
            }
            if (currentPath != null && pathIndex < currentPath.Count)
                MoveAlongPath();
            else
                currentPath = null;

            ApplySeparation(dt);
        }

        private void BuildNewPath(MonsterModel target)
        {
            currentTargetHex = GetRandomizedGoal(target.CurrentHex);

            currentPath = pathfinder.FindPath(unit.CurrentHex, currentTargetHex);
            pathIndex = 1;

            if (currentPath == null || currentPath.Count <= 1)
                currentPath = null;
        }

        private void MoveAlongPath()
        {
            var nextHex = currentPath[pathIndex];
            var hexObj = field.GetHex(nextHex);

            if (hexObj == null)
                return;

            var targetWorld = tilemap.GetCellCenterWorld(hexObj.offset) + formationOffset;
            var currentPosition = unit.WorldPosition;

            var step = unit.GetMoveSpeed() * TickManager.Instance.tickInterval;
            var distance = Vector3.Distance(currentPosition, targetWorld);

            if (distance <= step)
            {
                unit.SetPosition(targetWorld);
                unit.SetHex(nextHex);
                pathIndex++;
            }
            else
            {
                var direction = (targetWorld - currentPosition).normalized;
                unit.SetPosition(currentPosition + direction * step);
                unit.CurrentDirection = direction;
            }
        }

        private Vector2Int GetRandomizedGoal(Vector2Int center)
        {
            var centerHex = field.GetHex(center);
            if (centerHex == null)
                return center;

            var neighbours = field.GetNeighbours(centerHex)
                .Where(h => field.IsWalkable(h))
                .ToList();

            if (neighbours.Count == 0)
                return center;

            if (Random.value < 0.5f)
                return center;

            return neighbours[Random.Range(0, neighbours.Count)].coordinates;
        }

        private void HandleIdlePatrol(float dt)
        {
            if (currentPath != null && pathIndex < currentPath.Count)
            {
                MoveAlongPath();
                return;
            }

            var distanceToRallyPoint = Vector2Int.Distance(unit.CurrentHex, spawnHex);
            if (distanceToRallyPoint > 10f)
            {
                currentPath = null;
                unit.CurrentDirection = Vector3.zero;
                return;
            }

            patrolWaitTimer -= dt;
            if (patrolWaitTimer > 0f)
                return;

            var radius = 2;
            var randomX = Random.Range(-radius, radius + 1);
            var randomY = Random.Range(-radius, radius + 1);
            var potentialHex = spawnHex + new Vector2Int(randomX, randomY);

            var hexObj = field.GetHex(potentialHex);

            if (hexObj != null && field.IsWalkable(hexObj))
            {
                currentPath = pathfinder.FindPath(unit.CurrentHex, potentialHex);
                pathIndex = 1;

                if (currentPath == null || currentPath.Count <= 1)
                    currentPath = null;

                patrolWaitTimer = Random.Range(2f, 5f);
            }
        }

        private void ApplySeparation(float dt)
        {
            var separationForce = Vector3.zero;
            var allUnits = UnitSystem.Instance.GetAllUnits();
            var overlapCount = 0;

            foreach (var otherUnit in allUnits)
            {
                if (otherUnit == unit || otherUnit.IsDead)
                    continue;

                var distance = Vector3.Distance(unit.WorldPosition, otherUnit.WorldPosition);

                if (distance < MinDistanceBetweenUnits)
                {
                    overlapCount++;
                    var pushDirection = unit.WorldPosition - otherUnit.WorldPosition;

                    if (pushDirection.sqrMagnitude < 0.001f)
                        pushDirection = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0f);

                    pushDirection.Normalize();
                    separationForce += pushDirection * (1f - distance / MinDistanceBetweenUnits);
                }
            }

            if (overlapCount > 0)
            {
                var newPosition = unit.WorldPosition + separationForce * SeparationStrength * dt;
                unit.SetPosition(newPosition);
                var cellPos = tilemap.WorldToCell(newPosition);
                var hexAtPos = field.GetHexByOffset(cellPos);

                if (hexAtPos != null)
                    unit.SetHex(hexAtPos.coordinates);
            }
        }
    }
}