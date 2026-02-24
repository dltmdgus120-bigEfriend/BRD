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

    void Awake()
    {
        // 맵에 스폰되는 순간, 원본(data)에 적힌 기본 스탯을 내 개인 스탯으로 복사해옵니다!
        if (data != null)
        {
            currentDamage = data.damage;
            currentAttackSpeed = data.attackSpeed;
        }
    }
}