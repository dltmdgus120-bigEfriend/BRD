using UnityEngine;
using UnityEngine.AI;
using System.Collections;

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
    public bool isAttacking = false;

    [Header("점사 타겟")]
    public Transform forcedTarget; // 유저가 직접 마우스로 찍어준 점사 타겟
    
    [Header("발사 설정")]
    public Transform firePoint;  //] 투사체가 진짜로 발사될 위치 (지팡이 끝, 가슴팍 등)

    private UnitSkillController skillController;
    private Coroutine attackCoroutine;  //  캔슬을 위해 코루틴을 기억해둘 변수

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
        if (stat != null && stat.data != null && stat.data.isBuilding) return;
        if (skillController != null && skillController.isCasting) return;

        if (anim != null && agent != null)
        {
            // 속도가 0.1보다 크면 걷는 애니메이션 켜기
            bool isMoving = agent.velocity.sqrMagnitude > 0.1f;
            anim.SetBool("IsMoving", isMoving);
        }

        // 쿨타임 타이머는 언제나 최우선으로 돌아가게 맨 위로 올립니다.
        attackTimer += Time.deltaTime;

        // [핵심] 공격 모션(후딜레이) 중에는 AI가 스스로 이동하거나 타겟을 찾지 못하게 뇌를 끕니다!
        if (isAttacking) return;

        // 1. 점사 타겟이 죽었거나 사라졌으면 강제 타겟 해제
        if (forcedTarget != null && !forcedTarget.gameObject.activeInHierarchy)
        {
            forcedTarget = null;
            target = null;
        }

        // 2. 점사 타겟이 살아있을 때의 최우선 행동
        if (forcedTarget != null)
        {
            float distToForced = Vector3.Distance(transform.position, forcedTarget.position);

            if (distToForced > stat.data.attackRange)
            {
                if (agent != null) agent.SetDestination(forcedTarget.position);
                return;
            }
            else
            {
                if (agent != null)
                {
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;
                }
                target = forcedTarget;
            }
        }
        else
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

            if (target == null || Vector3.Distance(transform.position, target.position) > stat.data.attackRange)
            {
                FindTarget();
            }
        }

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
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(AttackRoutine());
    }

    //  공격 모션 동안 발을 묶어두는 코루틴  
    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        if (agent != null)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        if (anim != null) anim.SetTrigger("Attack");

        // 전체 공격 사이클 시간 계산 (예: 공속 1이면 1초, 2면 0.5초)
        float totalAttackTime = 1f / Mathf.Max(0.01f, stat.data.attackSpeed);

        // UnitData에 설정된 개별 비율을 가져와서 계산합니다!
        float windUpTime = totalAttackTime * stat.data.attackWindUpRatio;

        float backswingTime = totalAttackTime * (1f - stat.data.attackWindUpRatio);

        // 1. 칼을 들어올리는 시간 대기 (이때 CancelAttack이 들어오면 데미지 안 나감!)
        yield return new WaitForSeconds(windUpTime);

        // 2. ----------------- 실제 타격 발생 지점 -----------------
        if (stat != null && stat.data != null && stat.data.attackSound != null)
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(stat.data.attackSound);
        }

        if (target != null)
        {
            if (stat.data.projectilePrefab != null)
            {
                //  발사구가 지정되어 있으면 거기서 쏘고, 안 까먹고 안 넣었으면 원래대로 약간 위에서 쏩니다!
                Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f;
                GameObject projGO = Instantiate(stat.data.projectilePrefab, spawnPos, Quaternion.identity);
                Projectile projectile = projGO.GetComponent<Projectile>();

                if (projectile != null)
                {
                    projectile.Setup(target, stat.data.damage, stat.data.attackType, skillController);
                }
            }
            else
            {
                EnemyHP enemyHP = target.GetComponent<EnemyHP>();
                if (enemyHP != null)
                {
                    enemyHP.TakeDamage(stat.data.damage, stat.data.attackType);
                    if (skillController != null) skillController.TryAttackProc(target.position);
                }
            }
        }

        // 3. 자세를 거두는 시간 대기 (이때 명령을 내리면 후딜을 씹고 이동 가능!)
        yield return new WaitForSeconds(backswingTime);

        isAttacking = false;
    }

    // 유저가 무빙샷(평타 캔슬)을 할 수 있도록 도와주는 함수
    private void CancelAttack()
    {
        if (isAttacking)
        {
            isAttacking = false;
            if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        }
    }

    public void OrderAttackMove(Vector3 dest)
    {
        CancelAttack();
        forcedTarget = null;
        isStopped = false;
        isAttackMoving = true;
        target = null;
        if (agent != null) agent.SetDestination(dest);
    }

    public void CommandFocusAttack(Transform enemyTransform)
    {
        CancelAttack();
        forcedTarget = enemyTransform;
        target = enemyTransform;
        isAttackMoving = false;
        isStopped = false;
    }

    public void OrderStop()
    {
        CancelAttack();
        forcedTarget = null;
        isStopped = true;
        isAttackMoving = false;
        target = null;
        if (agent != null) agent.ResetPath();
    }

    public void OrderMove(Vector3 dest)
    {
        CancelAttack();
        forcedTarget = null;
        isStopped = false;
        isAttackMoving = false;
        target = null;
        if (agent != null) agent.SetDestination(dest);
    }

    public void OrderHold()
    {
        CancelAttack();
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