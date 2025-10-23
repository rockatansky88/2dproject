using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public GameObject turnIndicator; // ������ ȭ��ǥ ������Ʈ
    public List<GameObject> players = new List<GameObject>(); // �÷��̾� ���
    public List<GameObject> monsters = new List<GameObject>(); // ���� ���
    private List<GameObject> turnOrder = new List<GameObject>(); // �� ���� ���
    private int currentTurnIndex = 0;

    public GameObject CurrentTurnCharacter { get; private set; } // ���� ���� ĳ����

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
        // �÷��̾�� ���͸� ã�Ƽ� ��Ͽ� �߰�
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        players.AddRange(playerObjects);
        monsters.AddRange(GameObject.FindGameObjectsWithTag("Monster"));

        // �� ���� �ʱ�ȭ
        UpdateTurnOrder();
        // ���� �� ����
        StartTurn();
    }

    // �� ���� ������Ʈ (�ӵ��� ����)
    public void UpdateTurnOrder()
    {
        turnOrder.Clear();
        turnOrder.AddRange(players);
        turnOrder.AddRange(monsters);

        // �ӵ��� ���� ������� ����
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
                return 0; // �Ӽ� ������ ���� ��� 0���� ó��
            }
        }).ToList();
    }

    public void StartTurn()
    {
        if (turnOrder.Count == 0) return;

        CurrentTurnCharacter = turnOrder[currentTurnIndex];
        Debug.Log("���� ��: " + CurrentTurnCharacter.name);

        // �� �ε������� ��ġ ������Ʈ
        turnIndicator.transform.position = CurrentTurnCharacter.transform.position + new Vector3(0, 1, 0); // ĳ���� ���� ǥ��

        // ���� ���� ��� �ڵ����� ����
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
        // ���Ͱ� �÷��̾ ����
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