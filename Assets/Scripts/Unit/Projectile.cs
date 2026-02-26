using UnityEngine;

public class Projectile : MonoBehaviour
{
    private UnitSkillController ownerSkill;
    public float speed = 15f; // 날아가는 속도

    private Transform target;
    private int damage;
    private AttackType attackType;

    private Camera mainCam;

    public void Setup(Transform _target, int _damage, AttackType _type, UnitSkillController _owner)
    {
        target = _target;
        damage = _damage;
        attackType = _type; 
        ownerSkill = _owner;

        mainCam = Camera.main;

        // 혹시 투사체에 꼬리(Trail Renderer)가 있다면, 이전 잔상을 지워줍니다.
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail != null) trail.Clear();
    }

    void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            PoolManager.Instance.ReturnProjectile(gameObject);
            return;
        }

        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        // 2D 스프라이트 전용: 빌보드 + 시계바늘 회전 로직!
        if (dir != Vector3.zero && mainCam != null)
        {
            // 1. 유닛(종이)이 카메라를 정면으로 쳐다보게 똑바로 세웁니다. (빌보드)
            transform.forward = mainCam.transform.forward;

            // 2. 카메라 시점을 기준으로, 타겟이 내 상하좌우 어디에 있는지 계산합니다.
            Vector3 localDir = transform.InverseTransformDirection(dir);

            // 3. 그 방향을 향해 시계바늘처럼 Z축만 돌려줍니다!
            float angle = Mathf.Atan2(localDir.y, localDir.x) * Mathf.Rad2Deg;
            transform.Rotate(0, 0, angle);
        }

        // 2D 투사체는 꼭 우측을 향하게 프리펩을 설정해줄것! 그래야 이쁘게 나감.

        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
    }

    void HitTarget()
    {
        EnemyHP enemy = target.GetComponent<EnemyHP>();
        if (enemy != null)
        {
            //  프록(강타)이 터졌는지 먼저 물어봅니다!
            bool isProc = false;
            if (ownerSkill != null)
            {
                isProc = ownerSkill.TryAttackProc(transform.position);
            }

            // ★프록이 안 터졌을 때만 일반 평타 데미지를 줍니다!
            // (프록이 터졌다면 평타 데미지는 캔슬되고 강타 스킬이 대신 데미지와 팝업을 발생시킴)
            if (!isProc)
            {
                enemy.TakeDamage(damage, attackType);
            }
        }

        PoolManager.Instance.ReturnProjectile(gameObject);
    }
}