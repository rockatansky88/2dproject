//using System.Collections.Generic;

//public class RoomEvent
//{
//    private RoomEventDataSO data;
//    private List<IEventEffect> effects;

//    public RoomEvent(RoomEventDataSO eventData)
//    {
//        data = eventData;
//        effects = new List<IEventEffect>();

//        // ScriptableObject �����͸� ���� Effect ��ü�� ��ȯ
//        foreach (var effectData in eventData.effects)
//        {
//            effects.Add(CreateEffect(effectData));
//        }
//    }

//    private IEventEffect CreateEffect(EventEffect effectData)
//    {
//        switch (effectData.type)
//        {
//            case EventEffectType.Buff:
//                return new BuffEffect(/* �����ͷκ��� ���� */);
//            case EventEffectType.Reward:
//                return new RewardEffect();
//            case EventEffectType.Damage:
//                return new DamageEffect();
//            // ... �� ���� Ÿ��
//            default:
//                return null;
//        }
//    }

//    public void Execute(Party party)
//    {
//        foreach (var effect in effects)
//        {
//            effect.Apply(party);
//        }
//    }
//}