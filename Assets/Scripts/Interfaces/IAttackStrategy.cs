namespace Interfaces
{
    public interface IAttackStrategy
    {
        bool IsAttacking { get; }
        void Tick();
    }
}