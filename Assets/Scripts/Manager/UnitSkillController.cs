using UnityEngine;
using System.Collections.Generic;

public class UnitSkillController : MonoBehaviour
{
    // 현재 남은 쿨타임 저장소 (스킬: 남은시간)
    private Dictionary<SkillBase, float> cooldowns = new Dictionary<SkillBase, float>();

    private UnitStat stat;

    void Start()
    {
        stat = GetComponent<UnitStat>();

        // 데이터에 있는 스킬들을 가져와서 쿨타임 초기화
        if (stat.data != null && stat.data.skills != null)
        {
            foreach (var skill in stat.data.skills)
            {
                if (skill == null) continue;

                // 쿨타임 0으로 시작
                cooldowns[skill] = 0f;

                // 패시브라면 시작하자마자 효과 적용 (예: 공격력 증가)
                if (skill.isPassive)
                {
                    skill.OnEquip(stat);
                }
            }
        }
    }

    void Update()
    {
        // 쿨타임 줄이기
        // (Dictionary를 돌면서 값을 수정하기 위해 키를 리스트로 복사해서 씀)
        var keys = new List<SkillBase>(cooldowns.Keys);
        foreach (var skill in keys)
        {
            if (cooldowns[skill] > 0)
            {
                cooldowns[skill] -= Time.deltaTime;
                if (cooldowns[skill] < 0) cooldowns[skill] = 0;
            }
        }
    }

    // 스킬 사용 시도 (외부에서 호출)
    public void TryUseSkill(int index)
    {
        // 인덱스 범위 확인
        if (stat.data.skills == null || index >= stat.data.skills.Count) return;

        SkillBase skill = stat.data.skills[index];
        if (skill == null) return;

        // 패시브는 사용 불가
        if (skill.isPassive) return;

        // 쿨타임 확인
        if (cooldowns.ContainsKey(skill) && cooldowns[skill] <= 0)
        {
            // 스킬 발동!
            skill.Execute(stat);

            // 쿨타임 적용
            cooldowns[skill] = skill.cooldown;

            // (옵션) 스킬 사용 소리 재생 등
            Debug.Log($"{skill.skillName} 사용!");
        }
        else
        {
            Debug.Log("쿨타임 중입니다.");
        }
    }

    // UI에서 쿨타임 비율(0~1)을 가져가기 위한 함수
    public float GetCooldownRatio(int index)
    {
        if (stat.data == null || stat.data.skills == null || index >= stat.data.skills.Count) return 0;

        SkillBase skill = stat.data.skills[index];
        if (skill == null || skill.isPassive || skill.cooldown == 0) return 0;

        if (cooldowns.ContainsKey(skill))
        {
            return cooldowns[skill] / skill.cooldown;
        }
        return 0;
    }
}