using UnityEngine;
using Interfaces;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using Logic.Unit;

namespace Logic.Monster
{
    public class HexMoveToTargetStrategy : IMovementStrategy
    {
        private readonly MonsterModel monster;
        private readonly Field.Field field;
        private readonly List<Vector2Int> targetHexes;
        private readonly Tilemap tilemap;
        private readonly HexAStarPathfinder pathfinder;

        private List<Vector2Int> currentPath;
        private int pathIndex;

        private float repathTimer;
        private const float RepathDelay = 0.7f;

        private readonly Vector3 formationOffset;

        public HexMoveToTargetStrategy(
            MonsterModel monster,
            Field.Field field,
            List<Vector2Int> targetHexes,
            Tilemap tilemap)
        {
            this.monster = monster;
            this.field = field;
            this.targetHexes = targetHexes;
            this.tilemap = tilemap;

            pathfinder = new HexAStarPathfinder(field);

            formationOffset = new Vector3(
                Random.Range(-0.2f, 0.2f),
                Random.Range(-0.2f, 0.2f),
                0f);
        }

        public void Tick()
        {
            if (targetHexes.Contains(monster.CurrentHex))
                return;

            repathTimer -= Core.TickManager.Instance.tickInterval;

            if (currentPath == null || pathIndex >= currentPath.Count || repathTimer <= 0f)
            {
                BuildPath();
                repathTimer = RepathDelay;
            }

            if (currentPath == null || pathIndex >= currentPath.Count)
                return;

            MoveAlongPath();
        }

        private void BuildPath()
        {
            var closestWallHex = GetClosestWallHex();
            var goal = GetRandomizedGoal(closestWallHex);

            currentPath = pathfinder.FindPath(monster.CurrentHex, goal);
            pathIndex = 1;

            if (currentPath is not { Count: > 1 })
                currentPath = null;
        }
        
        private Vector2Int GetClosestWallHex()
        {
            if (targetHexes == null || targetHexes.Count == 0) return monster.CurrentHex;

            var closest = targetHexes[0];
            var minDist = Vector2Int.Distance(monster.CurrentHex, closest);

            foreach (var hex in targetHexes)
            {
                var d = Vector2Int.Distance(monster.CurrentHex, hex);
                if (d < minDist)
                {
                    minDist = d;
                    closest = hex;
                }
            }
            return closest;
        }

        private void MoveAlongPath()
        {
            var nextHex = currentPath[pathIndex];
            var hexObj = field.GetHex(nextHex);

            if (hexObj == null)
                return;

            var targetWorld = tilemap.GetCellCenterWorld(hexObj.offset) + formationOffset;
            var directionVector = targetWorld - monster.WorldPosition;
            var maxStep = monster.Data.moveSpeed * Core.TickManager.Instance.tickInterval;

            if (directionVector.magnitude <= maxStep)
            {
                monster.Move(directionVector /  maxStep);
                monster.SetHex(nextHex);
                pathIndex++;
            }
            else
                monster.Move(directionVector.normalized);
        }

        private Vector2Int GetRandomizedGoal(Vector2Int center)
        {
            var centerHex = field.GetHex(center);
            if (centerHex == null)
                return center;

            var neighbours = field.GetNeighbours(centerHex);

            var walkable = (from n in neighbours where field.IsWalkable(n) select n.coordinates).ToList();

            return walkable.Count == 0 ? center : walkable[Random.Range(0, walkable.Count)];
        }
    }
}