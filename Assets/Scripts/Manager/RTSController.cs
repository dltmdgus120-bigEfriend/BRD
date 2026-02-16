using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class RTSController : MonoBehaviour
{
    [Header("UI 연결")]
    public RectTransform selectionBox;
    public UnitInfoPanel infoPanel;

    [Header("설정")]
    public LayerMask unitLayer;
    public LayerMask groundLayer;

    // 내부 변수
    public List<NavMeshAgent> selectedUnits = new List<NavMeshAgent>();
    // 부대 지정 저장소 (0~9번 키)
    private List<NavMeshAgent>[] controlGroups = new List<NavMeshAgent>[10];
    private Vector2 startPos;
    private bool isDragging = false;
        
    [Header("커서 설정")]
    public Texture2D defaultCursor;
    public Texture2D attackCursor;
    public Vector2 cursorHotspot = Vector2.zero;

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

        // UI 클릭 무시
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // 2. 드래그 시작
        if (Input.GetMouseButtonDown(0)) StartSelection();

        // 3. 드래그 중
        if (Input.GetMouseButton(0) && isDragging) UpdateSelectionBox();

        // 4. 드래그 끝
        if (Input.GetMouseButtonUp(0)) EndSelection();

        // 5. 우클릭 (이동)
        if (Input.GetMouseButtonDown(1) && selectedUnits.Count > 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                foreach (var agent in selectedUnits)
                {
                    if (agent == null) continue;
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
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, unitLayer))
        {
            NavMeshAgent agent = hit.collider.GetComponent<NavMeshAgent>();
            if (agent == null) agent = hit.collider.GetComponentInParent<NavMeshAgent>();

            if (agent != null) AddUnitToSelection(agent);
        }
        else if (!Input.GetKey(KeyCode.LeftShift)) DeselectAll();
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

    // ★ [오류 해결] UnitCommandPanel에서 호출할 수 있도록 새로 만든 함수
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
        if (infoPanel == null) return;

        // null인 유닛 청소 (죽은 유닛 제거)
        selectedUnits.RemoveAll(u => u == null);

        // 유닛 '하나'가 아니라 '리스트 전체'를 넘깁니다.
        infoPanel.UpdateSelection(selectedUnits);

        // 커맨드 패널 갱신
        if (UnitCommandPanel.Instance != null)
            UnitCommandPanel.Instance.UpdateCommandPanel();
    }

    void PerformAttackCommand()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            EnemyHP enemy = hit.collider.GetComponent<EnemyHP>();
            if (enemy == null) enemy = hit.collider.GetComponentInParent<EnemyHP>();

            if (enemy != null)
            {
                foreach (var agent in selectedUnits)
                {
                    if (agent == null) continue;
                    var attack = agent.GetComponent<UnitAttack>();
                    if (attack != null)
                    {
                        attack.isAttackMoving = false;
                        attack.target = enemy.transform;
                    }
                    agent.SetDestination(enemy.transform.position);
                }
            }
            else
            {
                foreach (var agent in selectedUnits)
                {
                    if (agent == null) continue;
                    var attack = agent.GetComponent<UnitAttack>();
                    if (attack != null) attack.OrderAttackMove(hit.point);
                }
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
}

