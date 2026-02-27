using UnityEngine;
using System.Collections; 

[CreateAssetMenu(menuName = "Trickcal/Skills/Triple Shot (Active)")]
public class Skill_TripleShot : SkillBase
{
    [Header("3연사 설정")]
    public int damagePerShot = 30;       
    public float timeBetweenShots = 0.15f; 
    public GameObject hitVFX;              
    public LayerMask enemyLayer;           

    // 시전 시간이 끝나고 호출되는 메인 실행 함수
    public override void Execute(UnitStat user, Vector3 targetPos)
    {
        // 1. 마우스로 찍은 위치 주변(기본 1.5m)에서 타겟을 찾습니다.
        float searchRadius = effectRadius > 0 ? effectRadius : 1.5f;
        Collider[] hits = Physics.OverlapSphere(targetPos, searchRadius, enemyLayer);

        EnemyHP targetEnemy = null;

        // 2. 가장 먼저 잡힌 적 1명만 조준합니다! (단일 타겟 3연사)
        foreach (var hit in hits)
        {
            EnemyHP enemy = hit.GetComponent<EnemyHP>();
            if (enemy == null) enemy = hit.GetComponentInParent<EnemyHP>();

            if (enemy != null && enemy.currentHP > 0)
            {
                targetEnemy = enemy;
                break;
            }
        }

        // 3. 적을 찾았다면 타다당! 사격 개시
        if (targetEnemy != null)
        {
            // 스킬 데이터(SO)는 코루틴을 못 쓰므로, 유닛(user)에게 3연사 코루틴을 대신 실행시킵니다!
            user.StartCoroutine(FireBurst(targetEnemy, targetPos));
        }
        else
        {
            Debug.Log("[3연사 스킬] 범위 내에 살아있는 적이 없습니다!");
        }
    }

    // 실제 3연사를 처리하는 코루틴
    private IEnumerator FireBurst(EnemyHP targetEnemy, Vector3 targetPos)
    {
        for (int i = 0; i < 3; i++)
        {
            // [핵심] 쏘는 도중에 적이 죽었으면 즉시 사격 중지! (허공에 쏘는 낭비 방지)
            if (targetEnemy == null || targetEnemy.currentHP <= 0) break;

            // 1. 사운드 재생 (탕!)
            if (skillSound != null && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(skillSound);
            }

            // 2. 이펙트 생성 (투사체 없이 적 몸통에서 바로 터짐!)
            if (hitVFX != null && PoolManager.Instance != null)
            {
                // 배꼽 높이(Y+1.0f) 쯤에서 이펙트가 터지게 조정
                Vector3 vfxPos = targetEnemy.transform.position + Vector3.up * 1.0f;
                PoolManager.Instance.GetProjectile(hitVFX, vfxPos);
            }

            // 3. 데미지 적용 및 데미지 팝업 띄우기 (전에 만든 팝업 로직 100% 재활용!)
            int finalDamage = targetEnemy.TakeDamage(damagePerShot, attackType);
            if (PoolManager.Instance != null)
            {
                Vector3 popupPos = targetEnemy.transform.position + new Vector3(0f, 1.5f, 0f);
                PoolManager.Instance.ShowDamagePopup(popupPos, finalDamage, attackType);
            }

            // 4. 다음 총알 발사까지 아주 잠깐 대기
            yield return new WaitForSeconds(timeBetweenShots);
        }
    }
}