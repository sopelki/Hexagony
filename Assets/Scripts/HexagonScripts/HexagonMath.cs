using UnityEngine;

namespace HexagonScripts
{
    public class HexagonMath
    {
        private const float HexSize = 1f;

        public static Vector2Int OffsetToAxial(int x, int y)
        {
            var q = x - (y - (y & 1)) / 2;

            return new Vector2Int(q, y);
        }

        public static Vector2Int WorldToAxial(Vector3 worldPos)
        {
            var q = (Mathf.Sqrt(3f) / 3f * worldPos.x -
                     1f / 3f * worldPos.y) / HexSize;

            var r = 2f / 3f * worldPos.y / HexSize;

            return HexRound(q, r);
        }

        public static Vector3 OffsetToWorld(int x, int y)
        {
            var axial = OffsetToAxial(x, y);
            return AxialToWorld(axial.x, axial.y);
        }

        public static Vector3 AxialToWorld(int q, int r)
        {
            var x = HexSize * Mathf.Sqrt(3f) * (q + r * 0.5f);
            var y = HexSize * 1.5f * r;

            return new Vector3(x, y, 0f);
        }

        private static Vector2Int HexRound(float q, float r)
        {
            var x = q;
            var z = r;
            var y = -x - z;

            var rx = Mathf.RoundToInt(x);
            var ry = Mathf.RoundToInt(y);
            var rz = Mathf.RoundToInt(z);

            var xDiff = Mathf.Abs(rx - x);
            var yDiff = Mathf.Abs(ry - y);
            var zDiff = Mathf.Abs(rz - z);

            if (xDiff > yDiff && xDiff > zDiff)
                rx = -ry - rz;
            else if (yDiff > zDiff)
                // ReSharper disable once RedundantAssignment
                ry = -rx - rz;
            else
                rz = -rx - ry;

            return new Vector2Int(rx, rz);
        }

        public static int Distance(Vector2Int a, Vector2Int b)
        {
            var dq = a.x - b.x;
            var dr = a.y - b.y;

            return (Mathf.Abs(dq)
                    + Mathf.Abs(dq + dr)
                    + Mathf.Abs(dr)) / 2;
        }
    }
}