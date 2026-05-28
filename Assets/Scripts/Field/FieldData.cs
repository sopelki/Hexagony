using System;
using System.Collections.Generic;
using HexagonScripts;

namespace Field
{
    [Serializable]
    public class FieldData
    {
        public List<Hexagon> savedHexes = new();
        public List<MapObjectData> savedObjects = new();
    }
}