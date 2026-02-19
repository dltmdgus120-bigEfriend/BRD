using UnityEngine;

[CreateAssetMenu(menuName = "Trickcal/Skills/Guaranteed Summon")]
public class Skill_GuaranteedSummon : SkillBase
{
    [Header("확정 뽑기 설정")]
    public int costElif = 5; // 이 스킬을 쓰기 위해 필요한 엘리프

    public override void Execute(UnitStat user, Vector3 targetPos = default)
    {
        // 주의: DefenseManager 부분은 현재 쓰시는 재화 관리 스크립트 이름으로 맞춰주세요!
        // (예: GameManager.Instance.currentElif 등)

        if (DefenseManager.Instance != null)
        {
            // 1. 엘리프가 충분한지 검사
            if (DefenseManager.Instance.elif >= costElif)
            {
                // 2. 엘리프 차감
                DefenseManager.Instance.elif -= costElif;
                // (필요시 UI 갱신 함수 호출: DefenseManager.Instance.UpdateCurrencyUI(); )

                // 3. 소환 매니저를 통해 3성 확정 소환!
                if (SummonManager.Instance != null)
                {
                    SummonManager.Instance.SpawnGuaranteed3Star();

                    // 성공 로그 (시스템)
                    LogManager.Instance.ShowLog($"엘리프 {costElif}개를 소모하여 확정 소환 성공!", LogType.System);
                }
            }
            else
            {
                // 4. 엘리프 부족 알림
                if (LogManager.Instance != null)
                {
                    LogManager.Instance.ShowLog($"엘리프가 부족합니다! (필요: {costElif})", LogType.System);
                }
            }
        }
    }
}