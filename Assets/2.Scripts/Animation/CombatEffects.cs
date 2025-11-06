using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 전투 이펙트 유틸리티
/// 흔들림, 플래시, 스케일 펀치 등 코드 기반 효과를 제공합니다.
/// </summary>
public static class CombatEffects
{
    /// <summary>
    /// 크리티컬 흔들림 효과 (좌우 + 상하)
    /// 사용법: StartCoroutine(CombatEffects.CriticalShake(transform, 0.5f, 0.3f));
    /// </summary>
    /// <param name="target">흔들릴 Transform</param>
    /// <param name="strength">흔들림 강도</param>
    /// <param name="duration">지속 시간</param>
    public static IEnumerator CriticalShake(Transform target, float strength = 0.5f, float duration = 0.3f)
    {
        if (target == null) yield break;

        Vector3 originalPos = target.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // 흔들림 강도를 점차 감소
            float currentStrength = strength * (1f - (elapsed / duration));

            float offsetX = Random.Range(-currentStrength, currentStrength);
            float offsetY = Random.Range(-currentStrength, currentStrength);

            target.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);

            yield return null;
        }

        target.localPosition = originalPos;
    }

    /// <summary>
    /// 일반 피격 흔들림 (짧고 약하게)
    /// </summary>
    public static IEnumerator HitShake(Transform target, float strength = 0.2f, float duration = 0.15f)
    {
        if (target == null) yield break;

        Vector3 originalPos = target.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float offsetX = Random.Range(-strength, strength);
            float offsetY = Random.Range(-strength, strength);

            target.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);

            yield return null;
        }

        target.localPosition = originalPos;
    }

    /// <summary>
    /// 플래시 효과 (UI Image용)
    /// </summary>
    public static IEnumerator Flash(Image image, Color flashColor, float duration = 0.1f)
    {
        if (image == null) yield break;

        Color originalColor = image.color;
        image.color = flashColor;

        yield return new WaitForSeconds(duration);

        image.color = originalColor;
    }

    /// <summary>
    /// 플래시 효과 (SpriteRenderer용)
    /// </summary>
    public static IEnumerator Flash(SpriteRenderer sprite, Color flashColor, float duration = 0.1f)
    {
        if (sprite == null) yield break;

        Color originalColor = sprite.color;
        sprite.color = flashColor;

        yield return new WaitForSeconds(duration);

        sprite.color = originalColor;
    }

    /// <summary>
    /// 스케일 펀치 효과 (공격 시 살짝 확대)
    /// </summary>
    public static IEnumerator ScalePunch(Transform target, float strength = 1.2f, float duration = 0.2f)
    {
        if (target == null) yield break;

        Vector3 originalScale = target.localScale;
        Vector3 targetScale = originalScale * strength;
        float halfDuration = duration / 2f;
        float elapsed = 0f;

        // 확대
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            target.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        elapsed = 0f;

        // 축소
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            target.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        target.localScale = originalScale;
    }
}