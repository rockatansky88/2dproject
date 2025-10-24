using System.Collections;

public abstract class CombatAction
{
    protected ICombatant attacker;
    protected ICombatant target;

    public abstract IEnumerator Execute();
    public abstract bool CanExecute();
}