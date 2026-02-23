using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class UnitInfoPanel : MonoBehaviour
{
    public static UnitInfoPanel Instance; // 싱글톤

    [Header("1. 단일 선택 UI")]
    public GameObject contentRoot; // 기존 단일 정보창
    public Image portraitImage;
    public TMP_Text nameText;      
    public TMP_Text rankText;    
    public TMP_Text damageText;
    public TMP_Text speedText;
    public TMP_Text rangeText;
    public TMP_Text descriptionText;

    [Header("   속성 & 종족 표시")]
    public Image attributeIcon;    
    public TMP_Text attributeText;
    public Image raceIcon;         
    public TMP_Text raceText;      

    [Header("2. 다중 선택 UI ")]
    public GameObject multiSelectionRoot; 
    public Transform gridContainer;       
    public GameObject multiSlotPrefab;    

    [Header("아이콘 크기 설정")]
    public float maxIconSize = 80f; // 아이콘이 아무리 커져도 이 이상은 안 커짐 (1명일 때 너무 거대해짐 방지)
    public float spacing = 5f;      // 아이콘 사이 간격

    private UnitStat currentSingleTarget; // 실시간 갱신용 타겟 저장


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CloseAllPanels();
    }

    void Update()
    {
        // 단일 유닛 선택 중일 때, 공격력 같은 수치가 버프 등으로 변할 수 있으므로 실시간 갱신
        if (contentRoot.activeSelf && currentSingleTarget != null)
        {
            UpdateLiveValues();
        }
    }

    // RTSController가 호출할 메인 함수 (리스트를 통째로 받음)
    public void UpdateSelection(List<UnityEngine.AI.NavMeshAgent> selectedUnits)
    {
        // 1. 선택된 게 없으면 -> 다 끄기
        if (selectedUnits == null || selectedUnits.Count == 0)
        {
            CloseAllPanels();
            return;
        }

        if (EnemyInfoPanel.Instance != null)
        {
            EnemyInfoPanel.Instance.HidePanel();
        }

        // 2. 한 명만 선택됨 -> 기존 단일 정보창 켜기
        if (selectedUnits.Count == 1)
        {
            // null 체크 (삭제된 유닛 방지)
            if (selectedUnits[0] == null)
            {
                CloseAllPanels();
                return;
            }

            UnitStat stat = selectedUnits[0].GetComponent<UnitStat>();
            UpdateSingleInfo(stat);
        }
        // 3. 여러 명 선택됨 -> 다중 선택창(그리드) 켜기
        else
        {
            UpdateMultiInfo(selectedUnits);
        }
    }

    // 기존 단일 정보 갱신 함수 
    // --- 단일 정보창 로직 ---
    void UpdateSingleInfo(UnitStat stat)
    {
        if (stat == null || stat.data == null)
        {
            CloseAllPanels();
            return;
        }

        currentSingleTarget = stat; // 저장 (Update에서 갱신용)

        // 패널 전환
        if (contentRoot != null) contentRoot.SetActive(true);
        if (multiSelectionRoot != null) multiSelectionRoot.SetActive(false);

        // 기본 정보 갱신
        UnitData data = stat.data;
        if (portraitImage != null) portraitImage.sprite = data.icon;
        if (nameText != null) nameText.text = data.unitName;
        if (rankText != null) rankText.text = $"{data.level}";

        if (descriptionText != null) descriptionText.text = data.description;
       
        // 수치 정보 (UpdateLiveValues에서 계속 갱신됨)
        UpdateLiveValues();

        // 속성 정보 표시
        if (data.attribute != null)
        {
            if (attributeIcon != null)
            {
                attributeIcon.gameObject.SetActive(true);
                attributeIcon.sprite = data.attribute.icon;
            }
            if (attributeText != null)
            {
                attributeText.text = data.attribute.traitName;
                attributeText.color = data.attribute.color;
            }
        }
        else
        {
            if (attributeIcon != null) attributeIcon.gameObject.SetActive(false);
            if (attributeText != null) attributeText.text = "-";
        }

        //  종족 정보 표시
        if (data.race != null)
        {
            if (raceIcon != null)
            {
                raceIcon.gameObject.SetActive(true);
                raceIcon.sprite = data.race.icon;
            }
            if (raceText != null)
            {
                raceText.text = data.race.traitName;
                raceText.color = data.race.color;
            }
        }
        else
        {
            if (raceIcon != null) raceIcon.gameObject.SetActive(false);
            if (raceText != null) raceText.text = "-";
        }
    }

    // 실시간으로 변하는 수치 (공격력 등) 갱신
    void UpdateLiveValues()
    {
        if (currentSingleTarget == null) return;

        // 데이터 원본(data.damage)이 아니라, 현재 스탯(stat.damage)을 가져와야 버프 반영됨
        // (UnitStat에 현재 데미지 변수가 있다고 가정. 없다면 data.damage 사용)
        int currentDmg = currentSingleTarget.data.damage;
        AttackType type = currentSingleTarget.data.attackType;

        // 공격 타입에 따른 텍스트 색상 설정
        string typeColor = "white";
        string typeName = "";

        switch (type)
        {
            case AttackType.Physical:
                typeColor = "#FF5555"; // 빨강 (물리)
                typeName = "물리";
                break;
            case AttackType.Magic:
                typeColor = "#5555FF"; // 파랑 (마법)
                typeName = "마법";
                break;
            case AttackType.Fixed:
                typeColor = "#FFFFFF"; // 흰색 (고정)
                typeName = "고정";
                break;
        }

        // 공격력 텍스트: "공격력: 50 (물리)"
        if (damageText != null)
        {
            damageText.text = $"공격력: {currentDmg} <size=30><color={typeColor}>({typeName})</color></size>";
        }
        if (speedText != null)
        {
            // F2:소수점 둘째 자리까지 (예: 1.50 /초)
            speedText.text = $"공격속도: {currentSingleTarget.data.attackSpeed:F2} /초";
        }
        if (rangeText != null) rangeText.text = $"사거리: {currentSingleTarget.data.attackRange}";
    }


    // 다중 선택 그리드 채우기
    void UpdateMultiInfo(List<UnityEngine.AI.NavMeshAgent> units)
    {
        if (contentRoot != null) contentRoot.SetActive(false);
        if (multiSelectionRoot != null) multiSelectionRoot.SetActive(true);

        // 정렬
        var sortedUnits = units
            .Where(u => u != null)
            .OrderByDescending(u => u.GetComponent<UnitStat>().data.rank)
            .ThenBy(u => u.GetComponent<UnitStat>().data.unitName)
            .ToList();

        // 기존 슬롯 삭제
        foreach (Transform child in gridContainer) Destroy(child.gameObject);

        // ★ 핵심: 패널에 딱 맞는 최적의 사이즈 계산
        CalculateBestFit(sortedUnits.Count);

        RTSController controller = FindObjectOfType<RTSController>();

        // 슬롯 생성
        foreach (var unit in sortedUnits)
        {
            GameObject slotObj = Instantiate(multiSlotPrefab, gridContainer);
            MultiSelectSlot slot = slotObj.GetComponent<MultiSelectSlot>();
            if (slot != null) slot.Setup(unit, controller);
        }
    }

    // Best Fit 알고리즘: 주어진 공간 내에서 아이콘을 가장 크게 만드는 행/열 찾기
    void CalculateBestFit(int count)
    {
        GridLayoutGroup grid = gridContainer.GetComponent<GridLayoutGroup>();
        RectTransform rect = gridContainer.GetComponent<RectTransform>();

        if (grid == null || rect == null || count == 0) return;

        float panelWidth = rect.rect.width;
        float panelHeight = rect.rect.height;

        float bestSize = 0f;
        int bestCols = 1;

        // 1줄일 때부터 count줄일 때까지 전부 계산해봅니다.
        // 보통 RTS 하단 패널은 가로로 기니까 1~3줄 사이에서 결판이 날 겁니다.
        for (int rows = 1; rows <= count; rows++)
        {
            // 이 줄 수(rows)를 맞추려면 열(cols)은 몇 개 필요한가?
            int cols = Mathf.CeilToInt((float)count / rows);

            // 그때의 가로 최대 공간
            float availableWidth = panelWidth - (spacing * (cols - 1)) - grid.padding.horizontal;
            float cellWidth = availableWidth / cols;

            // 그때의 세로 최대 공간
            float availableHeight = panelHeight - (spacing * (rows - 1)) - grid.padding.vertical;
            float cellHeight = availableHeight / rows;

            // 둘 중 더 작은 쪽이 실제 아이콘 크기가 됨 (정사각형 유지)
            float currentSize = Mathf.Min(cellWidth, cellHeight);

            // 지금까지 찾은 것 중 제일 크다면 당첨!
            if (currentSize > bestSize)
            {
                bestSize = currentSize;
                bestCols = cols;
            }
        }

        // 최대 크기 제한 (너무 거대해지는 것 방지)
        bestSize = Mathf.Min(bestSize, maxIconSize);

        // 최종 적용
        grid.cellSize = new Vector2(bestSize, bestSize);
        grid.spacing = new Vector2(spacing, spacing);

        // 왼쪽 위부터 채우기 위해 설정 강제
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperLeft;

        // 열 개수를 고정해야 가로폭에 맞춰서 줄바꿈이 일어남
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = bestCols;
    }


    public void CloseAllPanels()
    {
        if (contentRoot != null) contentRoot.SetActive(false);
        if (multiSelectionRoot != null) multiSelectionRoot.SetActive(false);
    }
}