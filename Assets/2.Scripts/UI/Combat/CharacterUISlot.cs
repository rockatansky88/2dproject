using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// 캐릭터 UI 슬롯
/// - HP/MP 바 표시
/// - 캐릭터 초상화
/// - 스탯 변화 실시간 반영
/// </summary>
public class CharacterUISlot : MonoBehaviour
{
	[Header("UI 참조")]
	[SerializeField] private Image portraitImage;
	[SerializeField] private Text nameText;

	[Header("HP 바")]
	[SerializeField] private Image hpFillImage;
	[SerializeField] private Text hpText;

	[Header("MP 바")]
	[SerializeField] private Image mpFillImage;
	[SerializeField] private Text mpText;

	[Header("현재 턴 표시")]
	[SerializeField] private GameObject turnIndicator;

	private Character character;

	/// <summary>
	/// 초기화
	/// </summary>
	public void Initialize(Character target)
	{
		character = target;

		if (character == null)
		{
			Debug.LogError("[CharacterUISlot] character가 null입니다!");
			return;
		}

		// 기본 정보 설정
		if (nameText != null)
		{
			nameText.text = character.Name;
		}

		if (portraitImage != null && character.mercenaryData != null)
		{
			portraitImage.sprite = character.mercenaryData.portrait;
		}

		// 스탯 변화 이벤트 구독
		character.Stats.OnHPChanged += UpdateHPBar;
		character.Stats.OnMPChanged += UpdateMPBar;

		// 초기 바 업데이트
		UpdateHPBar(character.Stats.CurrentHP, character.Stats.MaxHP);
		UpdateMPBar(character.Stats.CurrentMP, character.Stats.MaxMP);

		// 턴 표시 숨김
		if (turnIndicator != null)
		{
			turnIndicator.SetActive(false);
		}

	}

	/// <summary>
	/// HP 바 업데이트
	/// </summary>
	private void UpdateHPBar(int currentHP, int maxHP)
	{
		if (hpFillImage != null)
		{
			float fillAmount = maxHP > 0 ? (float)currentHP / maxHP : 0f;
			hpFillImage.fillAmount = fillAmount;

		}

		if (hpText != null)
		{
			hpText.text = $"{currentHP}/{maxHP}";
		}
	}

	/// <summary>
	/// MP 바 업데이트
	/// </summary>
	private void UpdateMPBar(int currentMP, int maxMP)
	{
		if (mpFillImage != null)
		{
			float fillAmount = maxMP > 0 ? (float)currentMP / maxMP : 0f;
			mpFillImage.fillAmount = fillAmount;

		}

		if (mpText != null)
		{
			mpText.text = $"{currentMP}/{maxMP}";
		}
	}

	/// <summary>
	/// 현재 턴 표시
	/// </summary>
	public void SetTurnActive(bool active)
	{
		if (turnIndicator != null)
		{
			turnIndicator.SetActive(active);
		}
	}

	/// <summary>
	/// 캐릭터 참조 반환
	/// </summary>
	public Character GetCharacter()
	{
		return character;
	}

	private void OnDestroy()
	{
		// 이벤트 구독 해제
		if (character != null && character.Stats != null)
		{
			character.Stats.OnHPChanged -= UpdateHPBar;
			character.Stats.OnMPChanged -= UpdateMPBar;
		}
	}
}