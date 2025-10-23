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
        // 현재 턴이 플레이어인지, 타겟이 선택되었는지 확인
        if (TurnManager.Instance.CurrentTurnCharacter != null && TurnManager.Instance.CurrentTurnCharacter.GetComponent<PlayerAttack>() != null && TargetSelector.Instance.currentTarget != null)
        {
            button.interactable = true; // 버튼 활성화
            playerAttack = TurnManager.Instance.CurrentTurnCharacter.GetComponent<PlayerAttack>();
        }
        else
        {
            button.interactable = false; // 버튼 비활성화
        }
    }

    void OnAttackButtonClicked()
    {
        if (playerAttack != null && TargetSelector.Instance.currentTarget != null)
        {
            playerAttack.Attack(TargetSelector.Instance.currentTarget); // 플레이어가 몬스터 공격
        }
    }
}