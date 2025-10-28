using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 던전 UI 관리 (입구, 통로, 이벤트, 전투 화면)
/// - 카메라 이동 방식으로 화면 전환
/// - CanvasGroup을 사용한 페이드 인/아웃 효과
/// </summary>
public class DungeonUIManager : MonoBehaviour
{
    public static DungeonUIManager Instance { get; private set; }

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float cameraMoveDuration = 1f; // 카메라 이동 시간

    [Header("Positions")]
    [SerializeField] private Transform townPosition;      // 마을 위치
    [SerializeField] private Transform entrancePosition;  // 던전 입구 위치
    [SerializeField] private Transform corridorPosition;  // 던전 통로 위치
    [SerializeField] private Transform eventPosition;     // 이벤트 맵 위치
    [SerializeField] private Transform combatPosition;    // 전투 맵 위치

    [Header("UI Panels")]
    [SerializeField] private GameObject townUI;           // 마을 UI
    [SerializeField] private GameObject entranceUI;       // 던전 입구 UI
    [SerializeField] private GameObject corridorUI;       // 통로 선택 UI (3갈래)
    [SerializeField] private GameObject eventUI;          // 이벤트 UI
    [SerializeField] private GameObject combatUI;         // 전투 UI

    [Header("Entrance UI Elements")]
    [SerializeField] private Image entranceBackgroundImage;
    [SerializeField] private Text entranceTitleText;
    [SerializeField] private Text entranceDescriptionText;
    [SerializeField] private Button enterDungeonButton;
    [SerializeField] private Button backToTownButton;

    [Header("Corridor UI Elements")]
    [SerializeField] private Image corridorBackgroundImage;
    [SerializeField] private Text roomProgressText;       // "방 1/5"
    [SerializeField] private Button[] pathButtons;        // 3갈래 버튼 (0~2)

    [Header("Event UI Elements")]
    [SerializeField] private Image eventBackgroundImage;
    [SerializeField] private Image eventIllustrationImage;
    [SerializeField] private Text eventTitleText;
    [SerializeField] private Text eventDescriptionText;
    [SerializeField] private Button proceedButton;        // 다음으로 버튼

    [Header("Combat UI Elements")]
    [SerializeField] private Image combatBackgroundImage;
    [SerializeField] private Transform monsterSpawnParent; // 몬스터 스프라이트 부모
    [SerializeField] private GameObject monsterPrefab;     // 몬스터 UI 프리팹

    private CanvasGroup currentActiveGroup;
    private DungeonDataSO currentDungeonData;

    private void Awake()
    {
        Debug.Log("[DungeonUIManager] ━━━ Awake 시작 ━━━");

        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[DungeonUIManager] 싱글톤 인스턴스 생성됨");
        }
        else
        {
            Debug.LogWarning("[DungeonUIManager] 중복 인스턴스 파괴됨");
            Destroy(gameObject);
            return;
        }

        // 버튼 리스너 등록
        if (enterDungeonButton != null)
        {
            enterDungeonButton.onClick.AddListener(OnEnterDungeonClicked);
            Debug.Log("[DungeonUIManager] 던전 입장 버튼 리스너 등록");
        }

        if (backToTownButton != null)
        {
            backToTownButton.onClick.AddListener(OnBackToTownClicked);
            Debug.Log("[DungeonUIManager] 마을로 돌아가기 버튼 리스너 등록");
        }

        if (proceedButton != null)
        {
            proceedButton.onClick.AddListener(OnProceedClicked);
            Debug.Log("[DungeonUIManager] 다음으로 버튼 리스너 등록");
        }

        // 3갈래 버튼 리스너 등록
        for (int i = 0; i < pathButtons.Length; i++)
        {
            int index = i; // 클로저 문제 방지
            pathButtons[i].onClick.AddListener(() => OnPathSelected(index));
            Debug.Log($"[DungeonUIManager] 통로 {index}번 버튼 리스너 등록");
        }

        // DungeonManager 이벤트 구독
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.OnDungeonEntered += OnDungeonEntered;
            DungeonManager.Instance.OnRoomProgressed += OnRoomProgressed;
            DungeonManager.Instance.OnRoomTypeSelected += OnRoomTypeSelected;
            DungeonManager.Instance.OnMonstersSpawned += OnMonstersSpawned;
            DungeonManager.Instance.OnEventTriggered += OnEventTriggered;
            Debug.Log("[DungeonUIManager] DungeonManager 이벤트 구독 완료");
        }

        // CombatManager 이벤트 구독
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombatEnded += OnCombatEnded;
            Debug.Log("[DungeonUIManager] CombatManager 이벤트 구독");
        }

        // 초기 상태: 마을 UI만 활성화
        ShowTownUI();

        Debug.Log("[DungeonUIManager] ? Awake 완료");
    }

    /// <summary>
    /// 마을 UI 표시
    /// </summary>
    private void ShowTownUI()
    {
        Debug.Log("[DungeonUIManager] 마을 UI 표시");

        HideAllUI();
        townUI.SetActive(true);

        // 카메라 위치 이동
        if (mainCamera != null && townPosition != null)
        {
            StartCoroutine(MoveCameraSmooth(townPosition.position));
        }
    }

    /// <summary>
    /// 던전 입구 UI 표시
    /// </summary>
    public void ShowEntranceUI(DungeonDataSO dungeon)
    {
        Debug.Log($"[DungeonUIManager] 던전 입구 UI 표시: {dungeon.dungeonName}");

        currentDungeonData = dungeon;

        HideAllUI();
        entranceUI.SetActive(true);

        // 배경 이미지
        if (entranceBackgroundImage != null)
        {
            entranceBackgroundImage.sprite = dungeon.entranceSprite;
        }

        // 제목
        if (entranceTitleText != null)
        {
            entranceTitleText.text = dungeon.dungeonName;
        }

        // 설명
        if (entranceDescriptionText != null)
        {
            entranceDescriptionText.text = $"권장 레벨: {dungeon.recommendedLevel}\n총 {dungeon.totalRooms}개의 방을 돌파하세요!";
        }

        // 카메라 이동
        if (mainCamera != null && entrancePosition != null)
        {
            StartCoroutine(MoveCameraSmooth(entrancePosition.position));
        }

        Debug.Log("[DungeonUIManager] ? 입구 UI 표시 완료");
    }

    /// <summary>
    /// 통로 선택 UI 표시 (3갈래)
    /// </summary>
    private void ShowCorridorUI()
    {
        Debug.Log("[DungeonUIManager] 통로 선택 UI 표시");

        HideAllUI();
        corridorUI.SetActive(true);

        // 배경 이미지
        if (corridorBackgroundImage != null && currentDungeonData != null)
        {
            corridorBackgroundImage.sprite = currentDungeonData.corridorSprite;
        }

        // 카메라 이동
        if (mainCamera != null && corridorPosition != null)
        {
            StartCoroutine(MoveCameraSmooth(corridorPosition.position));
        }

        Debug.Log("[DungeonUIManager] ? 통로 UI 표시 완료");
    }

    /// <summary>
    /// 이벤트 UI 표시
    /// </summary>
    private void ShowEventUI(RoomEventDataSO eventData)
    {
        Debug.Log($"[DungeonUIManager] 이벤트 UI 표시: {eventData.eventName}");

        HideAllUI();
        eventUI.SetActive(true);

        // 배경 이미지
        if (eventBackgroundImage != null && currentDungeonData != null)
        {
            eventBackgroundImage.sprite = currentDungeonData.eventBackgroundSprite;
        }

        // 이벤트 일러스트
        if (eventIllustrationImage != null)
        {
            eventIllustrationImage.sprite = eventData.eventImage;
        }

        // 제목
        if (eventTitleText != null)
        {
            eventTitleText.text = eventData.eventName;
        }

        // 설명
        if (eventDescriptionText != null)
        {
            eventDescriptionText.text = eventData.description;
        }

        // 카메라 이동
        if (mainCamera != null && eventPosition != null)
        {
            StartCoroutine(MoveCameraSmooth(eventPosition.position));
        }

        // 이벤트 효과 자동 적용
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.ApplyEventEffects();
        }

        Debug.Log("[DungeonUIManager] ? 이벤트 UI 표시 완료");
    }

    /// <summary>
    /// 전투 UI 표시
    /// </summary>
    private void ShowCombatUI(bool isBoss)
    {
        Debug.Log($"[DungeonUIManager] 전투 UI 표시 (보스: {isBoss})");

        HideAllUI();
        combatUI.SetActive(true);

        // 배경 이미지
        if (combatBackgroundImage != null && currentDungeonData != null)
        {
            if (isBoss)
            {
                combatBackgroundImage.sprite = currentDungeonData.bossBackgroundSprite;
            }
            else
            {
                combatBackgroundImage.sprite = currentDungeonData.combatBackgroundSprite;
            }
        }

        // 카메라 이동
        if (mainCamera != null && combatPosition != null)
        {
            StartCoroutine(MoveCameraSmooth(combatPosition.position));
        }

        Debug.Log("[DungeonUIManager] ? 전투 UI 표시 완료");
    }

    /// <summary>
    /// 모든 UI 숨기기
    /// </summary>
    private void HideAllUI()
    {
        Debug.Log("[DungeonUIManager] 모든 UI 숨김");

        townUI.SetActive(false);
        entranceUI.SetActive(false);
        corridorUI.SetActive(false);
        eventUI.SetActive(false);
        combatUI.SetActive(false);
    }

    /// <summary>
    /// 카메라 부드럽게 이동
    /// </summary>
    private IEnumerator MoveCameraSmooth(Vector3 targetPosition)
    {
        Debug.Log($"[DungeonUIManager] 카메라 이동 시작 → {targetPosition}");

        Vector3 startPosition = mainCamera.transform.position;
        float elapsed = 0f;

        while (elapsed < cameraMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / cameraMoveDuration);

            // Ease-in-out 효과
            t = t * t * (3f - 2f * t);

            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        mainCamera.transform.position = targetPosition;

        Debug.Log("[DungeonUIManager] ? 카메라 이동 완료");
    }

    /// <summary>
    /// 몬스터 스프라이트 생성
    /// </summary>
    private void SpawnMonsterSprites(List<MonsterSpawnData> monsters)
    {
        Debug.Log($"[DungeonUIManager] ━━━ 몬스터 스프라이트 생성: {monsters.Count}마리 ━━━");

        // 기존 몬스터 제거
        foreach (Transform child in monsterSpawnParent)
        {
            Destroy(child.gameObject);
        }

        // 새 몬스터 생성
        for (int i = 0; i < monsters.Count; i++)
        {
            MonsterSpawnData monsterData = monsters[i];

            GameObject monsterObj = Instantiate(monsterPrefab, monsterSpawnParent);
            Image monsterImage = monsterObj.GetComponent<Image>();

            if (monsterImage != null)
            {
                monsterImage.sprite = monsterData.monsterSprite;
                Debug.Log($"[DungeonUIManager] 몬스터 {i + 1} 생성: {monsterData.monsterName}");
            }

            // 위치 조정 (가로로 배치)
            RectTransform rect = monsterObj.GetComponent<RectTransform>();
            if (rect != null)
            {
                float spacing = 200f;
                float offset = (monsters.Count - 1) * spacing * -0.5f;
                rect.anchoredPosition = new Vector2(offset + i * spacing, 0f);
            }
        }

        Debug.Log("[DungeonUIManager] ? 몬스터 스프라이트 생성 완료");
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // ?? 버튼 이벤트 핸들러
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    /// <summary>
    /// 던전 입장 버튼 클릭
    /// </summary>
    private void OnEnterDungeonClicked()
    {
        Debug.Log("[DungeonUIManager] ??? 던전 입장 버튼 클릭");

        if (currentDungeonData == null)
        {
            Debug.LogError("[DungeonUIManager] ? currentDungeonData가 null입니다!");
            return;
        }

        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.EnterDungeon(currentDungeonData);
        }
    }

    /// <summary>
    /// 마을로 돌아가기 버튼 클릭
    /// </summary>
    private void OnBackToTownClicked()
    {
        Debug.Log("[DungeonUIManager] ??? 마을로 돌아가기 버튼 클릭");

        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.ExitDungeon();
        }

        ShowTownUI();
    }

    /// <summary>
    /// 통로 선택 버튼 클릭 (0~2)
    /// </summary>
    private void OnPathSelected(int pathIndex)
    {
        Debug.Log($"[DungeonUIManager] ??? 통로 {pathIndex}번 선택");

        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.SelectPath(pathIndex);
        }
    }

    /// <summary>
    /// 다음으로 버튼 클릭 (이벤트 후)
    /// </summary>
    private void OnProceedClicked()
    {
        Debug.Log("[DungeonUIManager] ??? 다음으로 버튼 클릭");

        // 던전 클리어 체크
        if (DungeonManager.Instance != null && DungeonManager.Instance.IsDungeonCleared())
        {
            Debug.Log("[DungeonUIManager] ? 던전 클리어!");
            OnBackToTownClicked();
            return;
        }

        // 다음 방으로 이동
        ShowCorridorUI();
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // ?? DungeonManager 이벤트 핸들러
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    /// <summary>
    /// 던전 입장 시
    /// </summary>
    private void OnDungeonEntered(DungeonDataSO dungeon)
    {
        Debug.Log($"[DungeonUIManager] ?? OnDungeonEntered 이벤트: {dungeon.dungeonName}");

        // 첫 번째 통로 선택 화면 표시
        ShowCorridorUI();
    }

    /// <summary>
    /// 방 진행 시
    /// </summary>
    private void OnRoomProgressed(int currentRoom, int totalRooms)
    {
        Debug.Log($"[DungeonUIManager] ?? OnRoomProgressed 이벤트: {currentRoom}/{totalRooms}");

        // 진행도 텍스트 업데이트
        if (roomProgressText != null)
        {
            roomProgressText.text = $"방 {currentRoom}/{totalRooms}";
        }
    }

    /// <summary>
    /// 방 타입 선택 완료 시
    /// </summary>
    private void OnRoomTypeSelected(DungeonRoomType roomType)
    {
        Debug.Log($"[DungeonUIManager] ?? OnRoomTypeSelected 이벤트: {roomType}");

        // 방 타입에 따라 UI 표시
        switch (roomType)
        {
            case DungeonRoomType.Event:
                // 이벤트 UI는 OnEventTriggered에서 표시
                break;

            case DungeonRoomType.Combat:
                ShowCombatUI(false);
                break;

            case DungeonRoomType.Boss:
                ShowCombatUI(true);
                break;
        }
    }

    /// <summary>
    /// 몬스터 스폰 시
    /// </summary>
    private void OnMonstersSpawned(List<MonsterSpawnData> monsters)
    {
        Debug.Log($"[DungeonUIManager] 🎯 OnMonstersSpawned 이벤트: {monsters.Count}마리");

        SpawnMonsterSprites(monsters);

        // 전투 시작
        if (CombatManager.Instance != null)
        {
            bool isBoss = DungeonManager.Instance.GetCurrentRoomType() == DungeonRoomType.Boss;
            CombatManager.Instance.StartCombat(monsters, isBoss);
        }
    }

    /// <summary>
    /// 이벤트 발생 시
    /// </summary>
    private void OnEventTriggered(RoomEventDataSO eventData)
    {
        Debug.Log($"[DungeonUIManager] ?? OnEventTriggered 이벤트: {eventData.eventName}");

        ShowEventUI(eventData);
    }

    /// <summary>
    /// 전투 종료 시 호출
    /// </summary>
    private void OnCombatEnded(bool isVictory)
    {
        Debug.Log($"[DungeonUIManager] 🎯 OnCombatEnded 이벤트: {(isVictory ? "승리" : "패배")}");

        if (isVictory)
        {
            // 던전 클리어 체크
            if (DungeonManager.Instance != null && DungeonManager.Instance.IsDungeonCleared())
            {
                Debug.Log("[DungeonUIManager] ✅ 던전 클리어! 마을로 귀환");
                OnBackToTownClicked();
            }
            else
            {
                Debug.Log("[DungeonUIManager] 다음 방으로 이동");
                ShowCorridorUI();
            }
        }
        else
        {
            Debug.Log("[DungeonUIManager] 패배! 마을로 귀환");
            OnBackToTownClicked();
        }
    }

    private void OnDestroy()
    {
        // 버튼 리스너 해제
        if (enterDungeonButton != null)
        {
            enterDungeonButton.onClick.RemoveListener(OnEnterDungeonClicked);
        }

        if (backToTownButton != null)
        {
            backToTownButton.onClick.RemoveListener(OnBackToTownClicked);
        }

        if (proceedButton != null)
        {
            proceedButton.onClick.RemoveListener(OnProceedClicked);
        }

        foreach (var button in pathButtons)
        {
            button.onClick.RemoveAllListeners();
        }

        // DungeonManager 이벤트 구독 해제
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.OnDungeonEntered -= OnDungeonEntered;
            DungeonManager.Instance.OnRoomProgressed -= OnRoomProgressed;
            DungeonManager.Instance.OnRoomTypeSelected -= OnRoomTypeSelected;
            DungeonManager.Instance.OnMonstersSpawned -= OnMonstersSpawned;
            DungeonManager.Instance.OnEventTriggered -= OnEventTriggered;
        }

        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombatEnded -= OnCombatEnded;
        }
    }
}