using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitSkillController : MonoBehaviour
{
    private UnitStat stat;
    private Dictionary<SkillBase, float> cooldowns = new Dictionary<SkillBase, float>();

    public bool isCasting { get; private set; }

    private Animator anim;
    private UnitAttack unitAttack;

    void Start()
    {
        stat = GetComponent<UnitStat>();
        anim = GetComponentInChildren<Animator>();
        unitAttack = GetComponent<UnitAttack>();

        if (stat != null && stat.data.skills != null)
        {
            foreach (var skill in stat.data.skills)
            {
                if (skill == null) continue;

                if (skill.isPassive)
                {
                    skill.OnEquip(stat);                     // 1. 장착 효과 발동 (있는 경우)
                    skill.Execute(stat, transform.position); // 2. 패시브 효과 적용
                }
            }
        }
    }

    void Update()
    {
        // 쿨타임 
        if (cooldowns.Count > 0)
        {
            List<SkillBase> keys = new List<SkillBase>(cooldowns.Keys);
            foreach (var skill in keys)
            {
                if (cooldowns[skill] > 0)
                {
                    cooldowns[skill] -= Time.deltaTime;
                }
            }
        }
    }

    //  UI 버튼을 눌렀을 때 호출되는 함수
    public void OnClickSkillButton(int index)
    {

        if (isCasting)
        {
            Debug.Log("[취소] 현재 스킬 시전 중입니다!");
            return;
        }

        Debug.Log($"[1] 스킬 버튼 신호 수신! 인덱스: {index}"); // 1단계

        if (stat == null || stat.data.skills == null) return;
        if (index >= stat.data.skills.Count)
        {
            Debug.LogError("[오류] 스킬 인덱스 범위 초과!");
            return;
        }

        SkillBase skill = stat.data.skills[index];

        // 패시브거나, 평타 발동(프록) 스킬이면 수동 시전 절대 불가!!
        if (skill.isPassive || skill.isAttackProc)
        {
            Debug.Log($"[거절] {skill.skillName}은(는) 자동 발동 스킬이라 수동으로 쓸 수 없습니다!");
            return;
        }

        Debug.Log($"[2] 스킬 데이터 확인: {skill.skillName}, NeedTarget: {skill.needTarget}"); // 2단계

        // 쿨타임 체크
        if (cooldowns.ContainsKey(skill) && cooldowns[skill] > 0)
        {
            Debug.Log("[취소] 쿨타임 중입니다.");
            return;
        }

        if (skill.needTarget)
        {
            Debug.Log("[3] 조준 모드 요청 보냄 (RTSController로)"); // 3단계
            RTSController rts = FindObjectOfType<RTSController>();
            if (rts != null) rts.EnterSkillMode(index);
            else Debug.LogError("[오류] RTSController를 찾을 수 없습니다!");
        }
        else
        {
            Debug.Log("[3] 즉시 시전 (타겟팅 불필요)");
            UseSkill(index, transform.position);
        }
    }

    // 실제 스킬 실행 함수 (RTSController에서 호출함) 
    public void UseSkill(int index, Vector3 targetPos)
    {
        if (stat == null || stat.data.skills == null || index >= stat.data.skills.Count) return;

        SkillBase skill = stat.data.skills[index];
        if (skill == null) return;

        //  액티브 스킬이면 시전 코루틴 시작!
        if (!skill.isPassive)
        {
            //  코루틴을 부를 때 index(0=Q, 1=W...) 값도 같이 넘겨줍니다!
            StartCoroutine(CastSkillRoutine(index, skill, targetPos));
            cooldowns[skill] = skill.cooldown;
        }
    }

    //스킬 시전 & 행동 제어 코루틴
    private IEnumerator CastSkillRoutine(int skillIndex, SkillBase skill, Vector3 targetPos)
    {
        isCasting = true; // 시전 시작 (발 묶기)

        // 1. 하던 행동(이동, 평타) 즉시 강제 정지!
        if (unitAttack != null) unitAttack.OrderStop();

        Vector3 startPos = transform.position;
        Vector3 finalTargetPos = targetPos; // 진짜 터질 최종 위치

        // 사거리 제한이 0보다 크게 설정되어 있다면?
        if (skill.targetRange > 0)
        {
            float dist = Vector3.Distance(startPos, targetPos);
            // 마우스로 찍은 곳이 내 사거리보다 멀면?
            if (dist > skill.targetRange)
            {
                // 찍은 방향으로 사거리(targetRange) 끝까지만 목표 지점을 잘라냅니다!
                Vector3 dir = (targetPos - startPos).normalized;
                finalTargetPos = startPos + dir * skill.targetRange;
            }
        }

        // 2, 애니메이션 실행 전, 몇 번째 스킬인지 Int 값을 먼저 꽂아줍니다!
        if (anim != null)
        {
            anim.SetInteger("SkillIndex", skillIndex); // 0=Q, 1=W, 2=E, 3=R

            if (!string.IsNullOrEmpty(skill.animTriggerName))
            {
                anim.SetTrigger(skill.animTriggerName); // 트리거 발사! (기본값 "Skill")
            }
        }

        // 시전 사운드 재생 (유닛의 기합이나 대사!)
        if (skill.castVoice != null && SoundManager.Instance != null)
        {
            // 만약 캐릭터 목소리 전용 함수(PlayVoice 등)가 있다면 그걸 쓰셔도 좋습니다.
            SoundManager.Instance.PlaySFX(skill.castVoice);
        }

        //  3. 돌진(Dash) OR 대기 처리
        if (skill.isDashSkill && skill.castTime > 0)
        {
            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            if (agent != null) agent.enabled = false;

            float timer = 0f;
            while (timer < skill.castTime)
            {
                timer += Time.deltaTime;
                float percent = timer / skill.castTime;

                transform.position = Vector3.Lerp(startPos, finalTargetPos, percent);
                yield return null;
            }

            transform.position = finalTargetPos;
            if (agent != null) agent.enabled = true;
        }
        else if (skill.castTime > 0)
        {
            yield return new WaitForSeconds(skill.castTime);
        }

        isCasting = false; // 발 묶기 해제

        // 하늘에 빵 소환!
        skill.OnCastStart(stat, targetPos);

        // 빵이 떨어지는 시간 대기
        if (skill.actionDelay > 0)
        {
            yield return new WaitForSeconds(skill.actionDelay);
        }

        // 임팩트 사운드 재생 (빵이 바닥에 닿아 쾅! 터지는 소리)
        if (skill.skillSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(skill.skillSound);
        }

        // 폭발 및 데미지!
        skill.Execute(stat, finalTargetPos);

        // 스킬이 끝났으니 뇌를 다시 켜고 자동 공격(Hold) 상태로 복귀
        if (unitAttack != null) unitAttack.OrderHold();
    }

    // (UI 갱신용) 쿨타임 비율 반환
    public float GetCooldownRatio(int index)
    {
        if (stat == null || stat.data.skills == null || index >= stat.data.skills.Count) return 0f;
        SkillBase skill = stat.data.skills[index];

        if (cooldowns.ContainsKey(skill) && skill.cooldown > 0)
        {
            return cooldowns[skill] / skill.cooldown;
        }
        return 0f;
    }

    
    public bool TryAttackProc(Vector3 targetPos)
    {
        if (stat == null || stat.data.skills == null) return false;

        for (int i = 0; i < stat.data.skills.Count; i++)
        {
            SkillBase skill = stat.data.skills[i];

            if (skill == null || !skill.isAttackProc) continue;
            if (cooldowns.ContainsKey(skill) && cooldowns[skill] > 0) continue;

            if (Random.value <= (skill.procChance / 100f))
            {
                Debug.Log($" {skill.skillName} 발동! (확률: {skill.procChance}%)");

                skill.Execute(stat, targetPos);
                cooldowns[skill] = skill.cooldown;

                return true; //  프록이 성공적으로 터졌다고 알려줌!
            }
        }
        return false; // 안 터짐
    }
}