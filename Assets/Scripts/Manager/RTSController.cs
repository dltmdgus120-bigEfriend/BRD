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
    }

    void Update()
    {
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
        selectedUnits.RemoveAll(u => u == null);

        if (selectedUnits.Count > 0)
        {
            UnitStat stat = selectedUnits[0].GetComponent<UnitStat>();
            infoPanel.UpdateInfo(stat);

            // ★ [오류 해결] Instance가 이제 존재하므로 에러 없이 호출됨
            if (UnitCommandPanel.Instance != null)
                UnitCommandPanel.Instance.UpdateCommandPanel();
        }
        else
        {
            infoPanel.UpdateInfo(null);
            if (UnitCommandPanel.Instance != null)
                UnitCommandPanel.Instance.UpdateCommandPanel();
        }
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

