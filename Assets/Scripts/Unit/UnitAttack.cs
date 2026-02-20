using UnityEngine;
using UnityEngine.AI; 

public class UnitAttack : MonoBehaviour
{
    private UnitStat stat;
    private float attackTimer = 0f;
    private Animator anim;    
    private NavMeshAgent agent;

    [Header("상태")]
    public Transform target;
    public bool isAttackMoving = false;
    public bool isStopped = false;

    private UnitSkillController skillController; 

    void Start()
    {
        stat = GetComponentInChildren<UnitStat>();
        anim = GetComponentInChildren<Animator>();       
        agent = GetComponent<NavMeshAgent>();
        skillController = GetComponent<UnitSkillController>();
        UpdateAnimationSpeed();
    }

    void Update()
    {
        // 건물이면 공격 AI 작동 중지
        if (stat != null && stat.data != null && stat.data.isBuilding) return;

        // 이동 애니메이션 처리
        if (anim != null && agent != null)
        {
            // NavMeshAgent가 이동 중인지 확인 (속도가 0.1보다 크면 걷는 중)
            bool isMoving = agent.velocity.sqrMagnitude > 0.1f;
            anim.SetBool("IsMoving", isMoving); 
        }

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
            // 쿨타임 계산 공식 변경: (1 / 공격속도)
            // 예: 공속 2.0 -> 1/2 = 0.5초마다 공격
            // 예: 공속 5.0 -> 1/5 = 0.2초마다 공격
            // (0으로 나누기 방지를 위해 Mathf.Max 사용)
            float cooldown = 1f / Mathf.Max(0.01f, stat.data.attackSpeed);

            if (attackTimer >= cooldown)
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
        attackTimer = 0f;
        if (anim != null) anim.SetTrigger("Attack");

        if (stat != null && stat.data != null && stat.data.attackSound != null)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(stat.data.attackSound);
            }
        }

        if (target != null)
        {
            // 1. 투사체(Projectile)를 쓰는 원거리 유닛
            if (stat.data.projectilePrefab != null)
            {
                Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
                GameObject projGO = Instantiate(stat.data.projectilePrefab, spawnPos, Quaternion.identity);
                Projectile projectile = projGO.GetComponent<Projectile>();

                if (projectile != null)
                {
                    // ★ [수정] Setup 함수에 공격 타입(attackType) 추가 전달
                    projectile.Setup(target, stat.data.damage, stat.data.attackType, skillController);
                }
            }
            // 2. 근접 공격 유닛 (즉발 데미지)
            else
            {
                EnemyHP enemyHP = target.GetComponent<EnemyHP>();
                if (enemyHP != null)
                {
                    // ★ [수정] TakeDamage 함수에 공격 타입 추가 전달
                    enemyHP.TakeDamage(stat.data.damage, stat.data.attackType);

                    if (skillController != null)
                    {
                        skillController.TryAttackProc(target.position);
                    }
                }
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

    // 공속이 바뀌거나(버프), 처음 시작할 때 호출
    public void UpdateAnimationSpeed()
    {
        if (anim == null || stat == null || stat.data == null) return;

        // 공속(APS) 자체가 곧 배속입니다.
        // 공속 1.0 -> 1배속, 공속 2.0 -> 2배속
        anim.SetFloat("AttackSpeedRatio", stat.data.attackSpeed);
    }
}