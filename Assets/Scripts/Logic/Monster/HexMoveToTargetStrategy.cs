using System.Collections.Generic;
using System.Linq;
using Core;
using Interfaces;
using Logic.Castle;
using Logic.Trap;
using Logic.Unit;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Logic.Monster
{
    public class HexMoveToTargetStrategy : IMovementStrategy
    {
        public HexMoveToTargetStrategy(
            MonsterModel monster,
            Field.Field field,
            Tilemap tilemap,
            TrapSystem trapSystem,
            MonsterSystem monsterSystem)
        {
            this.monster = monster;
            this.field = field;
            this.tilemap = tilemap;
            this.trapSystem = trapSystem;
            this.monsterSystem = monsterSystem;

            pathfinder = new HexAStarPathfinder(field);

            formationOffset = new Vector3(
                Random.Range(-0.2f, 0.2f),
                Random.Range(-0.2f, 0.2f),
                0f);
        }

        private List<Vector2Int> currentPath;

        private readonly Field.Field field;

        private readonly Vector3 formationOffset;
        private const float MinDistanceBetweenMonsters = 2.0f;
        private readonly MonsterModel monster;
        private readonly MonsterSystem monsterSystem;
        private readonly HexAStarPathfinder pathfinder;
        private int pathIndex;
        private const float RepathDelay = 0.7f;

        private float repathTimer;
        private const float SeparationStrength = 2.5f;
        private readonly Tilemap tilemap;
        private readonly TrapSystem trapSystem;

        public void Tick()
        {
            var castle = CastleSystem.Instance;
            var dt = TickManager.Instance.tickInterval;
            if (castle == null || castle.CastleModel.WallHexes.Count == 0)
                return;

            if (castle.CastleModel.WallHexes.Contains(monster.CurrentHex) || IsAdjacentToWall(monster.CurrentHex))
            {
                currentPath = null;
                ApplySeparation(dt);
                return;
            }

            repathTimer -= TickManager.Instance.tickInterval;

            if (currentPath == null || pathIndex >= currentPath.Count || repathTimer <= 0f)
            {
                BuildPath();
                repathTimer = RepathDelay;
            }

            if (currentPath != null && pathIndex < currentPath.Count)
                MoveAlongPath();

            ApplySeparation(dt);
        }

        private void ApplySeparation(float dt)
        {
            var separationForce = Vector3.zero;
            var allMonsters = monsterSystem.GetAllMonsters();
            var overlapCount = 0;

            foreach (var otherMonster in allMonsters)
            {
                if (otherMonster == monster || otherMonster.IsDead)
                    continue;

                var distance = Vector3.Distance(monster.WorldPosition, otherMonster.WorldPosition);

                if (distance < MinDistanceBetweenMonsters)
                {
                    overlapCount++;
                    var pushDirection = monster.WorldPosition - otherMonster.WorldPosition;

                    if (pushDirection.sqrMagnitude < 0.001f)
                        pushDirection = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0f);

                    pushDirection.Normalize();
                    separationForce += pushDirection * (1f - distance / MinDistanceBetweenMonsters);
                }
            }

            if (overlapCount > 0)
            {
                var newPosition = monster.WorldPosition + separationForce * SeparationStrength * dt;
                var cellPos = tilemap.WorldToCell(newPosition);
                var hexAtPos = field.GetHexByOffset(cellPos);

                if (hexAtPos != null && field.IsWalkable(hexAtPos))
                {
                    monster.SetPosition(newPosition);

                    var previousHex = monster.CurrentHex;
                    if (previousHex != hexAtPos.coordinates)
                    {
                        monster.SetHex(hexAtPos.coordinates);
                        trapSystem.OnMonsterExitedCell(previousHex, monster);
                        trapSystem.OnMonsterEnteredCell(hexAtPos.coordinates, monster);
                    }
                }
                else
                {
                    var currentCell = tilemap.WorldToCell(monster.WorldPosition);
                    if (cellPos == currentCell)
                        monster.SetPosition(newPosition);
                }
            }
        }

        private void BuildPath()
        {
            var castle = CastleSystem.Instance;

            if (IsAdjacentToWall(monster.CurrentHex))
            {
                currentPath = null;
                return;
            }

            var goal = GetBestSiegePosition();

            if (goal == monster.CurrentHex)
            {
                currentPath = null;
                return;
            }

            currentPath = pathfinder.FindPath(monster.CurrentHex, goal.Value);
            pathIndex = 1;

            if (currentPath is not { Count: > 1 })
                currentPath = null;
        }

        private Vector2Int? GetBestSiegePosition()
        {
            var castle = CastleSystem.Instance;
            if (castle == null || castle.CastleModel.WallHexes.Count == 0) return null;

            var validSiegePositions = new HashSet<Vector2Int>();

            foreach (var wallCoord in castle.CastleModel.WallHexes)
            {
                var wallHex = field.GetHex(wallCoord);
                if (wallHex == null) continue;

                var walkableNeighbours = field.GetNeighbours(wallHex)
                    .Where(n => field.IsWalkable(n))
                    .Select(n => n.coordinates);

                foreach (var pos in walkableNeighbours)
                    validSiegePositions.Add(pos);
            }

            if (validSiegePositions.Count == 0)
                return null;

            var topPositions = validSiegePositions
                .OrderBy(p => Vector2Int.Distance(monster.CurrentHex, p))
                .Take(4)
                .ToList();
            return topPositions[Random.Range(0, topPositions.Count)];
        }

        private bool IsAdjacentToWall(Vector2Int hexCoord)
        {
            var hex = field.GetHex(hexCoord);
            if (hex == null) return false;

            var castle = CastleSystem.Instance;
            if (castle == null) return false;

            var neighbours = field.GetNeighbours(hex);
            return neighbours.Any(n => castle.CastleModel.WallHexes.Contains(n.coordinates));
        }

        private void MoveAlongPath()
        {
            var nextHex = currentPath[pathIndex];
            var hexObj = field.GetHex(nextHex);

            if (hexObj == null)
                return;

            var targetWorld = tilemap.GetCellCenterWorld(hexObj.offset) + formationOffset;
            var directionVector = targetWorld - monster.WorldPosition;
            var maxStep = monster.MoveSpeed * TickManager.Instance.tickInterval;

            if (directionVector.magnitude <= maxStep)
            {
                monster.Move(directionVector / maxStep);
                var previousHex = monster.CurrentHex;

                monster.SetHex(nextHex);

                trapSystem.OnMonsterExitedCell(previousHex, monster);
                trapSystem.OnMonsterEnteredCell(nextHex, monster);

                pathIndex++;
            }
            else
                monster.Move(directionVector.normalized);
        }
    }
}