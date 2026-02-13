using UnityEngine;
using UnityEngine.AI; // ★ NavMeshAgent를 쓰기 위해 반드시 필요!

public class UnitAttack : MonoBehaviour
{
    private UnitStat stat;
    private float attackTimer = 0f;
    private Animator anim;

    // ★ 추가됨: 내 유닛의 이동 상태를 확인할 변수
    private NavMeshAgent agent;

    [Header("상태")]
    public Transform target;
    public bool isAttackMoving = false;
    public bool isStopped = false;

    void Start()
    {
        stat = GetComponent<UnitStat>();
        anim = GetComponentInChildren<Animator>();

        // ★ 내 몸에 붙어있는 네비게이션(이동) 컴포넌트 가져오기
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    { 
        //완전 정지 상태면 아무것도 안 하고 함수 종료
        if (isStopped) return;
       

        // 쿨타임은 이동 중에도 흘러가게 둡니다. (도착하자마자 바로 쏠 수 있도록!)
        attackTimer += Time.deltaTime;


        if (isAttackMoving)
        {
            // 이동하면서 적을 계속 찾음
            FindTarget();

            // 적을 찾았다면?
            if (target != null)
            {
                // 이동 멈추고 공격 모드로 전환!
                agent.ResetPath();
                isAttackMoving = false;
            }
        }

       
        // velocity(현재 속도)가 조금이라도 있으면 걷고 있는 상태로 판단
        if (agent != null && agent.velocity.sqrMagnitude > 0.1f)
        {
            // 걷는 중이면 타겟 찾기와 공격을 전부 건너뛰고 바로 함수 종료!
            return;
        }

        // --- 여기서부터는 제자리에 서 있을 때만 실행됨 ---

        // 타겟이 없거나, 타겟이 죽었거나, 사거리 밖으로 나갔으면 -> 새 타겟 찾기
        if (target == null || Vector3.Distance(transform.position, target.position) > stat.data.attackRange)
        {
            FindTarget();
        }

        // 타겟이 있으면 공격
        if (target != null)
        {
            if (attackTimer >= stat.data.attackSpeed)
            {
                Attack();
            }
        }
    }

    void FindTarget()
    {
        EnemyHP[] enemies = FindObjectsOfType<EnemyHP>();
        float shortestDistance = Mathf.Infinity;
        EnemyHP nearestEnemy = null;

        foreach (var enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (distance <= stat.data.attackRange && distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null)
        {
            target = nearestEnemy.transform;
        }
        else
        {
            target = null;
        }
    }

    void Attack()
    {
        attackTimer = 0f; // 쿨타임 초기화

        // 1. 애니메이션 재생
        if (anim != null) anim.SetTrigger("Attack");

        // 2. 투사체 발사
        if (target != null)
        {
            // 데이터에 투사체가 등록되어 있는지 확인
            if (stat.data.projectilePrefab != null)
            {
                Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
                GameObject projGO = Instantiate(stat.data.projectilePrefab, spawnPos, Quaternion.identity);
                Projectile projectile = projGO.GetComponent<Projectile>();

                if (projectile != null)
                {
                    projectile.Setup(target, stat.data.damage);
                }
            }
            else
            {
                // 투사체가 없으면 근접 공격 (즉시 데미지)
                EnemyHP enemyHP = target.GetComponent<EnemyHP>();
                if (enemyHP != null) enemyHP.TakeDamage(stat.data.damage);
            }
        }
    }

    public void OrderAttackMove(Vector3 dest)
    {
        isStopped = false;      
        isAttackMoving = true;
        target = null;
        if (agent != null) agent.SetDestination(dest);
    }

    public void OrderStop()
    {
        isStopped = true;       // 돌부처 모드 
        isAttackMoving = false; // 어택땅 끄기
        target = null;          // 타겟 잊기
        if (agent != null) agent.ResetPath(); // 발 멈추기
    }

    public void OrderMove(Vector3 dest)
    {
        isStopped = false;     
        isAttackMoving = false;
        target = null;
        if (agent != null) agent.SetDestination(dest);
    }

    // 홀드 명령 (H키) - 제자리 사수하지만 공격은 함
    public void OrderHold()
    {
        isStopped = false;     
        isAttackMoving = false;
        if (agent != null)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
    }



    void OnDrawGizmosSelected()
    {
        if (GetComponent<UnitStat>() != null && GetComponent<UnitStat>().data != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, GetComponent<UnitStat>().data.attackRange);
        }
    }
}