//using UnityEngine;

//public class HealthBar : MonoBehaviour
//{
//    [SerializeField] private Image fillImage;
//    [SerializeField] private Text healthText;

//    private Character character;

//    public void Initialize(Character target)
//    {
//        character = target;
//        character.OnHealthChanged += UpdateDisplay;
//        UpdateDisplay();
//    }

//    private void UpdateDisplay()
//    {
//        float fillAmount = (float)character.CurrentHP / character.Stats.MaxHP;
//        fillImage.fillAmount = fillAmount;
//        healthText.text = $"{character.CurrentHP}/{character.Stats.MaxHP}";
//    }

//    private void OnDestroy()
//    {
//        if (character != null)
//        {
//            character.OnHealthChanged -= UpdateDisplay;
//        }
//    }
//}