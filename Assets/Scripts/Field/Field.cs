using System.Collections.Generic;
using System.Linq;
using HexagonScripts;
using UnityEngine;

namespace Field
{
    public class Field
    {
        private static readonly Vector2Int[] neighborDirections =
        {
            new(0, +1),
            new(+1, 0),
            new(+1, -1),
            new(0, -1),
            new(-1, 0),
            new(-1, +1)
        };
        public readonly Dictionary<Vector2Int, Hexagon> Hexagons = new();
        private readonly Dictionary<Vector3Int, Hexagon> hexagonsByOffset = new();
        public List<MapObjectData> MapObjects = new();


        public void AddHexagon(int x, int y, HexagonType type)
        {
            var offsetPos = new Vector3Int(x, y, 0);
            var axialPos = HexagonMath.OffsetToAxial(x, y);

            var hex = new Hexagon(axialPos.x, axialPos.y, offsetPos, type);

            Hexagons[axialPos] = hex;
            hexagonsByOffset[offsetPos] = hex;
        }

        public FieldData ExportToSaveData()
        {
            var data = new FieldData
            {
                savedHexes = new List<Hexagon>(Hexagons.Values),
                savedObjects = new List<MapObjectData>(MapObjects)
            };
            Debug.Log($"Exported {data.savedHexes.Count} hexagons, {data.savedObjects.Count} objects.");
            return data;
        }

        public void ImportFromFieldData(FieldData data)
        {
            Hexagons.Clear();
            foreach (var hexagon in data.savedHexes)
            {
                Hexagons.Add(hexagon.coordinates, hexagon);
                hexagonsByOffset.Add(hexagon.offset, hexagon);
            }

            MapObjects = data.savedObjects != null
                ? new List<MapObjectData>(data.savedObjects)
                : new List<MapObjectData>();
        }

        public Hexagon GetHexByOffset(Vector3Int offsetCoords)
        {
            return hexagonsByOffset.GetValueOrDefault(offsetCoords);
        }

        public Hexagon GetHex(Vector2Int axialCoords)
        {
            return Hexagons.GetValueOrDefault(axialCoords);
        }

        public List<Hexagon> GetNeighbours(Hexagon currentHex)
        {
            return neighborDirections
                .Select(direction => currentHex.coordinates + direction)
                .Select(GetHex)
                .Where(neighbor => neighbor != null)
                .ToList();
        }

        public bool IsWalkable(Hexagon hex)
        {
            return hex.type == HexagonType.Path;
        }
    }
}