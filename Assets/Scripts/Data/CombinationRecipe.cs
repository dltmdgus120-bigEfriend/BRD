using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Trickcal/Combination Recipe")]
public class CombinationRecipe : ScriptableObject
{
    [Header("재료 목록 (몇 개든 상관없음!)")]
    public List<UnitData> ingredients; // 재료 리스트

    [Header("결과")]
    public UnitData resultUnit;  // 결과물

    [TextArea]
    public string description;   // 설명

    // 편의 기능: 재료가 다 있는지 검사하는 함수
    public bool CheckIngredients(List<UnitData> currentUnits)
    {
        // (이 함수는 나중에 UnitCommandPanel에서 씁니다)
        // 로직: 필요한 재료 목록을 복사해서, 현재 유닛들과 하나씩 지워가며 검사
        List<UnitData> tempIngredients = new List<UnitData>(ingredients);

        foreach (var unit in currentUnits)
        {
            if (tempIngredients.Contains(unit))
            {
                tempIngredients.Remove(unit);
            }
        }

        return tempIngredients.Count == 0; // 남은 재료가 없으면 통과
    }
}