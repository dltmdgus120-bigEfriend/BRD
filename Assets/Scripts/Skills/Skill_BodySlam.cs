using UnityEngine;

[CreateAssetMenu(menuName = "Trickcal/Skills/Body Slam (AoE)")]
public class Skill_BodySlam : SkillBase
{
    [Header("몸통박치기 전용 설정")]
    public int baseSkillDamage = 50;         // 스킬 자체의 깡딜
    public float damageMultiplier = 1.0f;    // 내 공격력의 몇 배수?

    // 아군이나 맨땅은 무시하고 '적'만 검사하게 만들어 렉을 줄입니다!
    public LayerMask enemyLayer;

    [Header("VFX 설정")]
    public GameObject impactVfxPrefab;       // 쾅! 부딪힐 때 터질 이펙트

    public override void Execute(UnitStat user, Vector3 targetPos = default)
    {
        // 1. 타겟 위치에 타격 이펙트(VFX) 풀링에서 꺼내오기!
        if (impactVfxPrefab != null && PoolManager.Instance != null)
        {
            
            // 메테오 때처럼 범용인 GetProjectile을 사용해 이펙트를 꺼냅니다!
            GameObject vfxObj = PoolManager.Instance.GetProjectile(impactVfxPrefab, targetPos);          
        }

        // 2. 최종 데미지 계산
        int finalDamage = baseSkillDamage + Mathf.RoundToInt(user.currentDamage * damageMultiplier);

        // 3. 범위 데미지 처리
        Collider[] hitColliders = Physics.OverlapSphere(targetPos, effectRadius, enemyLayer);

        int hitCount = 0;

        foreach (var hit in hitColliders)
        {
            EnemyHP enemy = hit.GetComponent<EnemyHP>();
            if (enemy == null) enemy = hit.GetComponentInParent<EnemyHP>();

            if (enemy != null)
            {
                enemy.TakeDamage(finalDamage, attackType);
                hitCount++;
            }
        }

        Debug.Log($"[{user.Name}] 몸통박치기 쾅! 반경 {effectRadius} 내의 적 {hitCount}명에게 {finalDamage} 데미지!");
    }
}
