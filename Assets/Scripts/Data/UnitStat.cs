using UnityEngine;

public class UnitStat : MonoBehaviour
{
    public UnitData data; // 내 정보가 담긴 데이터 원본 (틀)

    // 편의를 위해 정보를 바로 꺼낼 수 있게 해둠
    public int Rank => data.rank;
    public string Name => data.unitName;

    //  인게임에서 패시브/버프를 받아 실제로 변할 내 '개인 스탯'
    public int currentDamage;
    public float currentAttackSpeed;

    [Header("시각 요소 연결")]
    public Animator anim; // 껍데기 프리팹의 애니메이터 연결용



    // 풀에서 꺼낼 때(또는 조합될 때) 매니저가 호출해 줄 초기화 함수
    public void InitAlly(UnitData newData)
    {
        data = newData;
        currentDamage = data.damage;
        currentAttackSpeed = data.attackSpeed;

        // ★ AC 덮어씌우기
        if (anim != null && data.animController != null)
        {
            anim.runtimeAnimatorController = data.animController;

            // 덮어씌운 직후 애니메이터를 강제로 새로고침(Rebind) 시켜서 즉시 작동하게 만듭니다!
            anim.Rebind();
            anim.Update(0f);
        }

        transform.localScale = new Vector3(data.unitSize, data.unitSize, 1f);

        UnitAttack attackScript = GetComponent<UnitAttack>();
        if (attackScript != null)
        {
            attackScript.UpdateAnimationSpeed();
        }
    }
}