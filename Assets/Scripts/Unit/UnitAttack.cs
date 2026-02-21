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

    [Header("점사 타겟")]
    public Transform forcedTarget; // 유저가 직접 마우스로 찍어준 점사 타겟

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

        //  스킬 시전 중이면 이동/평타 싹 다 금지하고 바로 종료!
        if (skillController != null && skillController.isCasting) return;

        if (stat != null && stat.data != null && stat.data.isBuilding) return;

        if (anim != null && agent != null)
        {
            bool isMoving = agent.velocity.sqrMagnitude > 0.1f;
            anim.SetBool("IsMoving", isMoving);
        }

        if (isStopped) return;

        attackTimer += Time.deltaTime;

        // 1. 점사 타겟(보스 등)이 죽었거나 사라졌으면 강제 타겟 해제
        if (forcedTarget != null && !forcedTarget.gameObject.activeInHierarchy)
        {
            forcedTarget = null;
            target = null;
        }

        // 2. 점사 타겟이 살아있을 때의 최우선 행동!
        if (forcedTarget != null)
        {
            float distToForced = Vector3.Distance(transform.position, forcedTarget.position);

            // 사거리 밖이면 다른 놈 무시하고 무조건 쫓아감
            if (distToForced > stat.data.attackRange)
            {
                if (agent != null) agent.SetDestination(forcedTarget.position);
                return; // 쫓아가는 중에는 쏘지 않음
            }
            else
            {
                // 사거리 안에 들어오면 발을 멈추고 쏘기 시작!
                if (agent != null)
                {
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;
                }
                target = forcedTarget; // 타겟을 점사 대상으로 꽉 고정!
            }
        }
        else // 점사 타겟이 없을 때만 기존 자동 공격 모드 작동
        {
            if (isAttackMoving)
            {
                FindTarget();
                if (target != null)
                {
                    if (agent != null) agent.ResetPath();
                    isAttackMoving = false;
                }
            }

            // 타겟이 없거나 사거리 밖이면 새 타겟 찾기
            if (target == null || Vector3.Distance(transform.position, target.position) > stat.data.attackRange)
            {
                FindTarget();
            }
        }

        // 걷는 중이면 공격 금지 (단, 위에서 사거리 내에 들어와 멈췄다면 여길 통과함)
        if (agent != null && agent.velocity.sqrMagnitude > 0.1f) return;

        // 타겟이 있고 쿨타임이 찼으면 공격!
        if (target != null)
        {
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
        forcedTarget = null; // 점사 기억 리셋
        isStopped = false;
        isAttackMoving = true;
        target = null;
        if (agent != null) agent.SetDestination(dest);
    }

    public void CommandFocusAttack(Transform enemyTransform)
    {
        forcedTarget = enemyTransform;
        target = enemyTransform;
        isAttackMoving = false;
        isStopped = false; // 혹시 홀드 상태였어도 풀고 때리러 감
    }

    public void OrderStop()
    {
        forcedTarget = null; 
        isStopped = true;
        isAttackMoving = false;
        target = null;
        if (agent != null) agent.ResetPath();
    }

    public void OrderMove(Vector3 dest)
    {
        forcedTarget = null; 
        isStopped = false;
        isAttackMoving = false;
        target = null;
        if (agent != null) agent.SetDestination(dest);
    }

    public void OrderHold()
    {
        forcedTarget = null; 
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