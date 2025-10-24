//using System.Collections;
//using UnityEngine;

//public class ParryAction : CombatAction
//{
//    private float parryWindow = 0.3f; // 패링 가능 시간
//    private bool parrySucceeded;

//    public override IEnumerator Execute()
//    {
//        // 패링 타이밍 UI 표시
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
//            // 데미지 반사 or 무효화
//            attacker.ApplyBuff(new BuffData { /* 방어력 증가 */ });
//        }

//        yield return new WaitForSeconds(0.5f);
//    }

//    public override bool CanExecute() => true;
//}