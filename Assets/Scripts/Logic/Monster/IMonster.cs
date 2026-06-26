using UnityEngine;

namespace Logic.Monster
{
    public interface IMonster
    {
        Vector2Int CurrentHex { get; }
        bool IsDead { get; }
        Vector3 WorldPosition { get; }

        void TakeDamage(int damage);
    }
}