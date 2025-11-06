using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 스프라이트 배열 기반 애니메이션 시스템
/// SpriteRenderer와 UI Image 모두 지원하며, 런타임에 애니메이션 클립을 동적으로 설정 가능
/// </summary>
public class SpriteAnimator : MonoBehaviour
{
    [Header("Target Component")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Image uiImage;

    [Header("Animation Data")]
    [SerializeField] private SpriteAnimationClip[] clips;

    private Coroutine currentCoroutine;
    private string currentClip;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (uiImage == null) uiImage = GetComponent<Image>();
    }

    /// <summary>
    /// 애니메이션 클립 동적 설정
    /// 런타임에 MonsterSpawnData 등에서 전달받은 클립 배열을 설정
    /// </summary>
    public void SetClips(SpriteAnimationClip[] newClips)
    {
        clips = newClips;
    }

    /// <summary>
    /// 애니메이션 재생
    /// 지정된 클립 이름의 애니메이션을 재생, 루프 여부 설정 가능
    /// </summary>
    /// <param name="clipName">애니메이션 클립 이름</param>
    /// <param name="loop">반복 재생 여부</param>
    public void Play(string clipName, bool loop = false)
    {
        SpriteAnimationClip clip = System.Array.Find(clips, c => c.name == clipName);

        if (clip == null || clip.frames.Length == 0)
        {
            return;
        }

        Stop();
        currentClip = clipName;
        currentCoroutine = StartCoroutine(PlayCoroutine(clip, loop));
    }

    /// <summary>
    /// 애니메이션 정지
    /// </summary>
    public void Stop()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
    }

    /// <summary>
    /// 현재 재생 중인 클립 이름
    /// </summary>
    public string CurrentClip => currentClip;

    /// <summary>
    /// 특정 클립이 재생 중인지 확인
    /// </summary>
    public bool IsPlaying(string clipName)
    {
        return currentClip == clipName && currentCoroutine != null;
    }

    private IEnumerator PlayCoroutine(SpriteAnimationClip clip, bool loop)
    {
        do
        {
            for (int i = 0; i < clip.frames.Length; i++)
            {
                SetSprite(clip.frames[i]);
                yield return new WaitForSeconds(clip.frameRate);
            }
        } while (loop);

        currentCoroutine = null;
        currentClip = null;
    }

    private void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
        else if (uiImage != null)
        {
            uiImage.sprite = sprite;
        }
    }
}

/// <summary>
/// 스프라이트 애니메이션 클립 데이터
/// 클립 이름, 프레임 배열, 프레임 속도를 포함
/// </summary>
[System.Serializable]
public class SpriteAnimationClip
{
    [Tooltip("클립 이름 (예: idle, attack, hit)")]
    public string name;

    [Tooltip("스프라이트 프레임 배열")]
    public Sprite[] frames;

    [Tooltip("프레임당 대기 시간 (초)")]
    public float frameRate = 0.1f;
}