using UnityEngine;
using System.Collections.Generic;
using System.Linq; // 리스트 비교를 쉽게 하기 위해 추가



[CreateAssetMenu(fileName = "Recipe Database", menuName = "Trickcal/Recipe Database")]
public class RecipeDatabase : ScriptableObject
{
    public List<CombinationRecipe> allRecipes;

    public CombinationRecipe FindRecipe(List<UnitData> selectedUnits)
    {
        // 안전장치 1: 레시피 목록 자체가 없으면 패스
        if (allRecipes == null) return null;

        foreach (var recipe in allRecipes)
        {
            // 안전장치 2: 빈 레시피거나, 재료 목록이 안 만들어져 있으면 패스
            if (recipe == null || recipe.ingredients == null) continue;

            // 1. 개수가 다르면 탈락
            if (recipe.ingredients.Count != selectedUnits.Count)
                continue;

            // 2. 내용물 비교
            List<UnitData> tempSelected = new List<UnitData>(selectedUnits);
            bool isMatch = true;

            foreach (var ingredient in recipe.ingredients)
            {
                // 안전장치 3: 재료 칸이 비어있으면(None) 에러 나니까 패스
                if (ingredient == null)
                {
                    isMatch = false;
                    break;
                }

                if (tempSelected.Contains(ingredient))
                {
                    tempSelected.Remove(ingredient);
                }
                else
                {
                    isMatch = false;
                    break;
                }
            }

            if (isMatch) return recipe;
        }
        return null;
    }
}