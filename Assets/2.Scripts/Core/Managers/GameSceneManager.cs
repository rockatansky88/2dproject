using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 게임 씬 전환 관리자 (마을, 던전 입구, 통로, 이벤트, 전투 화면)
/// - 카메라 이동 방식으로 화면 전환
/// - 각 씬의 배경 이미지를 동적으로 설정
/// </summary>
public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }
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

    [Header("Town UI Elements")]
    [SerializeField] private Image townBackgroundImage;   // 마을 배경 이미지
    [SerializeField] private Sprite townBackgroundSprite; // 마을 배경 스프라이트
    [SerializeField] private Button[] townShopButtons;    // 상점 버튼들 (0: 던전입구, 1: 상점, 2: 용병상점)
    [SerializeField] private DungeonDataSO[] availableDungeons; // 🆕 추가

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

    [Header("Mercenary Party UI")]
    [SerializeField] private MercenaryParty mercenaryParty; // MercenaryParty 참조
    [SerializeField] private MercenaryWindow mercenaryWindow; // 🆕 추가

    private DungeonDataSO currentDungeonData;

    private void Awake()
    {
        Debug.Log("[GameSceneManager] ━━━ Awake 시작 ━━━");

        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[GameSceneManager] 싱글톤 인스턴스 생성됨");
        }
        else
        {
            Debug.LogWarning("[GameSceneManager] 중복 인스턴스 파괴됨");
            Destroy(gameObject);
            return;
        }

        // 버튼 리스너 등록
        if (enterDungeonButton != null)
        {
            enterDungeonButton.onClick.AddListener(OnEnterDungeonClicked);
            Debug.Log("[GameSceneManager] 던전 입장 버튼 리스너 등록");
        }

        if (backToTownButton != null)
        {
            backToTownButton.onClick.AddListener(OnBackToTownClicked);
            Debug.Log("[GameSceneManager] 마을로 돌아가기 버튼 리스너 등록");
        }

        if (proceedButton != null)
        {
            proceedButton.onClick.AddListener(OnProceedClicked);
            Debug.Log("[GameSceneManager] 다음으로 버튼 리스너 등록");
        }

        // 3갈래 버튼 리스너 등록
        for (int i = 0; i < pathButtons.Length; i++)
        {
            int index = i; // 클로저 문제 방지
            pathButtons[i].onClick.AddListener(() => OnPathSelected(index));
            Debug.Log($"[GameSceneManager] 통로 {index}번 버튼 리스너 등록");
        }

        // 마을 상점 버튼 리스너 등록
        if (townShopButtons != null && townShopButtons.Length >= 3)
        {
            townShopButtons[0].onClick.AddListener(OnDungeonEntranceClicked); // 던전입구
            townShopButtons[1].onClick.AddListener(OnMerchantShopClicked);    // 상점
            townShopButtons[2].onClick.AddListener(OnMercenaryShopClicked);   // 용병상점
            Debug.Log("[GameSceneManager] 마을 버튼 리스너 등록 완료");
        }

        Debug.Log("[GameSceneManager] ✅ Awake 완료");
    }

    private void Start()
    {
        Debug.Log("[GameSceneManager] ━━━ Start 시작 ━━━");

        // DungeonManager 이벤트 구독 (Start에서 수행)
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.OnDungeonEntered += OnDungeonEntered;
            DungeonManager.Instance.OnDungeonExited += OnDungeonExited;
            DungeonManager.Instance.OnRoomProgressed += OnRoomProgressed;
            DungeonManager.Instance.OnRoomTypeSelected += OnRoomTypeSelected;
            DungeonManager.Instance.OnMonstersSpawned += OnMonstersSpawned;
            DungeonManager.Instance.OnEventTriggered += OnEventTriggered;
            Debug.Log("[GameSceneManager] ✅ DungeonManager 이벤트 구독 완료");
        }

        // CombatManager 이벤트 구독
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombatEnded += OnCombatEnded;
            Debug.Log("[GameSceneManager] CombatManager 이벤트 구독");
        }

        // 초기 상태: 마을 UI만 활성화
        ShowTownUI();

        Debug.Log("[GameSceneManager] ✅ Start 완료");
    }

    /// <summary>
    /// 마을 UI 표시
    /// </summary>
    private void ShowTownUI()
    {
        Debug.Log("[GameSceneManager] ━━━ 마을 UI 표시 ━━━");

        HideAllUI();
        townUI.SetActive(true);

        // 마을 배경 이미지 설정
        if (townBackgroundImage != null && townBackgroundSprite != null)
        {
            townBackgroundImage.sprite = townBackgroundSprite;
            Debug.Log("[GameSceneManager] 마을 배경 이미지 설정됨");
        }

        // 파티 UI 표시 (마을에서는 항상 표시)
        if (mercenaryParty != null)
        {
            mercenaryParty.SetCombatMode(false); // 전투 모드 아님
            mercenaryParty.Show();
            Debug.Log("[GameSceneManager] 마을에서 파티 UI 표시");
        }

        // 카메라 위치 이동
        if (mainCamera != null && townPosition != null)
        {
            StartCoroutine(MoveCameraSmooth(townPosition.position));
        }

        Debug.Log("[GameSceneManager] ✅ 마을 UI 표시 완료");
    }

    /// <summary>
    /// 던전 입구 UI 표시
    /// </summary>
    public void ShowEntranceUI(DungeonDataSO dungeon)
    {
        Debug.Log($"[GameSceneManager] 던전 입구 UI 표시: {dungeon.dungeonName}");

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
            entranceDescriptionText.text = $"권장 레벨: {dungeon.recommendedLevel}\n 총 {dungeon.totalRooms}개의 방을 돌파하세요!";
        }

        // 파티 UI 숨김
        if (mercenaryParty != null)
        {
            mercenaryParty.Hide();
        }

        // 카메라 이동
        if (mainCamera != null && entrancePosition != null)
        {
            StartCoroutine(MoveCameraSmooth(entrancePosition.position));
        }

        Debug.Log("[GameSceneManager] ✅ 입구 UI 표시 완료");
    }

    /// <summary>
    /// 통로 선택 UI 표시 (3갈래)
    /// </summary>
    private void ShowCorridorUI()
    {
        Debug.Log("[GameSceneManager] 통로 선택 UI 표시");

        HideAllUI();
        corridorUI.SetActive(true);

        // 배경 이미지
        if (corridorBackgroundImage != null && currentDungeonData != null)
        {
            corridorBackgroundImage.sprite = currentDungeonData.corridorSprite;
        }

        // 파티 UI 숨김
        if (mercenaryParty != null)
        {
            mercenaryParty.Hide();
        }

        // 카메라 이동
        if (mainCamera != null && corridorPosition != null)
        {
            StartCoroutine(MoveCameraSmooth(corridorPosition.position));
        }

        Debug.Log("[GameSceneManager] ✅ 통로 UI 표시 완료");
    }

    /// <summary>
    /// 이벤트 UI 표시
    /// </summary>
    private void ShowEventUI(RoomEventDataSO eventData)
    {
        Debug.Log($"[GameSceneManager] 이벤트 UI 표시: {eventData.eventName}");

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

        // 파티 UI 표시 (이벤트는 전투 모드 아님)
        if (mercenaryParty != null)
        {
            mercenaryParty.SetCombatMode(false);
            mercenaryParty.Show();
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

        Debug.Log("[GameSceneManager] ✅ 이벤트 UI 표시 완료");
    }

    /// <summary>
    /// 전투 UI 표시
    /// </summary>
    private void ShowCombatUI(bool isBoss)
    {
        Debug.Log($"[GameSceneManager] 전투 UI 표시 (보스: {isBoss})");

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

        // 파티 UI 표시 (전투 모드)
        if (mercenaryParty != null)
        {
            mercenaryParty.SetCombatMode(true);
            mercenaryParty.Show();
        }

        // 카메라 이동
        if (mainCamera != null && combatPosition != null)
        {
            StartCoroutine(MoveCameraSmooth(combatPosition.position));
        }

        Debug.Log("[GameSceneManager] ✅ 전투 UI 표시 완료");
    }

    /// <summary>
    /// 모든 UI 숨기기
    /// </summary>
    private void HideAllUI()
    {
        Debug.Log("[GameSceneManager] 모든 UI 숨김");

        townUI.SetActive(false);
        entranceUI.SetActive(false);
        corridorUI.SetActive(false);
        eventUI.SetActive(false);
        combatUI.SetActive(false);
    }

    /// <summary>
    /// 카메라 부드럽게 이동 (Z축 유지)
    /// </summary>
    private IEnumerator MoveCameraSmooth(Vector3 targetPosition)
    {
        Debug.Log($"[GameSceneManager] 카메라 이동 시작 → {targetPosition}");

        Vector3 startPosition = mainCamera.transform.position;

        // Z 좌표를 유지하면서 이동 (카메라는 항상 -10 위치 유지)
        Vector3 adjustedTarget = new Vector3(targetPosition.x, targetPosition.y, startPosition.z);

        float elapsed = 0f;

        while (elapsed < cameraMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / cameraMoveDuration);

            // Ease-in-out 효과
            t = t * t * (3f - 2f * t);

            mainCamera.transform.position = Vector3.Lerp(startPosition, adjustedTarget, t);
            yield return null;
        }

        mainCamera.transform.position = adjustedTarget;

        Debug.Log("[GameSceneManager] ✅ 카메라 이동 완료");
    }

    /// <summary>
    /// 몬스터 스프라이트 생성
    /// </summary>
    private void SpawnMonsterSprites(List<MonsterSpawnData> monsters)
    {
        Debug.Log($"[GameSceneManager] ━━━ 몬스터 스프라이트 생성: {monsters.Count}마리 ━━━");

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
                Debug.Log($"[GameSceneManager] 몬스터 {i + 1} 생성: {monsterData.monsterName}");
            }

            // 위치 조정 (가로로 배치)
            RectTransform rect = monsterObj.GetComponent<RectTransform>();
            if (rect != null)
            {
                float spacing = 400f;
                float offset = (monsters.Count - 1) * spacing * -0.5f;
                rect.anchoredPosition = new Vector2(offset + i * spacing, 0f);
            }
        }

        Debug.Log("[GameSceneManager] ✅ 몬스터 스프라이트 생성 완료");
    }

    private void OnEnterDungeonClicked()
    {
        Debug.Log("[GameSceneManager] 🖱️ 던전 입장 버튼 클릭");

        if (currentDungeonData == null)
        {
            Debug.LogError("[GameSceneManager] ❌ currentDungeonData가 null입니다!");
            return;
        }

        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.EnterDungeon(currentDungeonData);
        }
    }

    private void OnBackToTownClicked()
    {
        Debug.Log("[GameSceneManager] 🖱️ 마을로 돌아가기 버튼 클릭");

        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.ExitDungeon();
        }

        ShowTownUI();
    }

    private void OnPathSelected(int pathIndex)
    {
        Debug.Log($"[GameSceneManager] 🖱️ 통로 {pathIndex}번 선택");

        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.SelectPath(pathIndex);
        }
    }

    private void OnProceedClicked()
    {
        Debug.Log("[GameSceneManager] 🖱️ 다음으로 버튼 클릭");

        // 던전 클리어 체크
        if (DungeonManager.Instance != null && DungeonManager.Instance.IsDungeonCleared())
        {
            Debug.Log("[GameSceneManager] ✅ 던전 클리어!");
            OnBackToTownClicked();
            return;
        }

        // 다음 방으로 이동
        ShowCorridorUI();
    }

    // 상점 버튼 클릭 - 수정
    private void OnMerchantShopClicked()
    {
        Debug.Log("[GameSceneManager] 🖱️ 상점 버튼 클릭");

        if (mercenaryParty != null)
        {
            mercenaryParty.Hide();
        }

        InventoryWindow inventoryWindow = FindObjectOfType<InventoryWindow>(true);
        if (inventoryWindow != null)
        {
            inventoryWindow.OpenShopMode();
            Debug.Log("[GameSceneManager] ✅ 상점 열림");
        }
        else
        {
            Debug.LogError("[GameSceneManager] ❌ InventoryWindow를 찾을 수 없습니다!");
        }
    }

    // 던전 입구 버튼 클릭 - 수정
    private void OnDungeonEntranceClicked()
    {
        Debug.Log("[GameSceneManager] 🖱️ 던전 입구 버튼 클릭");

        if (availableDungeons != null && availableDungeons.Length > 0)
        {
            DungeonDataSO selectedDungeon = availableDungeons[0];
            Debug.Log($"[GameSceneManager] 던전 선택: {selectedDungeon.dungeonName}");

            ShowEntranceUI(selectedDungeon);
        }
        else
        {
            Debug.LogError("[GameSceneManager] ❌ availableDungeons가 비어있습니다!");
        }
    }

    // 용병 상점 버튼 클릭 - 수정
    private void OnMercenaryShopClicked()
    {
        Debug.Log("[GameSceneManager] 🖱️ 용병 상점 버튼 클릭");

        if (mercenaryParty != null)
        {
            mercenaryParty.Hide();
        }

        if (mercenaryWindow != null)
        {
            mercenaryWindow.Open();
            Debug.Log("[GameSceneManager] ✅ 용병 상점 열림");
        }
        else
        {
            Debug.LogError("[GameSceneManager] ❌ MercenaryWindow가 할당되지 않았습니다!");
        }
    }

    private void OnDungeonEntered(DungeonDataSO dungeon)
    {
        Debug.Log($"[GameSceneManager] 📡 OnDungeonEntered 이벤트: {dungeon.dungeonName}");
        ShowCorridorUI();
    }

    /// <summary>
    /// 던전 퇴장 이벤트 핸들러 (마을 귀환 처리)
    /// 전투 상태를 완전히 초기화하고 마을 UI를 표시합니다.
    /// </summary>
    private void OnDungeonExited()
    {
        Debug.Log("[GameSceneManager] 📡 OnDungeonExited 이벤트 → 마을 귀환 처리");

        if (mercenaryParty != null)
        {
            mercenaryParty.ResetCombatState();
            Debug.Log("[GameSceneManager] ✅ MercenaryParty 전투 상태 초기화 완료");
        }

        // 마을 UI 표시
        ShowTownUI();
    }

    private void OnRoomProgressed(int currentRoom, int totalRooms)
    {
        Debug.Log($"[GameSceneManager] 📡 OnRoomProgressed 이벤트: {currentRoom}/{totalRooms}");

        if (roomProgressText != null)
        {
            roomProgressText.text = $"방 {currentRoom}/{totalRooms}";
        }
    }

    private void OnRoomTypeSelected(DungeonRoomType roomType)
    {
        Debug.Log($"[GameSceneManager] 📡 OnRoomTypeSelected 이벤트: {roomType}");

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

    private void OnMonstersSpawned(List<MonsterSpawnData> monsters)
    {
        Debug.Log($"[GameSceneManager] 🎯 OnMonstersSpawned 이벤트: {monsters.Count}마리");

        SpawnMonsterSprites(monsters);

        if (CombatManager.Instance != null)
        {
            bool isBoss = DungeonManager.Instance.GetCurrentRoomType() == DungeonRoomType.Boss;
            CombatManager.Instance.StartCombat(monsters, isBoss);
        }
    }

    private void OnEventTriggered(RoomEventDataSO eventData)
    {
        Debug.Log($"[GameSceneManager] 📡 OnEventTriggered 이벤트: {eventData.eventName}");
        ShowEventUI(eventData);
    }

    private void OnCombatEnded(bool isVictory)
    {
        Debug.Log($"[GameSceneManager] 🎯 OnCombatEnded 이벤트: {(isVictory ? "승리" : "패배")}");

        if (isVictory)
        {
            if (DungeonManager.Instance != null && DungeonManager.Instance.IsDungeonCleared())
            {
                Debug.Log("[GameSceneManager] ✅ 던전 클리어! 마을로 귀환");
                OnBackToTownClicked();
            }
            else
            {
                Debug.Log("[GameSceneManager] 다음 방으로 이동");
                ShowCorridorUI();
            }
        }
        else
        {
            Debug.Log("[GameSceneManager] 패배! 마을로 귀환");
            OnBackToTownClicked();
        }
    }

    private void OnDestroy()
    {
        // 버튼 리스너 해제
        if (enterDungeonButton != null)
            enterDungeonButton.onClick.RemoveListener(OnEnterDungeonClicked);

        if (backToTownButton != null)
            backToTownButton.onClick.RemoveListener(OnBackToTownClicked);

        if (proceedButton != null)
            proceedButton.onClick.RemoveListener(OnProceedClicked);

        foreach (var button in pathButtons)
        {
            button.onClick.RemoveAllListeners();
        }

        // 마을 버튼 리스너 해제
        if (townShopButtons != null)
        {
            foreach (var button in townShopButtons)
            {
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                }
            }
        }

        // DungeonManager 이벤트 구독 해제
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.OnDungeonEntered -= OnDungeonEntered;
            DungeonManager.Instance.OnDungeonExited -= OnDungeonExited;
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
