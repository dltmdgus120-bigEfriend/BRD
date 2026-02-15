using UnityEngine;

[CreateAssetMenu(menuName = "Trickcal/Skills/Meteor Attack")]
public class Skill_Meteor : SkillBase
{
    [Header("메테오 설정")]
    public int damage = 100;      // 폭발 데미지
    public float radius = 3.0f;   // 폭발 범위 (반경)
    public GameObject explosionVFX; // 폭발 이펙트 프리팹 (파티클)

    public override void Execute(UnitStat user)
    {
        // 1. 공격 담당 컴포넌트를 가져와서 "지금 누구 때리고 있니?" 물어봄
        UnitAttack attack = user.GetComponent<UnitAttack>();

        // 타겟이 없거나 사거리 밖이면 스킬 실패 (쿨타임은 돌지 않게 처리 필요하지만, 일단 실행)
        if (attack == null || attack.target == null)
        {
            Debug.Log("스킬 실패: 타겟이 없습니다.");
            return;
        }

        Vector3 targetPos = attack.target.position;

        // 2. 이펙트 소환 (펑!)
        if (explosionVFX != null)
        {
            Instantiate(explosionVFX, targetPos, Quaternion.identity);
        }

        // 3. 광역 데미지 판정 (OverlapSphere)
        // 타겟 위치를 중심으로 공을 그려서, 그 안에 닿은 적들을 다 찾음
        Collider[] hitColliders = Physics.OverlapSphere(targetPos, radius);

        int hitCount = 0;
        foreach (var hit in hitColliders)
        {
            // 적 태그나 컴포넌트 확인 (EnemyHP가 있다고 가정)
            EnemyHP enemy = hit.GetComponent<EnemyHP>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                hitCount++;
            }
        }

        Debug.Log($"메테오 발동! {hitCount}명에게 {damage} 데미지!");
    }
}
