using UnityEngine;
using Logic.Monster;

namespace Logic.Projectile
{
    public class ProjectileModel
    {
        public Vector3 Position;
        public Vector3 Direction;
        public Vector3 TargetPoint;
        public Vector3 StartPosition;
        public float TravelProgress;

        public float TowerBaseY { get; private set; }

        public MonsterModel Target;
        public ProjectileData Data;

        public ProjectileModel(
            Vector3 startPos,
            MonsterModel target,
            ProjectileData data,
            Vector3 interceptPoint)
        {
            var spawnPoint = startPos + new Vector3(data.xOffset, data.yOffset, 0);

            StartPosition = spawnPoint;
            Position = spawnPoint;
            Target = target;
            Data = data;

            TowerBaseY = spawnPoint.y;

            TargetPoint = interceptPoint;
            Direction = (TargetPoint - spawnPoint).normalized;

            TravelProgress = 0f;
        }
    }
}