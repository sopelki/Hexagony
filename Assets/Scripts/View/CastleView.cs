using System.Collections.Generic;
using Logic.Castle;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace View
{
    public class CastleView : MonoBehaviour
    {
        [Header("Настройки стены замка")]
        // Список координат гексов, которые образуют стену
        [SerializeField] private List<Vector2Int> castleHexes = new();

        // Храним мировые позиции для расчетов урона (чтобы не пересчитывать каждый кадр)
        public List<Vector3> WallWorldPositions { get; private set; } = new();
        public List<Vector2Int> WallHexes => castleHexes;
        public CastleModel Model { get; private set; }
        public Field.Field Field { get; private set; }

        public void Initialize(CastleModel model, Tilemap tilemap, Field.Field field)
        {
            Model = model;
            Field = field;
            WallWorldPositions.Clear();
    
            var sumPosition = Vector3.zero;

            foreach (var hex in castleHexes)
            {
                var hexObj = Field.GetHex(hex); 
                var worldPos = tilemap.GetCellCenterWorld(hexObj.offset); 
                WallWorldPositions.Add(worldPos);
                sumPosition += worldPos; // Суммируем все позиции
            }
    
            // Ставим картинку замка ровно посередине между всеми гексами стены
            if (WallWorldPositions.Count > 0)
            {
                transform.position = sumPosition / WallWorldPositions.Count;
            }
        }

        // Полезный метод: найти ближайшую точку замка к монстру
        public Vector3 GetClosestWallPoint(Vector3 monsterPos)
        {
            var closest = WallWorldPositions[0];
            var minDist = Vector3.Distance(monsterPos, closest);

            foreach (var pos in WallWorldPositions)
            {
                var d = Vector3.Distance(monsterPos, pos);
                if (d < minDist)
                {
                    minDist = d;
                    closest = pos;
                }
            }
            return closest;
        }
        
        private void OnDrawGizmos()
        {
            // Рисует красные сферы в редакторе там, где сейчас гексы стены
            Gizmos.color = Color.red;
            if (WallWorldPositions != null)
            {
                foreach (var pos in WallWorldPositions)
                {
                    Gizmos.DrawSphere(pos, 0.3f);
                }
            }
        }
    }
}