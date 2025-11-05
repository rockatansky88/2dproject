using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// 공통 확인 팝업 (OK/Cancel)
/// - 던전 퇴장, 아이템 삭제 등 확인이 필요한 작업에 사용
/// - 싱글톤으로 관리되어 어디서든 호출 가능
/// - DontDestroyOnLoad로 씬 전환 시에도 유지됨
/// </summary>
public class ConfirmationPopup : MonoBehaviour
{
	public static ConfirmationPopup Instance { get; private set; }

	[Header("UI References")]
	[SerializeField] private GameObject popupRoot;       // 전체 팝업 루트
	[SerializeField] private Text titleText;             // 제목
	[SerializeField] private Text messageText;           // 메시지 텍스트
	[SerializeField] private Button confirmButton;       // 확인 버튼
	[SerializeField] private Button cancelButton;        // 취소 버튼

	private Action onConfirmCallback;
	private Action onCancelCallback;

	private void Awake()
	{

		// 싱글톤 설정
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject); // 🆕 추가: 씬 전환 시에도 유지
		}
		else
		{
			Debug.LogWarning("[ConfirmationPopup] ⚠️ 중복 인스턴스 파괴됨");
			Destroy(gameObject);
			return;
		}

		// 버튼 리스너 등록
		if (confirmButton != null)
		{
			confirmButton.onClick.AddListener(OnConfirmClicked);
		}
		else
		{
			Debug.LogError("[ConfirmationPopup] ❌ confirmButton이 null입니다!");
		}

		if (cancelButton != null)
		{
			cancelButton.onClick.AddListener(OnCancelClicked);
		}
		else
		{
			Debug.LogError("[ConfirmationPopup] ❌ cancelButton이 null입니다!");
		}

		// 초기 상태: 비활성화
		if (popupRoot != null)
		{
			popupRoot.SetActive(false);
		}
		else
		{
			Debug.LogError("[ConfirmationPopup] ❌ popupRoot가 null입니다!");
		}

	}

	/// <summary>
	/// 확인 팝업 표시
	/// </summary>
	/// <param name="message">메시지 내용</param>
	/// <param name="onConfirm">확인 버튼 클릭 시 콜백</param>
	/// <param name="onCancel">취소 버튼 클릭 시 콜백 (선택사항)</param>
	/// <param name="title">제목 (선택사항)</param>
	public void Show(string message, Action onConfirm, Action onCancel = null, string title = "확인")
	{

		if (popupRoot != null)
		{
			popupRoot.SetActive(true);
		}

		// 텍스트 설정
		if (titleText != null)
		{
			titleText.text = title;
		}

		if (messageText != null)
		{
			messageText.text = message;
		}

		// 콜백 저장
		onConfirmCallback = onConfirm;
		onCancelCallback = onCancel;

	}

	/// <summary>
	/// 팝업 닫기
	/// </summary>
	public void Hide()
	{

		if (popupRoot != null)
		{
			popupRoot.SetActive(false);
		}

		// 콜백 초기화
		onConfirmCallback = null;
		onCancelCallback = null;
	}

	/// <summary>
	/// 확인 버튼 클릭
	/// </summary>
	private void OnConfirmClicked()
	{

		onConfirmCallback?.Invoke();
		Hide();
	}

	/// <summary>
	/// 취소 버튼 클릭
	/// </summary>
	private void OnCancelClicked()
	{

		onCancelCallback?.Invoke();
		Hide();
	}

	private void OnDestroy()
	{

		if (confirmButton != null)
		{
			confirmButton.onClick.RemoveListener(OnConfirmClicked);
		}

		if (cancelButton != null)
		{
			cancelButton.onClick.RemoveListener(OnCancelClicked);
		}
	}
}