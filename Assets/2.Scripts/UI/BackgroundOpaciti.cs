using System.Collections;
using UnityEngine;

public class BackgroundOpaciti : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float fadeSpeed = 0.05f; // ���İ� ��ȭ �ӵ� (������ ����)
    private float minAlpha = 20f / 255f; // �ּ� ���İ� (����ȭ)
    private float maxAlpha = 60f / 255f; // �ִ� ���İ� (����ȭ)
    private bool fadingOut = true; // ���� ���İ� ���� ������ ����

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer�� �� ������Ʈ�� �����ϴ�.");
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
