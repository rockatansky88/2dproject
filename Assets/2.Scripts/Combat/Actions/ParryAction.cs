//using System.Collections;
//using UnityEngine;

//public class ParryAction : CombatAction
//{
//    private float parryWindow = 0.3f; // �и� ���� �ð�
//    private bool parrySucceeded;

//    public override IEnumerator Execute()
//    {
//        // �и� Ÿ�̹� UI ǥ��
//        UIManager.Instance.ShowParryTiming();

//        float timer = 0f;
//        parrySucceeded = false;

//        while (timer < parryWindow)
//        {
//            if (Input.GetKeyDown(KeyCode.Space))
//            {
//                parrySucceeded = true;
//                break;
//            }
//            timer += Time.deltaTime;
//            yield return null;
//        }

//        if (parrySucceeded)
//        {
//            // ������ �ݻ� or ��ȿȭ
//            attacker.ApplyBuff(new BuffData { /* ���� ���� */ });
//        }

//        yield return new WaitForSeconds(0.5f);
//    }

//    public override bool CanExecute() => true;
//}