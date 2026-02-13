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
}