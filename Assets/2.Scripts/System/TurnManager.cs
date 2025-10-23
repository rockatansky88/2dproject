using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public GameObject turnIndicator; // 빨간색 화살표 오브젝트
    public List<GameObject> players = new List<GameObject>(); // 플레이어 목록
    public List<GameObject> monsters = new List<GameObject>(); // 몬스터 목록
    private List<GameObject> turnOrder = new List<GameObject>(); // 턴 순서 목록
    private int currentTurnIndex = 0;

    public GameObject CurrentTurnCharacter { get; private set; } // 현재 턴인 캐릭터

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 플레이어와 몬스터를 찾아서 목록에 추가
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        players.AddRange(playerObjects);
        monsters.AddRange(GameObject.FindGameObjectsWithTag("Monster"));

        // 턴 순서 초기화
        UpdateTurnOrder();
        // 시작 턴 설정
        StartTurn();
    }

    // 턴 순서 업데이트 (속도에 따라)
    public void UpdateTurnOrder()
    {
        turnOrder.Clear();
        turnOrder.AddRange(players);
        turnOrder.AddRange(monsters);

        // 속도가 높은 순서대로 정렬
        turnOrder = turnOrder.OrderByDescending(character =>
        {
            if (character.GetComponent<PlayerExperience>() != null && character.GetComponent<PlayerExperience>().characterStats != null)
            {
                return character.GetComponent<PlayerExperience>().characterStats.Speed;
            }
            else if (character.GetComponent<Monster>() != null && character.GetComponent<Monster>().monsterStats != null)
            {
                return character.GetComponent<Monster>().monsterStats.Speed;
            }
            else
            {
                return 0; // 속성 정보가 없는 경우 0으로 처리
            }
        }).ToList();
    }

    public void StartTurn()
    {
        if (turnOrder.Count == 0) return;

        CurrentTurnCharacter = turnOrder[currentTurnIndex];
        Debug.Log("현재 턴: " + CurrentTurnCharacter.name);

        // 턴 인디케이터 위치 업데이트
        turnIndicator.transform.position = CurrentTurnCharacter.transform.position + new Vector3(0, 1, 0); // 캐릭터 위에 표시

        // 몬스터 턴인 경우 자동으로 공격
        if (CurrentTurnCharacter.GetComponent<Monster>() != null)
        {
            MonsterTurn();
        }
    }

    public void EndTurn()
    {
        currentTurnIndex = (currentTurnIndex + 1) % turnOrder.Count;
        StartTurn();
    }

    private void MonsterTurn()
    {
        // 몬스터가 플레이어를 공격
        GameObject target = GetRandomPlayer();
        if (target != null)
        {
            CurrentTurnCharacter.GetComponent<Monster>().Attack(target);
        }

        EndTurn();
    }

    private GameObject GetRandomPlayer()
    {
        if (players.Count == 0) return null;
        return players[Random.Range(0, players.Count)];
    }
}