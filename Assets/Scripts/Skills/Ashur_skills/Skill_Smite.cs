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
        if (hitVFX != null) Instantiate(hitVFX, targetPos, Quaternion.identity);

        // 2. 해당 위치에 있는 적에게 추가 데미지
        // (단일 타겟이라고 가정하고 가장 가까운 적을 찾거나, targetPos 반경 체크)
        Collider[] hits = Physics.OverlapSphere(targetPos, 0.5f);
        foreach (var hit in hits)
        {
            EnemyHP enemy = hit.GetComponent<EnemyHP>();
            if (enemy != null)
            {
                enemy.TakeDamage(bonusDamage);
                break; // 한 명만 때림
            }
        }
    }
}