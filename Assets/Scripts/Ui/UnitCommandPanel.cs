using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.AI;

public class UnitCommandPanel : MonoBehaviour
{
    [Header("연결 필요")]
    public RTSController rtsController;
    public RecipeDatabase recipeDatabase;

    [Header("UI 요소")]
    public GameObject panelObj;
    public Button mergeButton;
    public Image resultUnitIcon; // ★ 버튼 위에 띄울 결과물 얼굴
    public Text recipeInfoText;  // ★ 조합식 설명 텍스트
    public Button holdButton;

    // 내부 변수: 지금 찾은 짝꿍 유닛 (조합할 때 얘를 희생양으로 씀)
    private NavMeshAgent partnerUnit;
    private CombinationRecipe currentRecipe;

    void Update()
    {
        CheckSelection();
    }

    void CheckSelection()
    {
        // 1. 선택된 유닛이 딱 1마리일 때만 작동 (원랜디 방식)
        if (rtsController.selectedUnits.Count != 1)
        {
            panelObj.SetActive(false);
            return;
        }

        // 선택된 유닛 정보 가져오기
        NavMeshAgent selectedUnit = rtsController.selectedUnits[0];
        if (selectedUnit == null) return;

        UnitStat selectedStat = selectedUnit.GetComponent<UnitStat>();
        if (selectedStat == null || selectedStat.data == null) return;

        // 패널 켜기
        panelObj.SetActive(true);

        // 2. 이 유닛으로 만들 수 있는 레시피가 있는지, 그리고 재료가 맵에 있는지 검색
        // (FindMatchRecipe 함수가 아래에 있습니다)
        bool canMerge = FindMatchRecipe(selectedUnit, selectedStat.data);

        if (canMerge)
        {
            // 조합 가능!
            mergeButton.interactable = true;

            // ★ UI 업데이트: 결과물 얼굴 & 텍스트
            if (resultUnitIcon != null)
            {
                resultUnitIcon.sprite = currentRecipe.resultUnit.icon;
                resultUnitIcon.gameObject.SetActive(true);
            }

            // 텍스트 형식: [재료1] + [재료2] -> [결과물] \n 설명
            string mat1 = currentRecipe.ingredients[0].unitName;
            string mat2 = currentRecipe.ingredients[1].unitName; // 2개 재료 기준
            string result = currentRecipe.resultUnit.unitName;
            string desc = currentRecipe.resultUnit.description;

            recipeInfoText.text = $"<color=yellow>{mat1} + {mat2}</color> = <color=green>{result}</color>\n{desc}";
        }
        else
        {
            // 조합 불가능 (재료 부족 or 레시피 없음)
            mergeButton.interactable = false;

            if (resultUnitIcon != null) resultUnitIcon.gameObject.SetActive(false); // 아이콘 끄기
            recipeInfoText.text = $"{selectedStat.Name}\n(미구현)";
        }
    }

    // ★ 핵심 로직: 짝꿍 찾기
    bool FindMatchRecipe(NavMeshAgent myUnit, UnitData myData)
    {
        // 1. 모든 레시피를 뒤져본다
        foreach (var recipe in recipeDatabase.allRecipes)
        {
            // 이 레시피에 내가 재료로 들어가는가?
            if (recipe.ingredients.Contains(myData))
            {
                // 2. 내가 재료라면, "나 말고 다른 재료(짝꿍)"가 뭔지 알아낸다
                // (일단 재료 2개짜리 레시피라고 가정)
                List<UnitData> ingredientsNeeded = new List<UnitData>(recipe.ingredients);
                ingredientsNeeded.Remove(myData); // 내 데이터 하나 뺌

                if (ingredientsNeeded.Count > 0)
                {
                    UnitData partnerData = ingredientsNeeded[0]; // 필요한 짝꿍 데이터

                    // 3. 맵 전체에서 짝꿍 유닛을 찾는다 (나 자신은 제외!)
                    partnerUnit = FindPartnerOnMap(partnerData, myUnit);

                    if (partnerUnit != null)
                    {
                        // 찾았다!
                        currentRecipe = recipe;
                        return true;
                    }
                }
            }
        }
        return false; // 맞는 레시피나 짝꿍이 없음
    }

    // 맵에 있는 유닛 중 조건에 맞는 녀석 하나 데려오기
    NavMeshAgent FindPartnerOnMap(UnitData targetData, NavMeshAgent ignoreMe)
    {
        // 최적화를 위해 Tag나 Layer로 찾는 게 좋지만, 일단은 모든 UnitStat 검색
        UnitStat[] allUnits = FindObjectsOfType<UnitStat>();

        foreach (var unit in allUnits)
        {
            // 나 자신은 건너뜀
            if (unit.gameObject == ignoreMe.gameObject) continue;

            // 데이터가 일치하고, 아직 살아있는 녀석이라면
            if (unit.data == targetData)
            {
                return unit.GetComponent<NavMeshAgent>();
            }
        }
        return null;
    }

    public void OnClickMerge()
    {
        // 조합 실행
        if (currentRecipe != null && partnerUnit != null && rtsController.selectedUnits.Count > 0)
        {
            NavMeshAgent myUnit = rtsController.selectedUnits[0];
            Vector3 spawnPos = myUnit.transform.position;

            // ★ 나(선택된 애)와 짝꿍(찾은 애) 둘 다 제거
            Destroy(myUnit.gameObject);
            Destroy(partnerUnit.gameObject);

            // 결과물 소환
            GameObject newUnit = Instantiate(currentRecipe.resultUnit.prefab, spawnPos, Quaternion.identity);

            // 데이터 주입
            UnitStat newStat = newUnit.GetComponent<UnitStat>();
            if (newStat != null) newStat.data = currentRecipe.resultUnit;
            SoundManager.Instance.PlayVoice(currentRecipe.resultUnit.summonVoice);

            // 선택 초기화 및 패널 갱신
            rtsController.ClearSelection();
            CheckSelection(); // 바로 UI 갱신

            Debug.Log($"조합 완료: {currentRecipe.resultUnit.unitName}");
        }
    }

    public void OnClickHold()
    {
        // 선택된 유닛들의 이동을 멈춤
        foreach (var agent in rtsController.selectedUnits)
        {
            if (agent != null)
            {
                agent.ResetPath(); // 네비게이션 경로 삭제 (멈춤)
                agent.velocity = Vector3.zero; // 즉시 정지
            }
        }
        Debug.Log("모든 유닛 정지 (Hold)");
    }
}