public class DamageEffect : IEventEffect
{
    private int damageAmount;
    private bool percentage;

    public void Apply(Party party)
    {
        //foreach (var character in party.GetAliveMembers())
        //{
        //    int damage = percentage
        //        ? (int)(character.Stats.MaxHP * (damageAmount / 100f))
        //        : damageAmount;
        //    character.TakeDamage(damage);
        //}
    }

    public string GetDescription()
    {
        return $"��Ƽ���� {damageAmount}{(percentage ? "%" : "")} ���ظ� �Ծ���!";
    }
}