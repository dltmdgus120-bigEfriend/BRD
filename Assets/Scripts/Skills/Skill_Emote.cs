using UnityEngine;

[CreateAssetMenu(menuName = "Trickcal/Skills/Emote (Animation Only)")]
public class Skill_Emote : SkillBase
{
    // Execute는 스킬의 '결과'를 처리하는 곳인데, 
    // 감정표현은 적을 때리거나 버프를 주는 결과가 없으니 그냥 비워둡니다!
    public override void Execute(UnitStat user, Vector3 targetPos = default)
    {
        // 짠! 아무 일도 일어나지 않습니다.
        // (애니메이션과 소리는 이미 UnitSkillController에서 다 틀어줬기 때문이죠!)

        Debug.Log($"[{user.Name}] 감정표현({skillName}) 완료!");
    }
}