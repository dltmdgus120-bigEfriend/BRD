using UnityEngine;

public class Projectile : MonoBehaviour
{
    private UnitSkillController ownerSkill;
    public float speed = 15f; // 날아가는 속도

    private Transform target;
    private int damage;

    public void Setup(Transform _target, int _damage, UnitSkillController _owner)
    {
        target = _target;
        damage = _damage;
        ownerSkill = _owner; // 주인 저장
    }

    void Update()
    {
        // 1. 날아가는 도중에 적이 이미 죽어서 없어졌다면? -> 나도 파괴
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // 2. 적을 향한 방향과 이번 프레임에 이동할 거리 계산
        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        // 3. 목표물에 닿았는지 확인 (남은 거리가 이번 이동 거리보다 짧으면 닿은 것)
        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        // 4. 목표물을 향해 이동
        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
    }

    void HitTarget()
    {
        EnemyHP enemy = target.GetComponent<EnemyHP>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);

            // ★ [핵심] 적을 맞춘 이 시점에 주인의 스킬 발동을 시도!
            if (ownerSkill != null)
            {
                // 투사체 위치(transform.position)나 적 위치(target.position)를 넘겨줌
                ownerSkill.TryAttackProc(transform.position);
            }
        }

        Destroy(gameObject);
    }
}