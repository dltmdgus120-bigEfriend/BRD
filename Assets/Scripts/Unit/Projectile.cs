using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f; // 날아가는 속도

    private Transform target;
    private int damage;

    // 대포(UnitAttack)가 발사할 때 목표물과 데미지를 전달해주는 함수
    public void Setup(Transform _target, int _damage)
    {
        target = _target;
        damage = _damage;
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
        // 적에게 데미지를 주고 투사체는 파괴
        EnemyHP enemy = target.GetComponent<EnemyHP>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        // (선택) 여기에 펑! 하는 파티클 효과를 넣어도 좋습니다.
        Destroy(gameObject);
    }
}