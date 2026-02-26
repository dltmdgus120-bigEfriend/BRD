using UnityEngine;

[CreateAssetMenu(menuName = "Trickcal/Skills/Meteor Attack")]
public class Skill_Meteor : SkillBase
{
    [Header("메테오 설정")]
    public int damage = 100;          
                                    
    public LayerMask enemyLayer;  //아군이나 맨땅은 무시하고 '적'만 검사하게 만들어 렉을 줄입니다!

    [Header("VFX 설정")]
    public GameObject fallingVFX;   //  하늘에서 떨어지는 메테오 프리팹
    public float dropHeight = 15f;  // 얼마나 높은 곳에서 떨어질지
    public GameObject explosionVFX; // 기존 폭발 이펙트

    

    public override void OnCastStart(UnitStat user, Vector3 targetPos)
    {
        if (fallingVFX == null) return;

        // 하늘 좌표 계산
        Vector3 skyPos = targetPos + Vector3.up * dropHeight;
        Quaternion lookDown = Quaternion.LookRotation(Vector3.down);

        // 빵 소환
        GameObject fallingObj = PoolManager.Instance.GetProjectile(fallingVFX, skyPos);
        fallingObj.transform.rotation = lookDown; // 방향 설정

        // 빵에 달려있는 스크립트를 찾아서 이동 명령 내리기
        FallingMeteor fallingScript = fallingObj.GetComponent<FallingMeteor>();

        if (fallingScript != null)
        {           
            fallingScript.Setup(skyPos, targetPos, actionDelay);
        }
        else
        {
            Debug.LogError("빵 프리팹에 FallingMeteor 스크립트가 안 붙어있습니다!");
        }
    }

    public override void Execute(UnitStat user, Vector3 targetPos)
    {
        // (사운드 재생 로직 삭제: 액티브 스킬의 경우에는 애니메이션에 사운드 포함됨 )

        // 폭발 이펙트도 풀 매니저에서 꺼내오기!
        if (explosionVFX != null)
        {
            PoolManager.Instance.GetProjectile(explosionVFX, targetPos);
        }     

        // 2. 범위 데미지 처리
        // 최적화: effectRadius 반경 안에 있는 오브젝트 중, 오직 enemyLayer를 가진 놈들만 솎아냅니다.
        Collider[] hitColliders = Physics.OverlapSphere(targetPos, effectRadius, enemyLayer);

        foreach (var hit in hitColliders)
        {
            // 적 확인 (혹시 자식 오브젝트의 콜라이더를 맞췄을 경우를 대비해 InParent도 체크)
            EnemyHP enemy = hit.GetComponent<EnemyHP>();
            if (enemy == null) enemy = hit.GetComponentInParent<EnemyHP>();

            if (enemy != null)
            {
                // SkillBase에 미리 설정해둔 공격 타입(attackType)을 그대로 적용
                enemy.TakeDamage(damage, attackType);
            }
        }
    }
}
