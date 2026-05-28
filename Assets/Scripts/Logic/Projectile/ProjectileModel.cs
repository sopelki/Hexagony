using Logic.Monster;
using UnityEngine;

namespace Logic.Projectile
{
    public class ProjectileModel
    {
        public ProjectileData Data;
        public Vector3 Direction;
        public Vector3 Position;
        public Vector3 StartPosition;

        public MonsterModel Target;
        public Vector3 TargetPoint;
        public float TravelProgress;

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

        public float TowerBaseY { get; private set; }
    }
}