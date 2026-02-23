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

        // 1. 이펙트 생성
        if (hitVFX != null)
        {
            Instantiate(hitVFX, targetPos, Quaternion.identity);
        }

        // 2. 데미지 처리
        // targetPos는 '투사체가 맞은 위치' 혹은 '적의 위치'입니다.
        // 아주 작은 범위(0.5f)로 체크해서 그 자리에 있는 적을 찾아냅니다.
        Collider[] hits = Physics.OverlapSphere(targetPos, 0.5f, enemyLayer);

        foreach (var hit in hits)
        {
            // 적 껍데기(자식 콜라이더)를 맞췄을 수도 있으니 꼼꼼하게 검사
            EnemyHP enemy = hit.GetComponent<EnemyHP>();
            if (enemy == null) enemy = hit.GetComponentInParent<EnemyHP>();

            if (enemy != null)
            {
                enemy.TakeDamage(bonusDamage, attackType);

                // 단일 추가타니까 한 놈만 때리고 쿨하게 뒤돌아섭니다.
                break;
            }
        }
    }
}