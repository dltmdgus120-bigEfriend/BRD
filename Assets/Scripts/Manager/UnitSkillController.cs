using UnityEngine;
using System.Collections.Generic;

public class UnitSkillController : MonoBehaviour
{
    private UnitStat stat;
    private Dictionary<SkillBase, float> cooldowns = new Dictionary<SkillBase, float>();

    void Start()
    {
        stat = GetComponent<UnitStat>();

        // 시작할 때 패시브 스킬 자동 발동
        if (stat != null && stat.data.skills != null)
        {
            foreach (var skill in stat.data.skills)
            {
                if (skill.isPassive)
                {
                    skill.Execute(stat, transform.position);
                }
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

    // ★ [1] UI 버튼을 눌렀을 때 호출되는 함수
    public void OnClickSkillButton(int index)
    {
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

    // ★ [2] 실제 스킬 실행 함수 (RTSController에서 호출함) - 이 함수가 없어서 에러가 났던 겁니다!
    public void UseSkill(int index, Vector3 targetPos)
    {
        if (stat == null || stat.data.skills == null || index >= stat.data.skills.Count) return;

        SkillBase skill = stat.data.skills[index];
        if (skill == null) return;

        // 실행!
        skill.Execute(stat, targetPos);

        // 쿨타임 시작
        if (!skill.isPassive)
        {
            cooldowns[skill] = skill.cooldown;
        }
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