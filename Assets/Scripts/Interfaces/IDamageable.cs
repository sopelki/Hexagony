namespace Interfaces
{
    public interface IDamageable
    {
        bool IsDead { get; }

        void TakeDamage(int damage);
    }
}