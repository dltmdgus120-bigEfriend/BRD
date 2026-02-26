using UnityEngine;

[CreateAssetMenu(menuName = "Trickcal/Skills/Smite (Proc)")]
public class Skill_Smite : SkillBase
{
    [Header("강타 설정")]
    public int bonusDamage = 50;
    public GameObject hitVFX;

    public LayerMask enemyLayer;

    public override void Execute(UnitStat user, Vector3 targetPos)
    {
        if (skillSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(skillSound);
        }

        if (hitVFX != null && PoolManager.Instance != null)
        {
            PoolManager.Instance.GetProjectile(hitVFX, targetPos);
        }

        // 몬스터의 배꼽 높이를 찌르도록 Y축을 1m 올려줍니다!
        Vector3 checkPos = targetPos + Vector3.up * 1.0f;
      
        Collider[] hits = Physics.OverlapSphere(checkPos, 1.5f, enemyLayer);

        bool isHitSuccess = false; // 진짜로 때렸는지 확인용

        foreach (var hit in hits)
        {
            EnemyHP enemy = hit.GetComponent<EnemyHP>();
            if (enemy == null) enemy = hit.GetComponentInParent<EnemyHP>();

            // 평타에 맞아 이미 죽은(HP가 0인) 시체는 때리지 않고 넘어갑니다!
            if (enemy != null && enemy.currentHP > 0)
            {
                int finalDamage = enemy.TakeDamage(bonusDamage, attackType);

                if (PoolManager.Instance != null)
                {
                    // 머리 위로 아주 예쁘게 띄워줍니다.
                    Vector3 popupPos = enemy.transform.position + new Vector3(0f, 1.5f, 0f);
                    PoolManager.Instance.ShowDamagePopup(popupPos, finalDamage, attackType);
                }

                isHitSuccess = true;
                break; // 한 놈 때렸으니 종료
            }
        }

        // 평타에 몬스터가 죽어버려서 강타가 들어갈 자리가 없었다면 디버그를 띄워줍니다.
        if (!isHitSuccess)
        {
            Debug.Log($"[강타 빗나감] 평타에 이미 적이 죽었거나 범위를 벗어났습니다!");
        }
    }
}