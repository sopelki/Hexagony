using System;
using UnityEngine;

namespace HexagonScripts
{
    [Serializable]
    public class Hexagon
    {
        public Vector2Int coordinates;
        public Vector3Int offset;
        public HexagonType type;

        public Hexagon(int x, int y, Vector3Int offset, HexagonType type)
        {
            coordinates = new Vector2Int(x, y);
            this.offset = offset;
            this.type = type;
        }
    }
}