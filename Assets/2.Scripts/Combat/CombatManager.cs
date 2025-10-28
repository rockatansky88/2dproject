using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 전투 시스템 관리 (던전 전투 연동)
/// </summary>
public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    // 전투 종료 이벤트
    public event Action<bool> OnCombatEnded; // true: 승리, false: 패배

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[CombatManager] 싱글톤 인스턴스 생성");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 전투 시작 (DungeonManager에서 호출)
    /// </summary>
    public void StartCombat(List<MonsterSpawnData> monsters, bool isBoss)
    {
        Debug.Log($"[CombatManager] ━━━ 전투 시작: {monsters.Count}마리 (보스: {isBoss}) ━━━");

        // TODO: 전투 초기화 로직
        // - 파티 멤버 배치
        // - 몬스터 생성
        // - 턴 시스템 시작
    }

    /// <summary>
    /// 전투 종료 (승리/패배)
    /// </summary>
    public void EndCombat(bool isVictory)
    {
        Debug.Log($"[CombatManager] ━━━ 전투 종료: {(isVictory ? "승리" : "패배")} ━━━");

        OnCombatEnded?.Invoke(isVictory);

        if (isVictory)
        {
            // 승리 처리
            // - 보상 지급 (골드, 경험치)
            // - 다음 방으로 이동

            if (DungeonUIManager.Instance != null)
            {
                // 던전 클리어 체크
                if (DungeonManager.Instance.IsDungeonCleared())
                {
                    Debug.Log("[CombatManager] ✅ 던전 클리어!");
                    // 마을로 귀환
                }
                else
                {
                    Debug.Log("[CombatManager] 다음 방으로 이동");
                    // 통로 선택 화면으로
                }
            }
        }
        else
        {
            // 패배 처리
            // - 던전 강제 퇴장
            // - 페널티 적용

            if (DungeonManager.Instance != null)
            {
                DungeonManager.Instance.ExitDungeon();
            }
        }
    }
}