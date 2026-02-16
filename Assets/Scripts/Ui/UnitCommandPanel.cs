using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class UnitCommandPanel : MonoBehaviour
{
    // ★ [오류 해결] 다른 스크립트에서 접근할 수 있도록 Instance 변수 추가
    public static UnitCommandPanel Instance;

    [Header("슬롯 연결")]
    public CommandSlot[] slots; // 0~11번 슬롯

    [Header("기본 아이콘")]
    public Sprite attackIcon;
    public Sprite stopIcon;
    public Sprite holdIcon;
    public Sprite sellIcon;

    private RTSController rtsController;

    void Awake()
    {
        // ★ [오류 해결] 시작할 때 "내가 Instance다"라고 등록
        Instance = this;
    }

    void Start()
    {
        rtsController = FindObjectOfType<RTSController>();
        ClearAllSlots();
    }

    void Update()
    {
        

        if (rtsController != null && rtsController.selectedUnits.Count > 0)
        {
            if (Input.GetKeyDown(KeyCode.A)) OnClickAttack();
            if (Input.GetKeyDown(KeyCode.S)) OnClickStop();
            if (Input.GetKeyDown(KeyCode.H)) OnClickHold();

            var mainUnit = rtsController.selectedUnits[0].GetComponent<UnitSkillController>();
            if (mainUnit != null)
            {
                if (Input.GetKeyDown(KeyCode.Q)) mainUnit.OnClickSkillButton(0);
                if (Input.GetKeyDown(KeyCode.W)) mainUnit.OnClickSkillButton(1);
                if (Input.GetKeyDown(KeyCode.E)) mainUnit.OnClickSkillButton(2);
                if (Input.GetKeyDown(KeyCode.R)) mainUnit.OnClickSkillButton(3);
            }
        }
        // ★ 쿨타임 UI 갱신 (매 프레임)
        UpdateSkillCooldowns();
        
    }

    // ★ 스킬 버튼 세팅 함수 (CheckSelection 안에서 호출)
    void CheckSkills()
    {
        
        if (rtsController.selectedUnits.Count == 0) return;

        UnitStat mainStat = rtsController.selectedUnits[0].GetComponent<UnitStat>();
        if (mainStat == null || mainStat.data == null) return;

        // 스킬 리스트가 있으면 버튼 생성
        if (mainStat.data.skills != null)
        {
            int startSlotIndex = 8; // 8번 슬롯부터 시작

            for (int i = 0; i < mainStat.data.skills.Count; i++)
            {
                int slotIdx = startSlotIndex + i;
                if (slotIdx >= slots.Length) break;

                SkillBase skill = mainStat.data.skills[i];
                if (skill == null) continue;

                // 디버그 로그 (확인용)
                // Debug.Log($"[UI 그리기] 스킬 {i}: {skill.skillName}");

                int skillIndex = i;
                slots[slotIdx].Setup(
                    skill.icon,
                    skill.skillName,
                    skill.description + (skill.isPassive ? "\n<color=yellow>[패시브]</color>" : $"\n<color=cyan>[쿨타임: {skill.cooldown}초]</color>"),
                    false,
                    skill.isPassive ? null : () => OnClickSkill(skillIndex),
                    ""
                );
            }
        }
    }

    // 버튼을 클릭하면 이 함수가 실행됩니다.
    void OnClickSkill(int index)
    {
        Debug.Log($"[UI] 스킬 버튼 클릭됨! 슬롯 번호: {index}"); // 1. 여기까지 오는지 확인

        if (rtsController == null || rtsController.selectedUnits.Count == 0)
        {
            Debug.LogError("[UI] 선택된 유닛이 없습니다!");
            return;
        }

        // 대표 유닛(0번) 가져오기
        var unitSkill = rtsController.selectedUnits[0].GetComponent<UnitSkillController>();

        if (unitSkill != null)
        {
            Debug.Log($"[UI] 유닛({unitSkill.name})에게 스킬({index}) 사용 명령 보냄"); // 2. 여기까지 오는지 확인
            unitSkill.OnClickSkillButton(index);
        }
        else
        {
            Debug.LogError("[UI] 유닛에 UnitSkillController가 없습니다!");
        }
    }

    void UpdateSkillCooldowns()
    {
        if (rtsController.selectedUnits.Count == 0) return;

        var skillController = rtsController.selectedUnits[0].GetComponent<UnitSkillController>();
        if (skillController == null) return;

        int startSlotIndex = 7;
        for (int i = 0; i < 4; i++)
        {
            int slotIdx = startSlotIndex + i;
            if (slotIdx >= slots.Length) break;

            // CommandSlot에 쿨타임 표시 기능이 필요함 (아래에서 설명)
            // slots[slotIdx].SetCooldown(skillController.GetCooldownRatio(i));
        }
    }

    public void UpdateCommandPanel()
    {
        // 1. 일단 싹 비운다. (잔상 제거)
        ClearAllSlots();

        // 2. 유닛이 없으면 끝
        if (rtsController == null || rtsController.selectedUnits.Count == 0) return;

        // 3. 기본 명령 버튼 (공격, 정지, 홀드, 판매) 그리기
        SetBasicCommands();

        // 4. 조합 버튼 그리기
        CheckMerge();

        // 5. ★ 스킬 버튼 그리기 (여기서 호출!)
        CheckSkills();
    }

    void CheckSelection()
    {
        if (rtsController == null || rtsController.selectedUnits.Count == 0)
        {
            ClearAllSlots();
            return;
        }

        // null 체크 (삭제된 유닛 방어)
        if (rtsController.selectedUnits[0] == null)
        {
            rtsController.selectedUnits.RemoveAt(0);
            return;
        }    
    }

    void SetBasicCommands()
    {
        // 0: 공격, 1: 정지, 2: 홀드, 3: 판매
        if (slots.Length > 0) slots[0].Setup(attackIcon, "공격 (A)", "적을 공격합니다.", false, OnClickAttack);
        if (slots.Length > 1) slots[1].Setup(stopIcon, "정지 (S)", "멈춥니다.", false, OnClickStop);
        if (slots.Length > 2) slots[2].Setup(holdIcon, "홀드 (H)", "제자리를 지킵니다.", false, OnClickHold);

        // ★ 4번째 슬롯: 판매 버튼
        if (slots.Length > 3)
        {
            // 대표 유닛 정보 확인
            UnitStat mainStat = rtsController.selectedUnits[0].GetComponent<UnitStat>();
            if (mainStat != null && mainStat.data != null)
            {
                // 3성 이하만 판매 가능 (rank <= 3)
                bool canSell = mainStat.data.rank <= 3;

                slots[3].Setup(
                    sellIcon,
                    "판매 (V)",
                    canSell ? $"유닛을 판매하여\n재화 {mainStat.data.sellPrice}를 얻습니다." : "최상위 유닛은\n판매할 수 없습니다.",
                    !canSell, // 판매 불가면 잠금(true)
                    OnClickSell
                );
            }
        }
    }

    void CheckMerge()
    {
        if (slots == null) return;

        // ★ 조합 버튼은 이제 5번째(인덱스 4)부터 시작!
        int currentSlotIndex = 4;

        // 기존 슬롯 비우기 (4번부터 끝까지)
        for (int i = 4; i < slots.Length; i++) slots[i].Clear();

        if (rtsController.selectedUnits.Count == 0) return;

        UnitStat mainStat = rtsController.selectedUnits[0].GetComponent<UnitStat>();
        if (mainStat == null || mainStat.data == null) return;

        if (mainStat.data.availableRecipes != null)
        {
            foreach (var recipe in mainStat.data.availableRecipes)
            {
                if (currentSlotIndex >= slots.Length) break;

                bool isReady = CheckIfRecipeIsReady(recipe, mainStat.data);

                slots[currentSlotIndex].Setup(
                    recipe.resultUnit.icon,
                    recipe.resultUnit.unitName,
                    MakeRecipeDescription(recipe, mainStat.data),
                    !isReady,
                    () => ExecuteMerge(recipe)
                );

                currentSlotIndex++;
            }
        }
    }

    public void OnClickSell()
    {
        // 선택된 유닛들을 싹 다 검사해서 팝니다.
        // 리스트를 역순으로 돌거나 복사본을 써야 삭제 시 에러가 안 납니다.
        var unitsToSell = new List<UnityEngine.AI.NavMeshAgent>(rtsController.selectedUnits);

        int totalGain = 0;
        int soldCount = 0;

        foreach (var agent in unitsToSell)
        {
            if (agent == null) continue;
            UnitStat stat = agent.GetComponent<UnitStat>();

            // 데이터가 있고, 3성 이하인 경우만 판매
            if (stat != null && stat.data != null && stat.data.rank <= 3)
            {
                totalGain += stat.data.sellPrice;

                // 선택 해제 및 삭제
                rtsController.selectedUnits.Remove(agent);
                Destroy(agent.gameObject);
                soldCount++;
            }
        }

        if (soldCount > 0)
        {
            // ★ 여기에 재화 증가 로직 연결 (일단 로그로 대체)
            // Example: GameManager.Instance.AddGold(totalGain);
            Debug.Log($"유닛 {soldCount}명 판매 완료! (+{totalGain} 재화)");

            // 소리 재생 (돈 버는 소리?)
            // SoundManager.Instance.PlaySFX("SellCoin");

            rtsController.ClearSelection(); // 남은 게 있을 수 있으니 정리
            ClearAllSlots();
        }
    }

    bool CheckIfRecipeIsReady(CombinationRecipe recipe, UnitData myData)
    {
        List<UnitData> required = new List<UnitData>(recipe.ingredients);
        required.Remove(myData);

        var allAllies = FindObjectsOfType<UnitStat>()
            .Where(u => u.GetComponent<UnityEngine.AI.NavMeshAgent>() != null && !rtsController.selectedUnits.Contains(u.GetComponent<UnityEngine.AI.NavMeshAgent>()))
            .ToList();

        foreach (var req in required)
        {
            var partner = allAllies.FirstOrDefault(u => u.data == req);
            if (partner != null)
            {
                allAllies.Remove(partner);
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    string MakeRecipeDescription(CombinationRecipe recipe, UnitData myData)
    {
        string desc = "<color=yellow>[조합식]</color>\n";
        foreach (var ing in recipe.ingredients)
        {
            if (ing == myData) desc += $"- {ing.unitName} (나)\n";
            else desc += $"- {ing.unitName}\n";
        }
        desc += $"\n<color=cyan>결과: {recipe.resultUnit.unitName}</color>";
        return desc;
    }

    void ExecuteMerge(CombinationRecipe recipe)
    {
        if (rtsController.selectedUnits.Count == 0) return;

        var mainAgent = rtsController.selectedUnits[0];
        if (mainAgent == null) return;

        UnitStat mainStat = mainAgent.GetComponent<UnitStat>();

        List<UnitStat> partnersToDestroy = new List<UnitStat>();
        List<UnitData> required = new List<UnitData>(recipe.ingredients);

        required.Remove(mainStat.data);

        var allUnits = FindObjectsOfType<UnitStat>().ToList();
        allUnits.Remove(mainStat);

        foreach (var req in required)
        {
            var p = allUnits.FirstOrDefault(u => u.data == req && !partnersToDestroy.Contains(u));
            if (p != null) partnersToDestroy.Add(p);
            else return;
        }

        Vector3 spawnPos = mainAgent.transform.position;

        if (rtsController.selectedUnits.Contains(mainAgent))
            rtsController.selectedUnits.Remove(mainAgent);
        Destroy(mainAgent.gameObject);

        foreach (var p in partnersToDestroy)
        {
            var pAgent = p.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (pAgent != null && rtsController.selectedUnits.Contains(pAgent))
                rtsController.selectedUnits.Remove(pAgent);
            Destroy(p.gameObject);
        }

        rtsController.ClearSelection();

        // 결과물 소환
        GameObject newUnit = Instantiate(recipe.resultUnit.prefab, spawnPos, Quaternion.identity);

        var agent = newUnit.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
            newUnit.transform.position = spawnPos;
            agent.enabled = true;

            if (agent.isOnNavMesh == false)
            {
                UnityEngine.AI.NavMeshHit hit;
                if (UnityEngine.AI.NavMesh.SamplePosition(spawnPos, out hit, 5.0f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }
            }
        }

        UnitStat newStat = newUnit.GetComponent<UnitStat>();
        if (newStat != null) newStat.data = recipe.resultUnit;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayVoice(recipe.resultUnit.summonVoice);

        Debug.Log($"{recipe.resultUnit.unitName} 조합 성공!");

        // ★ [오류 해결] RTSController에 SelectUnit 함수가 있어야 이게 작동함
        if (agent != null) rtsController.SelectUnit(agent);

        ClearAllSlots();
    }

    // 기본 명령 함수들
    public void OnClickAttack() { rtsController.EnterAttackMode(); }
    public void OnClickStop()
    {
        foreach (var agent in rtsController.selectedUnits)
        {
            if (agent != null)
            {
                var attack = agent.GetComponent<UnitAttack>();
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
                if (attack != null) attack.OrderHold();
                else { agent.ResetPath(); agent.velocity = Vector3.zero; }
            }
        }
    }

    void ClearAllSlots()
    {
        if (slots == null) return;
        foreach (var slot in slots) if (slot != null) slot.Clear();
    }
}