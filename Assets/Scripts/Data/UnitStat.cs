using UnityEngine;

public class UnitStat : MonoBehaviour
{
    public UnitData data; // 내 정보가 담긴 데이터 파일

    // 편의를 위해 정보를 바로 꺼낼 수 있게 해둠
    public int Rank => data.rank;
    public string Name => data.unitName;
}