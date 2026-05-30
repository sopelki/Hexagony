using System.Collections.Generic;
using HexagonScripts;
using UnityEngine;
using Random = System.Random;

namespace Logic.Unit
{
    public class HexAStarPathfinder
    {
        private const int BaseMoveCost = 10;
        private const int RandomCostRange = 5;
        private readonly Field.Field field;
        private readonly Random randomGenerator;

        public HexAStarPathfinder(Field.Field field)
        {
            this.field = field;
            randomGenerator = new Random();
        }

        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
        {
            var startHex = field.GetHex(start);
            var goalHex = field.GetHex(goal);

            if (start == goal || !field.IsWalkable(startHex) || !field.IsWalkable(goalHex))
                return null;

            var openSet = new PriorityQueue<Vector2Int>();
            openSet.Enqueue(start, 0);

            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var gScore = new Dictionary<Vector2Int, int> { [start] = 0 };

            while (openSet.Count > 0)
            {
                var current = openSet.Dequeue();

                if (current == goal)
                    return ReconstructPath(cameFrom, current);

                var currentHex = field.GetHex(current);
                if (currentHex == null) continue;

                foreach (var neighbor in field.GetNeighbours(currentHex))
                {
                    if (!field.IsWalkable(neighbor)) continue;

                    var nCoord = neighbor.coordinates;

                    var moveCost = BaseMoveCost + randomGenerator.Next(0, RandomCostRange + 1);
                    var tentativeGScore = gScore[current] + moveCost;

                    if (tentativeGScore < gScore.GetValueOrDefault(nCoord, int.MaxValue))
                    {
                        cameFrom[nCoord] = current;
                        gScore[nCoord] = tentativeGScore;

                        var heuristic = HexagonMath.Distance(nCoord, goal) * BaseMoveCost;
                        float fScore = tentativeGScore + heuristic;

                        openSet.Enqueue(nCoord, fScore);
                    }
                }
            }

            return null;
        }

        private static List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
        {
            var totalPath = new List<Vector2Int> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                totalPath.Add(current);
            }
            totalPath.Reverse();
            return totalPath;
        }
    }
}