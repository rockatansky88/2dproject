using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// 인벤토리에 표시되는 용병 슬롯
/// 클릭하면 StatsPanel에 해당 용병의 스탯이 표시됩니다.
/// </summary>
public class MercenaryInventorySlot : MonoBehaviour
{
	[Header("UI References")]
	[SerializeField] private Image portraitImage;      // 초상화
	[SerializeField] private Text nameText;            // 이름
	[SerializeField] private Button slotButton;        // 클릭 버튼

	private MercenaryInstance mercenaryData;

	// 이벤트 (InventoryWindow가 구독)
	public event Action<MercenaryInstance> OnSlotClicked;

	private void Awake()
	{
		if (slotButton != null)
		{
			slotButton.onClick.AddListener(OnClicked);
		}
		else
		{
			Debug.LogError("[MercenaryInventorySlot] ? slotButton이 null입니다!");
		}
	}

	/// <summary>
	/// 슬롯 초기화 (용병 데이터 설정)
	/// </summary>
	public void Initialize(MercenaryInstance mercenary)
	{
		mercenaryData = mercenary;


		if (mercenary == null)
		{
			Debug.LogError("[MercenaryInventorySlot] ? mercenary가 null입니다!");
			return;
		}

		// 초상화
		if (portraitImage != null)
		{
			portraitImage.sprite = mercenary.portrait;
			portraitImage.enabled = mercenary.portrait != null;
		}

		// 이름
		if (nameText != null)
		{
			nameText.text = mercenary.mercenaryName;
		}

	}

	/// <summary>
	/// 슬롯 클릭 핸들러
	/// </summary>
	private void OnClicked()
	{

		if (mercenaryData != null)
		{
			OnSlotClicked?.Invoke(mercenaryData);
		}
		else
		{
			Debug.LogError("[MercenaryInventorySlot] ? mercenaryData가 null입니다!");
		}
	}

	private void OnDestroy()
	{
		if (slotButton != null)
		{
			slotButton.onClick.RemoveListener(OnClicked);
		}
	}
}