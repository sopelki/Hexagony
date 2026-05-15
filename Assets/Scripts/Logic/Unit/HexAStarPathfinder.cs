// using System.Collections.Generic;
// using System.Linq;
// using HexagonScripts;
// using UnityEngine;
//
// namespace Logic.Unit
// {
//     public class HexAStarPathfinder
//     {
//         private readonly Field.Field field;
//
//         public HexAStarPathfinder(Field.Field field)
//         {
//             this.field = field;
//         }
//
//         public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
//         {
//             if (start == goal)
//                 return new List<Vector2Int> { start };
//
//             var openSet = new List<Vector2Int> { start };
//             var closedSet = new HashSet<Vector2Int>();
//
//             var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
//
//             var gScore = new Dictionary<Vector2Int, int>
//             {
//                 [start] = 0
//             };
//
//             var fScore = new Dictionary<Vector2Int, int>
//             {
//                 [start] = HexagonMath.Distance(start, goal)
//             };
//
//             while (openSet.Count > 0)
//             {
//                 var current = openSet
//                     .OrderBy(h => fScore.GetValueOrDefault(h, int.MaxValue))
//                     .First();
//
//                 if (current == goal)
//                     return ReconstructPath(cameFrom, current);
//
//                 openSet.Remove(current);
//                 closedSet.Add(current);
//
//                 var currentHex = field.GetHex(current);
//                 if (currentHex == null)
//                     continue;
//
//                 foreach (var neighbor in field.GetNeighbours(currentHex))
//                 {
//                     if (!field.IsWalkable(neighbor))
//                         continue;
//
//                     var nCoord = neighbor.coordinates;
//
//                     if (closedSet.Contains(nCoord))
//                         continue;
//
//                     var randomCost = Random.Range(0, 3); // маленький шум
//                     var tentativeG = gScore[current] + 1 + randomCost;
//
//                     if (!openSet.Contains(nCoord))
//                         openSet.Add(nCoord);
//                     else if (tentativeG >= gScore.GetValueOrDefault(nCoord, int.MaxValue))
//                         continue;
//
//                     cameFrom[nCoord] = current;
//                     gScore[nCoord] = tentativeG;
//                     fScore[nCoord] = tentativeG + HexagonMath.Distance(nCoord, goal);
//                 }
//             }
//
//             return null; // пути нет
//         }
//
//         private List<Vector2Int> ReconstructPath(
//             Dictionary<Vector2Int, Vector2Int> cameFrom,
//             Vector2Int current)
//         {
//             var path = new List<Vector2Int> { current };
//
//             while (cameFrom.ContainsKey(current))
//             {
//                 current = cameFrom[current];
//                 path.Add(current);
//             }
//
//             path.Reverse();
//             return path;
//         }
//     }
// }

using System.Collections.Generic;
using HexagonScripts;
using UnityEngine;

namespace Logic.Unit
{
    public class HexAStarPathfinder
    {
        private readonly Field.Field field;
        private readonly System.Random randomGenerator;

        // --- ИЗМЕНЕНИЯ ДЛЯ ВАРИАТИВНОСТИ ---
        // Базовая стоимость перехода на соседнюю клетку
        private const int BASE_MOVE_COST = 10;
        // Величина случайного "шума". Путь может стать "дороже" на это значение.
        private const int RANDOM_COST_RANGE = 5;

        public HexAStarPathfinder(Field.Field field)
        {
            this.field = field;
            // Используем один экземпляр System.Random для настоящей случайности
            this.randomGenerator = new System.Random();
        }

        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
        {
            var startHex = field.GetHex(start);
            var goalHex = field.GetHex(goal);
            
            if (start == goal || !field.IsWalkable(startHex) || !field.IsWalkable(goalHex))
                return null;
                
            // Используем PriorityQueue для огромного прироста скорости
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
                    
                    // --- СБАЛАНСИРОВАННАЯ СТОИМОСТЬ ---
                    // Стоимость = База + Случайный шум
                    var moveCost = BASE_MOVE_COST + randomGenerator.Next(0, RANDOM_COST_RANGE + 1);
                    var tentativeGScore = gScore[current] + moveCost;

                    if (tentativeGScore < gScore.GetValueOrDefault(nCoord, int.MaxValue))
                    {
                        cameFrom[nCoord] = current;
                        gScore[nCoord] = tentativeGScore;
                        
                        // Эвристику тоже умножаем на базу, чтобы она была сопоставима с G-score
                        var heuristic = HexagonMath.Distance(nCoord, goal) * BASE_MOVE_COST;
                        float fScore = tentativeGScore + heuristic;
                        
                        openSet.Enqueue(nCoord, fScore);
                    }
                }
            }

            return null; // Путь не найден
        }

        private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
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