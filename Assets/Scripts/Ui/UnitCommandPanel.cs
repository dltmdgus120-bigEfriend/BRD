using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class UnitCommandPanel : MonoBehaviour
{
    [Header("슬롯 연결")]
    public CommandSlot[] slots; // 0~11번 슬롯

    [Header("기본 아이콘")]
    public Sprite attackIcon;
    public Sprite stopIcon;
    public Sprite holdIcon;

    [Header("조합 데이터베이스")]
    public RecipeDatabase recipeDB;

    private RTSController rtsController;

    void Start()
    {
        rtsController = FindObjectOfType<RTSController>();
        ClearAllSlots();
    }

    void Update()
    {
        CheckSelection();

        // ★ 단축키 입력 감지 (유닛이 선택되어 있을 때만)
        if (rtsController.selectedUnits.Count > 0)
        {
            if (Input.GetKeyDown(KeyCode.A)) OnClickAttack();
            if (Input.GetKeyDown(KeyCode.S)) OnClickStop();
            if (Input.GetKeyDown(KeyCode.H)) OnClickHold();
        }
    }

    void CheckSelection()
    {
        // 선택된 게 없으면 싹 비우기
        if (rtsController == null || rtsController.selectedUnits.Count == 0)
        {
            ClearAllSlots();
            return;
        }

        // 1. 기본 명령 (공격, 정지, 홀드) 세팅
        SetBasicCommands();

        // 2. 조합 버튼 세팅 (가능한 모든 레시피 표시)
        CheckMerge();
    }

    void SetBasicCommands()
    {
        // Setup(아이콘, 제목, 설명, 잠김여부, 클릭함수) - 5개를 꽉 채워야 에러가 안 납니다!

        if (slots.Length > 0)
            slots[0].Setup(attackIcon, "공격 (A)", "지정한 위치로 이동하며 적을 공격합니다.", false, OnClickAttack);

        if (slots.Length > 1)
            slots[1].Setup(stopIcon, "정지 (S)", "모든 행동을 멈춥니다.", false, OnClickStop);

        if (slots.Length > 2)
            slots[2].Setup(holdIcon, "홀드 (H)", "제자리를 사수하며 사거리 내 적을 공격합니다.", false, OnClickHold);
    }

    void CheckMerge()
    {
        if (recipeDB == null || slots == null) return;

        // 조합 버튼은 4번 슬롯(인덱스 4)부터 채우기 시작합니다. (0,1,2는 기본명령, 3은 비워둠)
        int currentSlotIndex = 4;

        // 기존에 4번부터 끝까지 남아있던 버튼들은 일단 지워줍니다.
        for (int i = 4; i < slots.Length; i++)
        {
            slots[i].Clear();
        }

        // 선택된 유닛(대표) 정보 가져오기
        if (rtsController.selectedUnits.Count == 0) return;
        UnitData mainUnit = rtsController.selectedUnits[0].GetComponent<UnitStat>().data;

        // ★ 핵심: 모든 레시피를 뒤져서 내가 재료로 들어가는 걸 다 찾습니다.
        foreach (var recipe in recipeDB.allRecipes)
        {
            // 더 이상 슬롯이 없으면 중단
            if (currentSlotIndex >= slots.Length) break;

            // 내가 재료에 포함되어 있는가?
            if (recipe.ingredients.Contains(mainUnit))
            {
                // 이 레시피의 상태(재료 다 모았나?) 확인
                bool isReady = CheckIfRecipeIsReady(recipe, mainUnit);

                // 툴팁 설명 만들기
                string tooltipTitle = recipe.resultUnit.unitName; // 결과물 이름
                string tooltipDesc = MakeRecipeDescription(recipe, mainUnit); // 재료 목록 설명

                // ★ 중요: 버튼마다 클릭 시 실행할 함수를 다르게 지정해야 합니다. (람다식 사용)
                // "이 버튼을 누르면 -> ExecuteMerge 함수에 이 recipe를 넣어서 실행해라"
                slots[currentSlotIndex].Setup(
                    recipe.resultUnit.icon,
                    tooltipTitle,
                    tooltipDesc,
                    !isReady, // 준비 안 됐으면 잠금(true)
                    () => ExecuteMerge(recipe)
                );

                currentSlotIndex++; // 다음 칸으로 이동
            }
        }
    }

    // 재료가 맵에 다 있는지 확인하는 함수
    bool CheckIfRecipeIsReady(CombinationRecipe recipe, UnitData myData)
    {
        List<UnitData> required = new List<UnitData>(recipe.ingredients);
        required.Remove(myData); // 내 몫은 뺌

        // 맵의 모든 유닛을 가져옴 (나 자신은 제외)
        var allUnits = FindObjectsOfType<UnitStat>()
            .Where(u => !rtsController.selectedUnits.Contains(u.GetComponent<UnityEngine.AI.NavMeshAgent>()))
            .ToList();

        foreach (var req in required)
        {
            // 필요한 재료와 일치하는 유닛 찾기
            var partner = allUnits.FirstOrDefault(u => u.data == req);
            if (partner != null)
            {
                allUnits.Remove(partner); // 찾았으면 리스트에서 제거 (중복 사용 방지)
            }
            else
            {
                return false; // 하나라도 없으면 false
            }
        }
        return true; // 다 있으면 true
    }

    // 툴팁에 띄울 설명글 만드는 함수
    string MakeRecipeDescription(CombinationRecipe recipe, UnitData myData)
    {
        string desc = "<color=yellow>[조합식]</color>\n";

        // 재료 목록 표시
        foreach (var ing in recipe.ingredients)
        {
            if (ing == myData) desc += $"- {ing.unitName} (나)\n";
            else desc += $"- {ing.unitName}\n";
        }

        desc += $"\n<color=cyan>결과: {recipe.resultUnit.unitName}</color>";
        return desc;
    }


    // --- 실행 함수들 ---

   
    // 실제 조합 실행 (버튼 클릭 시 호출됨)
    void ExecuteMerge(CombinationRecipe recipe)
    {
        // 1. 메인 재료 (선택된 유닛 중 대장)
        // 안전장치: 선택된 게 없으면 중단
        if (rtsController.selectedUnits.Count == 0) return;

        var mainAgent = rtsController.selectedUnits[0];
        UnitStat mainStat = mainAgent.GetComponent<UnitStat>();

        // 2. 필요한 나머지 재료 찾기
        List<UnitStat> partnersToDestroy = new List<UnitStat>();
        List<UnitData> required = new List<UnitData>(recipe.ingredients);

        // 레시피 재료 목록에서 내 몫(메인 유닛) 하나를 뺍니다.
        required.Remove(mainStat.data);

        // 맵에 있는 모든 유닛을 가져오되, '메인 유닛(나 자신)'은 제외합니다.
        // (이제 선택된 다른 유닛도 재료 후보에 포함됩니다!)
        var allUnits = FindObjectsOfType<UnitStat>().ToList();
        allUnits.Remove(mainStat);

        // 짝꿍들 수집
        foreach (var req in required)
        {
            // 이미 재료로 쓰기로 한 녀석(`partnersToDestroy`에 있는 애)은 중복해서 뽑지 않도록 체크
            var p = allUnits.FirstOrDefault(u => u.data == req && !partnersToDestroy.Contains(u));

            if (p != null)
            {
                partnersToDestroy.Add(p);
            }
            else
            {
                // 재료 부족 (타이밍 이슈로 사라졌을 때)
                TooltipManager.Instance.ShowWarning("유닛이 부족합니다!");
                return;
            }
        }

        // --- 진짜 조합 시작 ---
        Vector3 spawnPos = mainAgent.transform.position;

        // ★ 수정된 삭제 로직: 선택된 애들을 다 지우는 게 아니라, '재료로 쓰인 애들'만 지움

        // 1. 나(메인) 삭제
        Destroy(mainAgent.gameObject);

        // 2. 짝꿍들 삭제
        foreach (var p in partnersToDestroy) Destroy(p.gameObject);

        // 3. 소환
        GameObject newUnit = Instantiate(recipe.resultUnit.prefab, spawnPos, Quaternion.identity);

        // 데이터 주입 (혹시 프리팹에 데이터가 비어있을 경우를 대비)
        UnitStat newStat = newUnit.GetComponent<UnitStat>();
        if (newStat != null) newStat.data = recipe.resultUnit;

        // 4. 소리
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayVoice(recipe.resultUnit.summonVoice);

        Debug.Log($"{recipe.resultUnit.unitName} 조합 성공!");

        // 선택 초기화 및 UI 갱신
        rtsController.ClearSelection();
        ClearAllSlots();
    }

    public void OnClickAttack()
    {
        // 컨트롤러에게 "공격 모드 시작해"라고 전달
        rtsController.EnterAttackMode();
    }

    public void OnClickStop()
    {
        foreach (var agent in rtsController.selectedUnits)
        {
            if (agent != null)
            {
                var attack = agent.GetComponent<UnitAttack>();
                // OrderStop 호출 (공격도 멈춤)
                if (attack != null) attack.OrderStop();
                else agent.ResetPath();
            }
        }
    }

    public void OnClickHold()
    {
        foreach (var agent in rtsController.selectedUnits)
        {
            if (agent != null)
            {
                var attack = agent.GetComponent<UnitAttack>();
                //  OrderHold 호출 (공격은 함)
                if (attack != null) attack.OrderHold();
                else
                {
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;
                }
            }
        }
    }

    void ClearAllSlots()
    {
        if (slots == null) return;
        foreach (var slot in slots) if (slot != null) slot.Clear();
    }
}