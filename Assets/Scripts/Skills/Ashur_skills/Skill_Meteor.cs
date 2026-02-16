using UnityEngine;

[CreateAssetMenu(menuName = "Trickcal/Skills/Meteor Attack")]
public class Skill_Meteor : SkillBase
{
    [Header("메테오 설정")]
    public int damage = 100;      // 폭발 데미지
    public float radius = 3.0f;   // 폭발 범위 (반경)
    public GameObject explosionVFX; // 폭발 이펙트 프리팹 (파티클)

    public override void Execute(UnitStat user, Vector3 targetPos)
    {
        // 타겟팅 모드로 찍은 좌표(targetPos)를 바로 씁니다!
       
        // 1. 이펙트
        if (explosionVFX != null) Instantiate(explosionVFX, targetPos, Quaternion.identity);

        // 2. 데미지
        Collider[] hitColliders = Physics.OverlapSphere(targetPos, radius);
        foreach (var hit in hitColliders)
        {
            EnemyHP enemy = hit.GetComponent<EnemyHP>();
            if (enemy != null) enemy.TakeDamage(damage);
        }
    }
}
