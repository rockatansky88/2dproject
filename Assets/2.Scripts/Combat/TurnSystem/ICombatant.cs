public interface ICombatant
{
    string Name { get; }
    int Speed { get; }
    bool IsAlive { get; }
    bool IsPlayer { get; }

    void TakeDamage(int damage);
    void Heal(int amount);
    //void ApplyBuff(BuffData buff);
    //void ApplyDebuff(DebuffData debuff);
}