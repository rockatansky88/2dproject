using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 사운드 매니저 - 모든 사운드 재생 관리
/// - BGM과 효과음(SFX)을 분리하여 볼륨 조절 가능
/// - 사운드 그룹별로 관리 (Town, Dungeon, Combat 등)
/// - Inspector에서 사운드 할당만 하면 자동 재생
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;      // BGM 전용
    [SerializeField] private AudioSource sfxSource;      // 효과음 전용

    [Header("Volume Settings")]
    [SerializeField][Range(0f, 1f)] private float bgmVolume = 0.5f;
    [SerializeField][Range(0f, 1f)] private float sfxVolume = 0.7f;

    [Header("Sound Data Groups")]
    [SerializeField] private TownSoundData townSounds;
    [SerializeField] private DungeonSoundData dungeonSounds;
    [SerializeField] private CombatSoundData combatSounds;
    [SerializeField] private MinigameSoundData minigameSounds;
    [SerializeField] private UISoundData uiSounds;

    // 현재 재생 중인 BGM 추적
    private AudioClip currentBGM;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // AudioSource 자동 생성 (없으면)
        if (bgmSource == null)
        {
            GameObject bgmObj = new GameObject("BGM_AudioSource");
            bgmObj.transform.SetParent(transform);
            bgmSource = bgmObj.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFX_AudioSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        // 초기 볼륨 설정
        bgmSource.volume = bgmVolume;
        sfxSource.volume = sfxVolume;
    }


    /// <summary>
    /// BGM 재생 (중복 재생 방지)
    /// </summary>
    public void PlayBGM(AudioClip clip, bool forceReplay = false)
    {
        if (clip == null) return;

        // 같은 BGM이 재생 중이면 무시 (forceReplay가 false일 때)
        if (!forceReplay && currentBGM == clip && bgmSource.isPlaying)
        {
            return;
        }

        currentBGM = clip;
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    /// <summary>
    /// BGM 정지
    /// </summary>
    public void StopBGM()
    {
        bgmSource.Stop();
        currentBGM = null;
    }

    /// <summary>
    /// BGM 페이드 아웃 (부드러운 정지)
    /// </summary>
    public void FadeOutBGM(float duration = 1f)
    {
        StartCoroutine(FadeOutCoroutine(duration));
    }

    private System.Collections.IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.volume = bgmVolume; // 원래 볼륨으로 복구
        currentBGM = null;
    }


    /// <summary>
    /// 효과음 재생 (한 번만 재생)
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    /// <summary>
    /// 효과음 재생 (볼륨 조절)
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, volumeScale);
    }


    /// <summary>
    /// 마을 BGM 재생
    /// </summary>
    public void PlayTownBGM()
    {
        if (townSounds != null)
        {
            PlayBGM(townSounds.townBGM);
        }
    }

    /// <summary>
    /// 던전 입구 BGM 재생
    /// </summary>
    public void PlayDungeonEntranceBGM()
    {
        if (dungeonSounds != null)
        {
            PlayBGM(dungeonSounds.entranceBGM);
        }
    }

    /// <summary>
    /// 던전 통로 BGM 재생
    /// </summary>
    public void PlayDungeonCorridorBGM()
    {
        if (dungeonSounds != null)
        {
            PlayBGM(dungeonSounds.corridorBGM);
        }
    }

    /// <summary>
    /// 이벤트 BGM 재생
    /// </summary>
    public void PlayEventBGM()
    {
        if (dungeonSounds != null)
        {
            PlayBGM(dungeonSounds.eventBGM);
        }
    }

    /// <summary>
    /// 전투 BGM 재생 (일반 / 보스)
    /// </summary>
    public void PlayCombatBGM(bool isBoss = false)
    {
        if (combatSounds != null)
        {
            AudioClip clip = isBoss ? combatSounds.bossBGM : combatSounds.normalCombatBGM;
            PlayBGM(clip);
        }
    }

    /// <summary>
    /// 스킬 사용 효과음 (스킬 데이터에 있는 사운드 재생)
    /// </summary>
    public void PlaySkillSound(AudioClip skillSound)
    {
        PlaySFX(skillSound);
    }

    /// <summary>
    /// 기본 공격 효과음
    /// </summary>
    public void PlayBasicAttackSound()
    {
        if (combatSounds != null)
        {
            PlaySFX(combatSounds.basicAttackSound);
        }
    }

    /// <summary>
    /// 피격 효과음
    /// </summary>
    public void PlayHitSound()
    {
        if (combatSounds != null)
        {
            PlaySFX(combatSounds.hitSound);
        }
    }

    /// <summary>
    /// 크리티컬 효과음
    /// </summary>
    public void PlayCriticalSound()
    {
        if (combatSounds != null)
        {
            PlaySFX(combatSounds.criticalSound);
        }
    }

    /// <summary>
    /// 몬스터 사망 효과음
    /// </summary>
    public void PlayMonsterDeathSound()
    {
        if (combatSounds != null)
        {
            PlaySFX(combatSounds.monsterDeathSound);
        }
    }

    /// <summary>
    /// 용병 사망 효과음
    /// </summary>
    public void PlayCharacterDeathSound()
    {
        if (combatSounds != null)
        {
            PlaySFX(combatSounds.characterDeathSound);
        }
    }

    /// <summary>
    /// TPE 성공 효과음
    /// </summary>
    public void PlayTPESuccessSound()
    {
        if (minigameSounds != null)
        {
            PlaySFX(minigameSounds.tpeSuccessSound);
        }
    }

    /// <summary>
    /// TPE 실패 효과음
    /// </summary>
    public void PlayTPEFailSound()
    {
        if (minigameSounds != null)
        {
            PlaySFX(minigameSounds.tpeFailSound);
        }
    }

    /// <summary>
    /// 패링 성공 효과음
    /// </summary>
    public void PlayParrySuccessSound()
    {
        if (minigameSounds != null)
        {
            PlaySFX(minigameSounds.parrySuccessSound);
        }
    }

    /// <summary>
    /// 패링 실패 효과음
    /// </summary>
    public void PlayParryFailSound()
    {
        if (minigameSounds != null)
        {
            PlaySFX(minigameSounds.parryFailSound);
        }
    }


    /// <summary>
    /// 버튼 클릭 효과음
    /// </summary>
    public void PlayButtonClickSound()
    {
        if (uiSounds != null)
        {
            PlaySFX(uiSounds.buttonClickSound);
        }
    }

    /// <summary>
    /// 아이템 획득 효과음
    /// </summary>
    public void PlayItemGetSound()
    {
        if (uiSounds != null)
        {
            PlaySFX(uiSounds.itemGetSound);
        }
    }

    /// <summary>
    /// 골드 획득 효과음
    /// </summary>
    public void PlayGoldGetSound()
    {
        if (uiSounds != null)
        {
            PlaySFX(uiSounds.goldGetSound);
        }
    }

    /// <summary>
    /// 상점 구매 효과음
    /// </summary>
    public void PlayShopBuySound()
    {
        if (townSounds != null)
        {
            PlaySFX(townSounds.shopBuySound);
        }
    }

    /// <summary>
    /// 상점 판매 효과음
    /// </summary>
    public void PlayShopSellSound()
    {
        if (townSounds != null)
        {
            PlaySFX(townSounds.shopSellSound);
        }
    }

    /// <summary>
    /// BGM 볼륨 변경
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume;
    }

    /// <summary>
    /// 효과음 볼륨 변경
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }
}

/// <summary>
/// 마을 사운드 그룹
/// </summary>
[System.Serializable]
public class TownSoundData
{
    [Header("BGM")]
    [Tooltip("마을 배경음악")]
    public AudioClip townBGM;

    [Header("상점 효과음")]
    [Tooltip("아이템 구매 효과음")]
    public AudioClip shopBuySound;

    [Tooltip("아이템 판매 효과음")]
    public AudioClip shopSellSound;
}

/// <summary>
/// 던전 사운드 그룹
/// </summary>
[System.Serializable]
public class DungeonSoundData
{
    [Header("BGM")]
    [Tooltip("던전 입구 배경음악")]
    public AudioClip entranceBGM;

    [Tooltip("던전 통로 배경음악")]
    public AudioClip corridorBGM;

    [Tooltip("이벤트 배경음악")]
    public AudioClip eventBGM;
}

/// <summary>
/// 전투 사운드 그룹
/// </summary>
[System.Serializable]
public class CombatSoundData
{
    [Header("BGM")]
    [Tooltip("일반 전투 배경음악")]
    public AudioClip normalCombatBGM;

    [Tooltip("보스 전투 배경음악")]
    public AudioClip bossBGM;

    [Header("공격 효과음")]
    [Tooltip("기본 공격 효과음")]
    public AudioClip basicAttackSound;

    [Header("피격 효과음")]
    [Tooltip("일반 피격 효과음")]
    public AudioClip hitSound;

    [Tooltip("크리티컬 피격 효과음")]
    public AudioClip criticalSound;

    [Header("사망 효과음")]
    [Tooltip("몬스터 사망 효과음")]
    public AudioClip monsterDeathSound;

    [Tooltip("용병 사망 효과음")]
    public AudioClip characterDeathSound;
}

/// <summary>
/// 미니게임 사운드 그룹
/// </summary>
[System.Serializable]
public class MinigameSoundData
{
    [Header("TPE 미니게임")]
    [Tooltip("TPE 성공 효과음")]
    public AudioClip tpeSuccessSound;

    [Tooltip("TPE 실패 효과음")]
    public AudioClip tpeFailSound;

    [Header("패링 미니게임")]
    [Tooltip("패링 성공 효과음")]
    public AudioClip parrySuccessSound;

    [Tooltip("패링 실패 효과음")]
    public AudioClip parryFailSound;
}

/// <summary>
/// UI 사운드 그룹
/// </summary>
[System.Serializable]
public class UISoundData
{
    [Header("버튼 효과음")]
    [Tooltip("버튼 클릭 효과음")]
    public AudioClip buttonClickSound;

    [Header("보상 효과음")]
    [Tooltip("아이템 획득 효과음")]
    public AudioClip itemGetSound;

    [Tooltip("골드 획득 효과음")]
    public AudioClip goldGetSound;
}