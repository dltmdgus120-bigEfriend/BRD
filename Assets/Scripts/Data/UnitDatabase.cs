using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Unit Database", menuName = "Trickcal/Database")]
public class UnitDatabase : ScriptableObject
{
    [Header("등급별 사도 리스트")]
    public List<UnitData> rank1Units; // 1성 목록
    public List<UnitData> rank2Units; // 2성 목록
    public List<UnitData> rank3Units; // 3성 목록

    // 랜덤으로 다음 등급 유닛을 뽑아주는 함수
    public UnitData GetRandomUnit(int nextRank)
    {
        List<UnitData> targetList = null;

        switch (nextRank)
        {
            case 1: targetList = rank1Units; break;
            case 2: targetList = rank2Units; break;
            case 3: targetList = rank3Units; break;
        }

        if (targetList != null && targetList.Count > 0)
        {
            int randomIndex = Random.Range(0, targetList.Count);
            return targetList[randomIndex];
        }

        Debug.LogError(nextRank + "성 유닛 데이터가 없습니다!");
        return null;
    }
}