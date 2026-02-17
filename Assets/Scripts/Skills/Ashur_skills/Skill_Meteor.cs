using UnityEngine;

[CreateAssetMenu(menuName = "Trickcal/Skills/Meteor Attack")]
public class Skill_Meteor : SkillBase
{
    [Header("메테오 설정")]
    public int damage = 100;      // 폭발 데미지    
    public GameObject explosionVFX; // 폭발 이펙트 프리팹 (파티클)

    public override void Execute(UnitStat user, Vector3 targetPos)
    {
        // 1. 이펙트 생성
        if (explosionVFX != null)
        {
            Instantiate(explosionVFX, targetPos, Quaternion.identity);
        }

        // 2. 범위 데미지 처리      
        Collider[] hitColliders = Physics.OverlapSphere(targetPos, effectRadius);

        foreach (var hit in hitColliders)
        {
            // 적 확인
            EnemyHP enemy = hit.GetComponent<EnemyHP>();

            if (enemy != null)
            {
                //스킬에 설정된 공격 타입(attackType)으로 데미지 전달
                enemy.TakeDamage(damage, attackType);
            }
        }
    }
}
