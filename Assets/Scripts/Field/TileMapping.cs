using System;
using HexagonScripts;
using UnityEngine.Tilemaps;

namespace Field
{
    [Serializable]
    public struct TileMapping
    {
        public HexagonType type;
        public TileBase tileAsset;
    }
}