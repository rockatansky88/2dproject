using UnityEngine;
using UnityEngine.UI;

public class AttackButton : MonoBehaviour
{
    private Button button;
    private PlayerAttack playerAttack;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnAttackButtonClicked);
    }

    void Update()
    {
        // ���� ���� �÷��̾�����, Ÿ���� ���õǾ����� Ȯ��
        if (TurnManager.Instance.CurrentTurnCharacter != null && TurnManager.Instance.CurrentTurnCharacter.GetComponent<PlayerAttack>() != null && TargetSelector.Instance.currentTarget != null)
        {
            button.interactable = true; // ��ư Ȱ��ȭ
            playerAttack = TurnManager.Instance.CurrentTurnCharacter.GetComponent<PlayerAttack>();
        }
        else
        {
            button.interactable = false; // ��ư ��Ȱ��ȭ
        }
    }

    void OnAttackButtonClicked()
    {
        if (playerAttack != null && TargetSelector.Instance.currentTarget != null)
        {
            playerAttack.Attack(TargetSelector.Instance.currentTarget); // �÷��̾ ���� ����
        }
    }
}