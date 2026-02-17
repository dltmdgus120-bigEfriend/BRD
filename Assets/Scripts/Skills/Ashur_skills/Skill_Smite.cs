using UnityEngine;

[CreateAssetMenu(menuName = "Trickcal/Skills/Smite (Proc)")]
public class Skill_Smite : SkillBase
{
    [Header("강타 설정")]
    public int bonusDamage = 50;
    public GameObject hitVFX;

    public override void Execute(UnitStat user, Vector3 targetPos)
    {
        // 1. 이펙트 생성
        if (hitVFX != null)
        {
            Instantiate(hitVFX, targetPos, Quaternion.identity);
        }

        // 2. 데미지 처리
        // targetPos는 '투사체가 맞은 위치' 혹은 '적의 위치'입니다.
        // 아주 작은 범위(0.5f)로 체크해서 그 자리에 있는 적을 찾아냅니다.
        Collider[] hits = Physics.OverlapSphere(targetPos, 0.5f);

        foreach (var hit in hits)
        {
            EnemyHP enemy = hit.GetComponent<EnemyHP>();
            if (enemy != null)
            {                             
                enemy.TakeDamage(bonusDamage, attackType);

                break; // 강타는 보통 '단일' 추가타니까 한 명만 때리고 끝냅니다.
            }
        }
    }
}