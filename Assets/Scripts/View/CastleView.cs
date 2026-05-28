using System.Collections.Generic;
using Logic.Castle;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace View
{
    public class CastleView : MonoBehaviour
    {
        [Header("Castle wall settings")]
        [SerializeField]
        private List<Vector2Int> castleHexes = new();

        public List<Vector3> WallWorldPositions { get; } = new();
        // public List<Vector2Int> WallHexes => castleHexes;
        public CastleModel Model { get; private set; }
        public Field.Field Field { get; private set; }

        // найти ближайшую точку замка к монстру
        // public Vector3 GetClosestWallPoint(Vector3 monsterPos)
        // {
        //     var closest = WallWorldPositions[0];
        //     var minDist = Vector3.Distance(monsterPos, closest);
        //
        //     foreach (var pos in WallWorldPositions)
        //     {
        //         var d = Vector3.Distance(monsterPos, pos);
        //         if (d < minDist)
        //         {
        //             minDist = d;
        //             closest = pos;
        //         }
        //     }
        //     return closest;
        // }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            if (WallWorldPositions != null)
            {
                foreach (var pos in WallWorldPositions)
                    Gizmos.DrawSphere(pos, 0.3f);
            }
        }

        public void Initialize(CastleModel model, Tilemap tilemap, Field.Field field)
        {
            Model = model;
            Field = field;
            WallWorldPositions.Clear();

            foreach (var logicalHex in castleHexes)
            {
                var hexObj = Field.GetHex(logicalHex);
                if (hexObj != null)
                {
                    var worldPos = tilemap.GetCellCenterWorld(hexObj.offset);
                    worldPos.z = -0.1f;
                    WallWorldPositions.Add(worldPos);
                }
            }

            if (CastleSystem.Instance != null)
                CastleSystem.Instance.RegisterCastleData(WallWorldPositions, castleHexes);
        }
    }
}