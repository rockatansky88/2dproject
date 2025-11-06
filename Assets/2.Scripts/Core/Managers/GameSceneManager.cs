using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 게임 씬 전환 관리자 (마을, 던전 입구, 통로, 이벤트, 전투 화면)
/// 카메라 이동 방식으로 화면 전환하며, 각 씬의 배경 이미지를 동적으로 설정합니다.
/// 로딩 화면과 던전 완료 화면을 3초간 표시 후 자동 전환합니다.
/// </summary>
public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float cameraMoveDuration = 1f;

    [Header("Positions")]
    [SerializeField] private Transform townPosition;
    [SerializeField] private Transform entrancePosition;
    [SerializeField] private Transform corridorPosition;
    [SerializeField] private Transform eventPosition;
    [SerializeField] private Transform combatPosition;
    [SerializeField] private Transform loadingPosition;
    [SerializeField] private Transform dungeonCompletePosition;

    [Header("UI Panels")]
    [SerializeField] private GameObject townUI;
    [SerializeField] private GameObject entranceUI;
    [SerializeField] private GameObject corridorUI;
    [SerializeField] private GameObject eventUI;
    [SerializeField] private GameObject combatUI;
    [SerializeField] private GameObject loadingUI;
    [SerializeField] private GameObject dungeonCompleteUI;

    [Header("Town UI Elements")]
    [SerializeField] private Image townBackgroundImage;
    [SerializeField] private Sprite townBackgroundSprite;
    [SerializeField] private Button[] townShopButtons;
    [SerializeField] private DungeonDataSO[] availableDungeons;

    [Header("Entrance UI Elements")]
    [SerializeField] private Image entranceBackgroundImage;
    [SerializeField] private Text entranceTitleText;
    [SerializeField] private Text entranceDescriptionText;
    [SerializeField] private Button enterDungeonButton;
    [SerializeField] private Button backToTownButton;

    [Header("Corridor UI Elements")]
    [SerializeField] private Image corridorBackgroundImage;
    [SerializeField] private Text roomProgressText;
    [SerializeField] private Button[] pathButtons;

    [Header("Event UI Elements")]
    [SerializeField] private Image eventBackgroundImage;
    [SerializeField] private Image eventIllustrationImage;
    [SerializeField] private Text eventTitleText;
    [SerializeField] private Text eventDescriptionText;
    [SerializeField] private Button proceedButton;

    [Header("Combat UI Elements")]
    [SerializeField] private Image combatBackgroundImage;
    [SerializeField] private Transform monsterSpawnParent;
    [SerializeField] private GameObject monsterPrefab;

    [Header("Loading UI Elements")]
    [SerializeField] private Image loadingBackgroundImage;

    [Header("Dungeon Complete UI Elements")]
    [SerializeField] private Image dungeonCompleteBackgroundImage;

    [Header("Mercenary Party UI")]
    [SerializeField] private MercenaryParty mercenaryParty;
    [SerializeField] private MercenaryWindow mercenaryWindow;

    [Header("Screen Display Duration")]
    [SerializeField] private float screenDisplayDuration = 3f;

    private DungeonDataSO currentDungeonData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (enterDungeonButton != null)
        {
            enterDungeonButton.onClick.AddListener(OnEnterDungeonClicked);
        }

        if (backToTownButton != null)
        {
            backToTownButton.onClick.AddListener(OnBackToTownClicked);
        }

        if (proceedButton != null)
        {
            proceedButton.onClick.AddListener(OnProceedClicked);
        }

        for (int i = 0; i < pathButtons.Length; i++)
        {
            int index = i;
            pathButtons[i].onClick.AddListener(() => OnPathSelected(index));
        }

        if (townShopButtons != null && townShopButtons.Length >= 3)
        {
            townShopButtons[0].onClick.AddListener(OnDungeonEntranceClicked);
            townShopButtons[1].onClick.AddListener(OnMerchantShopClicked);
            townShopButtons[2].onClick.AddListener(OnMercenaryShopClicked);
        }
    }

    private void Start()
    {
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.OnDungeonEntered += OnDungeonEntered;
            DungeonManager.Instance.OnDungeonExited += OnDungeonExited;
            DungeonManager.Instance.OnRoomProgressed += OnRoomProgressed;
            DungeonManager.Instance.OnRoomTypeSelected += OnRoomTypeSelected;
            DungeonManager.Instance.OnMonstersSpawned += OnMonstersSpawned;
            DungeonManager.Instance.OnEventTriggered += OnEventTriggered;
        }

        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombatEnded += OnCombatEnded;
        }

        ShowTownUI();
    }

    /// <summary>
    /// 마을 UI 표시
    /// </summary>
    private void ShowTownUI()
    {
        HideAllUI();
        townUI.SetActive(true);

        if (townBackgroundImage != null && townBackgroundSprite != null)
        {
            townBackgroundImage.sprite = townBackgroundSprite;
        }

        if (mercenaryParty != null)
        {
            mercenaryParty.SetCombatMode(false);
            mercenaryParty.Show();
        }

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
        currentDungeonData = dungeon;

        HideAllUI();
        entranceUI.SetActive(true);

        if (entranceBackgroundImage != null)
        {
            entranceBackgroundImage.sprite = dungeon.entranceSprite;
        }

        if (entranceTitleText != null)
        {
            entranceTitleText.text = dungeon.dungeonName;
        }

        if (entranceDescriptionText != null)
        {
            entranceDescriptionText.text = $"권장 레벨: {dungeon.recommendedLevel}\n 총 {dungeon.totalRooms}개의 방을 돌파하세요!";
        }

        if (mercenaryParty != null)
        {
            mercenaryParty.Hide();
        }

        if (mainCamera != null && entrancePosition != null)
        {
            StartCoroutine(MoveCameraSmooth(entrancePosition.position));
        }
    }

    /// <summary>
    /// 통로 선택 UI 표시 (3갈래)
    /// </summary>
    private void ShowCorridorUI()
    {
        HideAllUI();
        corridorUI.SetActive(true);

        if (corridorBackgroundImage != null && currentDungeonData != null)
        {
            corridorBackgroundImage.sprite = currentDungeonData.corridorSprite;
        }

        if (mercenaryParty != null)
        {
            mercenaryParty.Hide();
        }

        if (mainCamera != null && corridorPosition != null)
        {
            StartCoroutine(MoveCameraSmooth(corridorPosition.position));
        }
    }

    /// <summary>
    /// 이벤트 UI 표시
    /// 이벤트에서도 HP/MP UI를 표시하여 회복/피해 효과를 시각적으로 확인할 수 있습니다.
    /// </summary>
    private void ShowEventUI(RoomEventDataSO eventData)
    {
        HideAllUI();
        eventUI.SetActive(true);

        if (eventBackgroundImage != null && currentDungeonData != null)
        {
            eventBackgroundImage.sprite = currentDungeonData.eventBackgroundSprite;
        }

        if (eventIllustrationImage != null)
        {
            eventIllustrationImage.sprite = eventData.eventImage;
        }

        if (eventTitleText != null)
        {
            eventTitleText.text = eventData.eventName;
        }

        if (eventDescriptionText != null)
        {
            eventDescriptionText.text = eventData.description;
        }

        if (mercenaryParty != null)
        {
            mercenaryParty.SetCombatMode(true);
            mercenaryParty.Show();
        }

        if (mainCamera != null && eventPosition != null)
        {
            StartCoroutine(MoveCameraSmooth(eventPosition.position));
        }

        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.ApplyEventEffects();
        }
    }

    /// <summary>
    /// 전투 UI 표시
    /// </summary>
    private void ShowCombatUI(bool isBoss)
    {
        HideAllUI();
        combatUI.SetActive(true);

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

        if (mercenaryParty != null)
        {
            mercenaryParty.SetCombatMode(true);
            mercenaryParty.Show();
        }

        if (mainCamera != null && combatPosition != null)
        {
            StartCoroutine(MoveCameraSmooth(combatPosition.position));
        }
    }

    /// <summary>
    /// 로딩 화면 표시 (3초 후 던전 입장)
    /// PNG 이미지만 표시하고 3초 후 자동으로 던전으로 전환됩니다.
    /// </summary>
    private void ShowLoadingScreen()
    {
        StartCoroutine(LoadingScreenCoroutine());
    }

    /// <summary>
    /// 로딩 화면 코루틴
    /// </summary>
    private IEnumerator LoadingScreenCoroutine()
    {
        HideAllUI();
        loadingUI.SetActive(true);

        if (mercenaryParty != null)
        {
            mercenaryParty.Hide();
        }

        if (mainCamera != null && loadingPosition != null)
        {
            yield return StartCoroutine(MoveCameraSmooth(loadingPosition.position));
        }

        yield return new WaitForSeconds(screenDisplayDuration);

        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.EnterDungeon(currentDungeonData);
        }
    }

    /// <summary>
    /// 던전 완료 화면 표시 (3초 후 마을로 귀환)
    /// PNG 이미지만 표시하고 3초 후 자동으로 마을로 이동합니다.
    /// </summary>
    private void ShowDungeonCompleteScreen()
    {
        StartCoroutine(DungeonCompleteScreenCoroutine());
    }

    /// <summary>
    /// 던전 완료 화면 코루틴
    /// </summary>
    private IEnumerator DungeonCompleteScreenCoroutine()
    {
        HideAllUI();
        dungeonCompleteUI.SetActive(true);

        if (mercenaryParty != null)
        {
            mercenaryParty.Hide();
        }

        if (mainCamera != null && dungeonCompletePosition != null)
        {
            yield return StartCoroutine(MoveCameraSmooth(dungeonCompletePosition.position));
        }

        yield return new WaitForSeconds(screenDisplayDuration);

        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.ExitDungeon();
        }

        ShowTownUI();
    }

    /// <summary>
    /// 모든 UI 숨기기
    /// </summary>
    private void HideAllUI()
    {
        townUI.SetActive(false);
        entranceUI.SetActive(false);
        corridorUI.SetActive(false);
        eventUI.SetActive(false);
        combatUI.SetActive(false);
        loadingUI.SetActive(false);
        dungeonCompleteUI.SetActive(false);
    }

    /// <summary>
    /// 카메라 부드럽게 이동 (Z축 유지)
    /// </summary>
    private IEnumerator MoveCameraSmooth(Vector3 targetPosition)
    {
        Vector3 startPosition = mainCamera.transform.position;
        Vector3 adjustedTarget = new Vector3(targetPosition.x, targetPosition.y, startPosition.z);

        float elapsed = 0f;

        while (elapsed < cameraMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / cameraMoveDuration);
            t = t * t * (3f - 2f * t);

            mainCamera.transform.position = Vector3.Lerp(startPosition, adjustedTarget, t);
            yield return null;
        }

        mainCamera.transform.position = adjustedTarget;
    }

    /// <summary>
    /// 몬스터 스프라이트 생성
    /// </summary>
    private void SpawnMonsterSprites(List<MonsterSpawnData> monsters)
    {
        foreach (Transform child in monsterSpawnParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < monsters.Count; i++)
        {
            MonsterSpawnData monsterData = monsters[i];

            GameObject monsterObj = Instantiate(monsterPrefab, monsterSpawnParent);
            Image monsterImage = monsterObj.GetComponent<Image>();

            if (monsterImage != null)
            {
                monsterImage.sprite = monsterData.monsterSprite;
            }

            RectTransform rect = monsterObj.GetComponent<RectTransform>();
            if (rect != null)
            {
                float spacing = 400f;
                float offset = (monsters.Count - 1) * spacing * -0.5f;
                rect.anchoredPosition = new Vector2(offset + i * spacing, 0f);
            }
        }
    }

    private void OnEnterDungeonClicked()
    {
        if (currentDungeonData == null)
        {
            return;
        }

        ShowLoadingScreen();
    }

    private void OnBackToTownClicked()
    {
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.ExitDungeon();
        }

        ShowTownUI();
    }

    private void OnPathSelected(int pathIndex)
    {
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.SelectPath(pathIndex);
        }
    }

    private void OnProceedClicked()
    {
        UnityEngine.Debug.Log($"[GameSceneManager] OnProceedClicked 호출");

        if (DungeonManager.Instance != null && DungeonManager.Instance.IsDungeonCleared())
        {
            UnityEngine.Debug.Log($"[GameSceneManager] 이벤트 후 던전 클리어! 완료 화면 표시");
            ShowDungeonCompleteScreen();
            return;
        }

        UnityEngine.Debug.Log($"[GameSceneManager] 이벤트 후 던전 진행 중... 통로 화면 표시");
        ShowCorridorUI();
    }

    private void OnMerchantShopClicked()
    {
        if (mercenaryParty != null)
        {
            mercenaryParty.Hide();
        }

        InventoryWindow inventoryWindow = FindObjectOfType<InventoryWindow>(true);
        if (inventoryWindow != null)
        {
            inventoryWindow.OpenShopMode();
        }
    }

    private void OnDungeonEntranceClicked()
    {
        if (availableDungeons != null && availableDungeons.Length > 0)
        {
            DungeonDataSO selectedDungeon = availableDungeons[0];
            ShowEntranceUI(selectedDungeon);
        }
    }

    private void OnMercenaryShopClicked()
    {
        if (mercenaryParty != null)
        {
            mercenaryParty.Hide();
        }

        if (mercenaryWindow != null)
        {
            mercenaryWindow.Open();
        }
    }

    private void OnDungeonEntered(DungeonDataSO dungeon)
    {
        // 던전 입장 시 0/5 표시
        if (roomProgressText != null)
        {
            roomProgressText.text = $"던전 진행도 0/{dungeon.totalRooms}";
        }

        ShowCorridorUI();
    }

    /// <summary>
    /// 던전 퇴장 이벤트 핸들러 (마을 귀환 처리)
    /// 전투 상태를 완전히 초기화하고 마을 UI를 표시합니다.
    /// </summary>
    private void OnDungeonExited()
    {
        if (mercenaryParty != null)
        {
            mercenaryParty.ResetCombatState();
        }

        ShowTownUI();
    }

    private void OnRoomProgressed(int currentRoom, int totalRooms)
    {
        if (roomProgressText != null)
        {
            roomProgressText.text = $"던전 진행도 {currentRoom}/{totalRooms}";
        }
    }

    private void OnRoomTypeSelected(DungeonRoomType roomType)
    {
        switch (roomType)
        {
            case DungeonRoomType.Event:
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
        SpawnMonsterSprites(monsters);

        if (CombatManager.Instance != null)
        {
            bool isBoss = DungeonManager.Instance.GetCurrentRoomType() == DungeonRoomType.Boss;
            CombatManager.Instance.StartCombat(monsters, isBoss);
        }
    }

    private void OnEventTriggered(RoomEventDataSO eventData)
    {
        ShowEventUI(eventData);
    }

    private void OnCombatEnded(bool isVictory)
    {
        UnityEngine.Debug.Log($"[GameSceneManager] 전투 종료: isVictory={isVictory}");

        if (isVictory)
        {
            if (DungeonManager.Instance != null && DungeonManager.Instance.IsDungeonCleared())
            {
                UnityEngine.Debug.Log($"[GameSceneManager] 던전 클리어! 완료 화면 표시");
                ShowDungeonCompleteScreen();
            }
            else
            {
                UnityEngine.Debug.Log($"[GameSceneManager] 던전 진행 중... 통로 화면 표시");
                ShowCorridorUI();
            }
        }
        else
        {
            UnityEngine.Debug.Log($"[GameSceneManager] 전투 패배! 마을 귀환");
            OnBackToTownClicked();
        }
    }

    private void OnDestroy()
    {
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
