//using System.Collections.Generic;

//public class RoomEvent
//{
//    private RoomEventDataSO data;
//    private List<IEventEffect> effects;

//    public RoomEvent(RoomEventDataSO eventData)
//    {
//        data = eventData;
//        effects = new List<IEventEffect>();

//        // ScriptableObject 데이터를 실제 Effect 객체로 변환
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
//                return new BuffEffect(/* 데이터로부터 생성 */);
//            case EventEffectType.Reward:
//                return new RewardEffect();
//            case EventEffectType.Damage:
//                return new DamageEffect();
//            // ... 더 많은 타입
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