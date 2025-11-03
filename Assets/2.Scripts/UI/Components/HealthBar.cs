using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HP 바 UI 컴포넌트
/// - 캐릭터/몬스터 HP 표시
/// </summary>
public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;       // HP 바 Fill 이미지
    [SerializeField] private Text healthText;       // HP 텍스트 (옵션)

    /// <summary>
    /// HP 바 업데이트
    /// </summary>
    public void UpdateHealth(int currentHP, int maxHP)
    {
        if (fillImage == null)
        {
            Debug.LogWarning("[HealthBar] ⚠️ fillImage가 null입니다!");
            return;
        }

        float fillAmount = maxHP > 0 ? (float)currentHP / maxHP : 0f;
        fillImage.fillAmount = fillAmount;

        if (healthText != null)
        {
            healthText.text = $"{currentHP}/{maxHP}";
        }

        Debug.Log($"[HealthBar] HP 업데이트: {currentHP}/{maxHP} ({fillAmount * 100f:F1}%)");
    }

    /// <summary>
    /// HP 바 색상 변경 (옵션)
    /// </summary>
    public void SetHealthColor(Color color)
    {
        if (fillImage != null)
        {
            fillImage.color = color;
        }
    }
}