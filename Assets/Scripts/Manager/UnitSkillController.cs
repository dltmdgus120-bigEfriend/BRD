using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                if (skill.isPassive) skill.Execute(stat, transform.position);
            }
        }
    }

    void Update()
    {
        // 쿨타임 감소
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
            StartCoroutine(CastSkillRoutine(skill, targetPos));
            cooldowns[skill] = skill.cooldown; // 쿨타임 시작
        }
    }

    //스킬 시전 & 행동 제어 코루틴
    private IEnumerator CastSkillRoutine(SkillBase skill, Vector3 targetPos)
    {
        isCasting = true; // 시전 시작 (발 묶기)

        // 1. 하던 행동(이동, 평타) 즉시 강제 정지!
        if (unitAttack != null) unitAttack.OrderStop();

        // 2. 애니메이션 실행
        if (anim != null && !string.IsNullOrEmpty(skill.animTriggerName))
        {
            anim.SetTrigger(skill.animTriggerName);
        }

        // 3. 사운드 재생
        if (skill.skillSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(skill.skillSound);
        }

        // 시전 시작 효과 발동! (여기서 메테오가 하늘에 나타남)
        skill.OnCastStart(stat, targetPos);

        // 시전 시간(캐스팅 타임) 동안 대기 (메테오가 떨어지는 중...)
        yield return new WaitForSeconds(skill.castTime);

        // 기다린 후 최종 효과 발동! (여기서 쾅! 폭발하고 데미지 들어감)
        skill.Execute(stat, targetPos);

        isCasting = false; // 시전 종료 (속박 해제!)
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

    public void TryAttackProc(Vector3 targetPos)
    {
        if (stat == null || stat.data.skills == null) return;

        // 유닛이 가진 모든 스킬을 검사
        for (int i = 0; i < stat.data.skills.Count; i++)
        {
            SkillBase skill = stat.data.skills[i];

            // 1. "평타 발동" 스킬이 아니면 패스
            if (skill == null || !skill.isAttackProc) continue;

            // 2. 쿨타임 중이면 패스
            if (cooldowns.ContainsKey(skill) && cooldowns[skill] > 0) continue;

            // 3. 확률 계산 (주사위 굴리기)
            // Random.value는 0.0 ~ 1.0 사이의 랜덤 값 (예: 0.15)
            // procChance가 20이면 -> 20/100 = 0.2보다 작으면 당첨!
            if (Random.value <= (skill.procChance / 100f))
            {
                // 당첨! 스킬 발사
                Debug.Log($" {skill.skillName} 발동! (확률: {skill.procChance}%)");

                // 실행
                skill.Execute(stat, targetPos);

                // 쿨타임 적용
                cooldowns[skill] = skill.cooldown;
            }
        }
    }
}