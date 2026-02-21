using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class RTSController : MonoBehaviour
{
    [Header("UI 연결")]
    public RectTransform selectionBox;

    

    [Header("설정")]
    public LayerMask unitLayer;
    public LayerMask enemyLayer;
    public LayerMask groundLayer;

    // 내부 변수
    public List<NavMeshAgent> selectedUnits = new List<NavMeshAgent>();   
    private List<NavMeshAgent>[] controlGroups = new List<NavMeshAgent>[10]; // 부대 지정 저장소 (0~9번 키)
    private Vector2 startPos;
    private bool isDragging = false;
        
    [Header("커서 설정")]
    public Texture2D defaultCursor;
    public Texture2D attackCursor;
    public Texture2D skillCursor;
    public Vector2 cursorHotspot = Vector2.zero;

    [Header("스킬 범위 표시")]
    public GameObject rangeIndicatorPrefab; // 원 프리팹 연결
    private GameObject currentIndicator;    // 생성된 원 인스턴스

    // 스킬 조준 관련 변수
    private bool isSkillCommand = false;
    private int pendingSkillIndex = -1; // 사용 대기 중인 스킬 번호
    public bool isAttackCommand = false;

    void Start()
    {
        SetCursor(defaultCursor);

        //부대 지정 리스트 초기화 (이거 안 하면 에러 남!)
        for (int i = 0; i < 10; i++)
        {
            controlGroups[i] = new List<NavMeshAgent>();
        }

    }

    void Update()
    {
        // 부대 지정 입력 감지 함수 호출
        HandleControlGroups();

        // 1. 공격 명령 대기
        if (isAttackCommand)
        {
            if (Input.GetMouseButtonDown(0))
            {
                PerformAttackCommand();
                return;
            }
            else if (Input.GetMouseButtonDown(1))
            {
                isAttackCommand = false;
                SetCursor(defaultCursor);
            }
            return;
        }

        // 2. 스킬 명령 대기 
        if (isSkillCommand)
        {
            UpdateSkillIndicator();

            if (Input.GetMouseButtonDown(0))
            {
                PerformSkillCommand(); // 클릭한 위치에 스킬 발사!
                return;
            }
            else if (Input.GetMouseButtonDown(1)) // 우클릭 취소
            {
                CancelCommand();
                return;
            }
            return;
        }

        // UI 클릭 무시
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // 2. 드래그 시작
        if (Input.GetMouseButtonDown(0)) StartSelection();

        // 3. 드래그 중
        if (Input.GetMouseButton(0) && isDragging) UpdateSelectionBox();

        // 4. 드래그 끝
        if (Input.GetMouseButtonUp(0) && isDragging) EndSelection();

        // 5. 우클릭 (이동)
        if (Input.GetMouseButtonDown(1) && selectedUnits.Count > 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                foreach (var agent in selectedUnits)
                {
                    if (agent == null) continue;
                    UnitStat stat = agent.GetComponent<UnitStat>();
                    if (stat != null && stat.data != null && stat.data.isBuilding)
                    {
                        // (선택 사항) 건물이면 이동 대신 "랠리 포인트 지정" 등을 넣을 수 있음                       
                        continue;
                    }
                    var attack = agent.GetComponent<UnitAttack>();
                    if (attack != null) attack.OrderMove(hit.point);
                    else agent.SetDestination(hit.point);
                }
            }
        }
    }


    void HandleControlGroups()
    {
        // 알파벳 위 숫자키 0~9 감지 (Alpha0 ~ Alpha9)
        for (int i = 0; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                // LeftControl(왼쪽)을 확실하게 체크!
                // (혹시 몰라 오른쪽도 되게는 해뒀습니다. 둘 중 편한 거 쓰세요)
                // 유니티 에디터에서는 단축키랑 겹쳐니 안겹치는 부대지정으로 테스트할것 (ex: ctrl+2)
                bool isCtrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

                if (isCtrl)
                {
                    // 저장 (Ctrl + 숫자)
                    AssignControlGroup(i);
                }
                else
                {
                    // 불러오기 (그냥 숫자)
                    SelectControlGroup(i);
                }
            }
        }
    }

    // 부대 저장
    void AssignControlGroup(int index)
    {
        // 아무것도 선택 안 된 상태에서 Ctrl+숫자 누르면 -> 해당 그룹 비우기
        if (selectedUnits.Count == 0)
        {
            controlGroups[index].Clear();
            Debug.Log($"부대 {index}번 초기화");
            return;
        }

        // 현재 선택된 유닛들을 해당 번호 리스트에 복사
        controlGroups[index].Clear();
        controlGroups[index].AddRange(selectedUnits);

        Debug.Log($"부대 {index}번 지정 완료! ({selectedUnits.Count}명)");
    }

    // 부대 불러오기
    void SelectControlGroup(int index)
    {
        // 해당 그룹에 저장된 게 없으면 무시
        if (controlGroups[index].Count == 0) return;

        // 1. 죽은 유닛 청소 (null 제거)
        controlGroups[index].RemoveAll(u => u == null);

        // 2. 남은 게 없으면 종료
        if (controlGroups[index].Count == 0) return;

        // 3. 기존 선택 해제 후 그룹 유닛들 선택
        DeselectAll();

        foreach (var unit in controlGroups[index])
        {
            AddUnitToSelection(unit);
        }

        // UI 갱신
        UpdateSelectionUI();
    }

    void SetCursor(Texture2D cursorTexture)
    {
        Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
    }

    void StartSelection()
    {
        if (!Input.GetKey(KeyCode.LeftShift)) DeselectAll();
        startPos = Input.mousePosition;
        isDragging = true;
        if (selectionBox != null) selectionBox.gameObject.SetActive(true);
    }

    void UpdateSelectionBox()
    {
        if (selectionBox == null) return;
        Vector2 currentPos = Input.mousePosition;
        float width = Mathf.Abs(currentPos.x - startPos.x);
        float height = Mathf.Abs(currentPos.y - startPos.y);
        float x = Mathf.Min(startPos.x, currentPos.x);
        float y = Mathf.Min(startPos.y, currentPos.y);
        selectionBox.anchoredPosition = new Vector2(x, y);
        selectionBox.sizeDelta = new Vector2(width, height);
    }

    void EndSelection()
    {
        isDragging = false;
        if (selectionBox != null) selectionBox.gameObject.SetActive(false);

        if (selectionBox != null && selectionBox.sizeDelta.magnitude < 10) SelectSingleUnit();
        else SelectUnitsInBox();

        SortSelectedUnitsByPower();
        UpdateSelectionUI();
    }

    void SelectSingleUnit()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // 1. 먼저 아군 유닛을 클릭했는지 검사
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, unitLayer))
        {
            NavMeshAgent agent = hit.collider.GetComponent<NavMeshAgent>();
            if (agent == null) agent = hit.collider.GetComponentInParent<NavMeshAgent>();

            if (agent != null)
            {
                AddUnitToSelection(agent);

                // 아군을 눌렀으니 적 패널은 끄기
                if (EnemyInfoPanel.Instance != null) EnemyInfoPanel.Instance.HidePanel();
            }
        }
        // 2. 아군이 아니라면, 적군을 클릭했는지 검사
        else if (Physics.Raycast(ray, out RaycastHit hitEnemy, Mathf.Infinity, enemyLayer))
        {
            EnemyHP enemy = hitEnemy.collider.GetComponent<EnemyHP>();
            if (enemy == null) enemy = hitEnemy.collider.GetComponentInParent<EnemyHP>();

            Debug.Log($"마우스 레이캐스트 적중! 찾은 놈: {(enemy != null ? enemy.gameObject.name : "컴포넌트 못찾음")}");

            if (enemy != null)
            {
                if (!Input.GetKey(KeyCode.LeftShift)) DeselectAll(); // 아군 선택 싹 풀기

                // 적 패널 띄우기
                if (EnemyInfoPanel.Instance != null) EnemyInfoPanel.Instance.ShowEnemyInfo(enemy);
            }
        }
        // 3. 아군도 적군도 아닌 맨땅을 클릭했을 때
        else if (!Input.GetKey(KeyCode.LeftShift))
        {
            DeselectAll();
            if (EnemyInfoPanel.Instance != null) EnemyInfoPanel.Instance.HidePanel(); // 적 패널 숨기기
        }
    }

    void SelectUnitsInBox()
    {
        NavMeshAgent[] allUnits = FindObjectsOfType<NavMeshAgent>();
        if (selectionBox == null) return;

        Vector2 min = selectionBox.anchoredPosition;
        Vector2 max = selectionBox.anchoredPosition + selectionBox.sizeDelta;

        foreach (NavMeshAgent unit in allUnits)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
            if (screenPos.x > min.x && screenPos.x < max.x &&
                screenPos.y > min.y && screenPos.y < max.y)
            {
                AddUnitToSelection(unit);
            }
        }
    }

    void AddUnitToSelection(NavMeshAgent unit)
    {
        if (!selectedUnits.Contains(unit))
        {
            selectedUnits.Add(unit);
            UnitSelection selection = unit.GetComponent<UnitSelection>();
            if (selection != null) selection.SetSelected(true);
        }
    }

    void DeselectAll()
    {
        foreach (var unit in selectedUnits)
        {
            if (unit != null)
            {
                UnitSelection selection = unit.GetComponent<UnitSelection>();
                if (selection != null) selection.SetSelected(false);
            }
        }
        selectedUnits.Clear();
        UpdateSelectionUI();
    }

    //  UnitCommandPanel에서 호출할 수 있도록 새로 만든 함수
    public void SelectUnit(NavMeshAgent unit)
    {
        DeselectAll(); // 기존 선택 다 해제

        if (unit != null)
        {
            AddUnitToSelection(unit); // 새 유닛 추가
            UpdateSelectionUI(); // UI 갱신
        }
    }

    public void ClearSelection() { DeselectAll(); }

    void SortSelectedUnitsByPower()
    {
        if (selectedUnits.Count <= 1) return;
        selectedUnits.Sort((a, b) => {
            if (a == null || b == null) return 0;
            UnitStat statA = a.GetComponent<UnitStat>();
            UnitStat statB = b.GetComponent<UnitStat>();
            int damageA = (statA != null && statA.data != null) ? statA.data.damage : 0;
            int damageB = (statB != null && statB.data != null) ? statB.data.damage : 0;
            return damageB.CompareTo(damageA);
        });
    }

    void UpdateSelectionUI()
    {
        // 1. 죽은 유닛 청소
        selectedUnits.RemoveAll(u => u == null);

        // 2. 정보창(InfoPanel) 갱신
        if (UnitInfoPanel.Instance != null)
        {
            UnitInfoPanel.Instance.UpdateSelection(selectedUnits);
        }

        // 3. 커맨드 패널(CommandPanel) 갱신
        if (UnitCommandPanel.Instance != null)
        {
            UnitCommandPanel.Instance.UpdateCommandPanel();
        }
    }

    private EnemyHP currentFocusedEnemy;

    void PerformAttackCommand()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // 1순위: 적 점사
        if (Physics.Raycast(ray, out RaycastHit hitUnit, Mathf.Infinity, enemyLayer | unitLayer))
        {
            EnemyHP enemy = hitUnit.collider.GetComponent<EnemyHP>();
            if (enemy == null) enemy = hitUnit.collider.GetComponentInParent<EnemyHP>();

            if (enemy != null)
            {
                // 기존에 마크 켜진 놈이 있으면 꺼주기
                if (currentFocusedEnemy != null && currentFocusedEnemy != enemy)
                {
                    currentFocusedEnemy.SetFocusMark(false);
                }

                // 새로 찍힌 놈 마크 켜고 기억하기
                currentFocusedEnemy = enemy;
                enemy.SetFocusMark(true);

                // 유닛들에게 공격 명령!
                foreach (var agent in selectedUnits)
                {
                    if (agent == null) continue;
                    var attack = agent.GetComponent<UnitAttack>();
                    if (attack != null) attack.CommandFocusAttack(enemy.transform);
                }
            }
        }
        // 2순위: 어택땅 (땅바닥 클릭)
        else if (Physics.Raycast(ray, out RaycastHit hitGround, Mathf.Infinity, groundLayer))
        {
            // 땅바닥을 찍었으니 기존 점사 마크는 꺼버림
            if (currentFocusedEnemy != null)
            {
                currentFocusedEnemy.SetFocusMark(false);
                currentFocusedEnemy = null;
            }

            foreach (var agent in selectedUnits)
            {
                if (agent == null) continue;
                var attack = agent.GetComponent<UnitAttack>();
                if (attack != null) attack.OrderAttackMove(hitGround.point);
            }
        }

        isAttackCommand = false;
        SetCursor(defaultCursor);
    }

    public void EnterAttackMode()
    {
        isAttackCommand = true;
        if (attackCursor != null)
            SetCursor(attackCursor);
    }

    public void EnterSkillMode(int skillIndex)
    {
        isSkillCommand = true;
        isAttackCommand = false;
        pendingSkillIndex = skillIndex;

        SetCursor(skillCursor != null ? skillCursor : attackCursor);

        // --- 범위 표시기 켜기 ---
        if (selectedUnits.Count > 0)
        {
            var unitStat = selectedUnits[0].GetComponent<UnitStat>();
            if (unitStat != null && unitStat.data.skills.Count > skillIndex)
            {
                float radius = unitStat.data.skills[skillIndex].effectRadius;

                // 반경이 0보다 클 때만 표시 (단일 타겟 스킬은 표시 안 함)
                if (radius > 0)
                {
                    if (currentIndicator == null)
                        currentIndicator = Instantiate(rangeIndicatorPrefab);

                    currentIndicator.SetActive(true);
                    // 원의 크기 조절 (지름 = 반지름 * 2)
                    currentIndicator.transform.localScale = new Vector3(radius * 2, radius * 2, 1);
                }
            }
        }
    }

    void PerformSkillCommand()
    {
        Debug.Log("스킬 발사 시도!"); // 1. 함수 진입 확인

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // 레이캐스트 확인
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer | unitLayer))
        {
            Debug.Log($"타겟 위치 감지: {hit.point}, 맞은 놈: {hit.collider.name}"); // 2. 레이캐스트 성공 확인
            Debug.Log($" 현재 선택된 유닛 수: {selectedUnits.Count}명");

            // 선택된 유닛들에게 "저기에 스킬 써!" 명령
            foreach (var unit in selectedUnits)
            {
                if (unit == null)
                {
                    Debug.LogError(" 선택된 유닛 리스트에 '빈 껍데기(null)'가 들어있습니다!");
                    continue;
                }

                // ... (스킬 발사 로직) ...
                var skillController = unit.GetComponent<UnitSkillController>();
                if (skillController != null)
                {
                    Debug.Log($" {unit.name}에게 스킬 발사 명령!"); // 이게 안 뜨고 있음
                    skillController.UseSkill(pendingSkillIndex, hit.point);
                }
            }
        }
        else
        {
            Debug.LogError("레이캐스트 실패! 바닥(Ground) 레이어 설정을 확인하세요."); // 3. 실패 원인
        }

        CancelCommand();
    }

    void CancelCommand()
    {
        isAttackCommand = false;
        isSkillCommand = false;
        SetCursor(defaultCursor);

        // 원 숨기기
        if (currentIndicator != null) currentIndicator.SetActive(false);
    }

    //  마우스 위치로 원 이동
    void UpdateSkillIndicator()
    {
        if (currentIndicator == null || !currentIndicator.activeSelf) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            // 바닥 위로 살짝 띄워서 위치시킴
            currentIndicator.transform.position = hit.point + Vector3.up * 0.1f;
        }
    }


}

