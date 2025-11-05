using UnityEngine;

/// <summary>
/// 던전 이벤트로 인한 임시 버프/디버프 데이터
/// 던전이 종료될 때까지 용병에게 적용되는 스탯 변화를 저장합니다.
/// </summary>
[System.Serializable]
public class EventBuffData
{
    public string buffID;              // 버프 고유 ID
    public string buffName;            // 버프 이름 (예: "고대의 축복")
    public EventEffectType buffType;   // Buff 또는 Debuff

    // 스탯 변화량
    public int strengthModifier;
    public int dexterityModifier;
    public int intelligenceModifier;
    public int wisdomModifier;
    public int speedModifier;

    public int duration;               // 지속 시간 (0 = 던전 종료까지)
    public int remainingDuration;      // 남은 지속 시간

    /// <summary>
    /// 버프 생성자
    /// </summary>
    public EventBuffData(string id, string name, EventEffectType type, int str, int dex, int intel, int wis, int spd, int dur)
    {
        buffID = id;
        buffName = name;
        buffType = type;
        strengthModifier = str;
        dexterityModifier = dex;
        intelligenceModifier = intel;
        wisdomModifier = wis;
        speedModifier = spd;
        duration = dur;
        remainingDuration = dur;
    }

    /// <summary>
    /// 버프가 활성 상태인지 확인
    /// </summary>
    public bool IsActive()
    {
        return duration == 0 || remainingDuration > 0;
    }

    /// <summary>
    /// 턴 종료 시 duration 감소
    /// </summary>
    public void DecreaseDuration()
    {
        if (duration > 0 && remainingDuration > 0)
        {
            remainingDuration--;
        }
    }
}
