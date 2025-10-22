using System.Collections;
using UnityEngine;

public class BackgroundOpaciti : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float fadeSpeed = 0.05f; // 알파값 변화 속도 (느리게 설정)
    private float minAlpha = 20f / 255f; // 최소 알파값 (정규화)
    private float maxAlpha = 60f / 255f; // 최대 알파값 (정규화)
    private bool fadingOut = true; // 현재 알파값 감소 중인지 여부

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer가 이 오브젝트에 없습니다.");
        }
    }

    void Update()
    {
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;

            if (fadingOut)
            {
                color.a -= fadeSpeed * Time.deltaTime;
                if (color.a <= minAlpha)
                {
                    color.a = minAlpha;
                    fadingOut = false;
                }
            }
            else
            {
                color.a += fadeSpeed * Time.deltaTime;
                if (color.a >= maxAlpha)
                {
                    color.a = maxAlpha;
                    fadingOut = true;
                }
            }

            spriteRenderer.color = color;
        }
    }
}
