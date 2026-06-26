using System.Collections.Generic;
using HexagonScripts;
using Logic.Castle;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace View
{
    public class CastleView : MonoBehaviour
    {
        private readonly List<Vector2Int> castleHexes = new()
        {
            new Vector2Int(-27, 22),
            new Vector2Int(-27, 21),
            new Vector2Int(-27, 20),
            new Vector2Int(-27, 19),
            new Vector2Int(-26, 21),
            new Vector2Int(-28, 20),
            new Vector2Int(-28, 19),
            new Vector2Int(-28, 21),
            new Vector2Int(-28, 22),
            new Vector2Int(-27, 23),
            new Vector2Int(-28, 23),
            new Vector2Int(-29, 22),
            new Vector2Int(-29, 21),
            new Vector2Int(-29, 20)
        };

        public List<Vector3> WallWorldPositions { get; } = new();
        public CastleModel Model { get; private set; }
        public Field.Field Field { get; private set; }

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
                    hexObj.type = HexagonType.Castle;
                    var worldPos = tilemap.GetCellCenterWorld(hexObj.offset);
                    worldPos.z = -0.1f;
                    WallWorldPositions.Add(worldPos);
                }
            }

            if (CastleSystem.Instance != null)
                CastleSystem.Instance.RegisterCastleData(WallWorldPositions, castleHexes);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            if (WallWorldPositions != null)
            {
                foreach (var pos in WallWorldPositions) Gizmos.DrawSphere(pos, 0.3f);
            }
        }
    }
}