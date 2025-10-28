//public class BuffEffect : IEventEffect
//{
//    private BuffData buffData;
//    private bool applyToAll;

//    public BuffEffect(BuffData buff, bool all = true)
//    {
//        buffData = buff;
//        applyToAll = all;
//    }

//    public void Apply(Party party)
//    {
//        if (applyToAll)
//        {
//            foreach (var character in party.GetAliveMembers())
//            {
//                character.ApplyBuff(buffData);
//            }
//        }
//        else
//        {
//            party.GetRandomMember().ApplyBuff(buffData);
//        }
//    }

//    public string GetDescription()
//    {
//        return $"파티원에게 {buffData.statType} +{buffData.value} 버프!";
//    }
//}
