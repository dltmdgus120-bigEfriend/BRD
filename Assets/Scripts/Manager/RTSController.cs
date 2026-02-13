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
    public Vector2 cursorHotspot = Vector2.zero; // 커서 클릭 지점 (보통 0,0)

    public bool isAttackCommand = false; // 공격 모드 스위치

    void Start()
    {
        // 게임 시작하면 기본 커서로 변경
        SetCursor(defaultCursor);
    }

    void Update()
    {
        // 1. 공격 명령 대기 상태
        if (isAttackCommand)
        {
            if (Input.GetMouseButtonDown(0)) // 좌클릭 (명령 실행)
            {
                PerformAttackCommand();
                return;
            }
            else if (Input.GetMouseButtonDown(1)) // 우클릭 (취소)
            {
                isAttackCommand = false;
                SetCursor(defaultCursor); // ★ 취소했으니 기본 커서로 복구
            }
            return;
        }

        // UI 위를 클릭했다면 무시
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // 2. 마우스 클릭 (드래그 시작)
        if (Input.GetMouseButtonDown(0))
        {
            StartSelection();
        }

        // 3. 마우스 누르는 중 (박스 그리기)
        if (Input.GetMouseButton(0) && isDragging)
        {
            UpdateSelectionBox();
        }

        // 4. 마우스 뗌 (선택 확정)
        if (Input.GetMouseButtonUp(0))
        {
            EndSelection();
        }

        // 5. 우클릭 (이동)
        if (Input.GetMouseButtonDown(1) && selectedUnits.Count > 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                foreach (var agent in selectedUnits)
                {
                    var attack = agent.GetComponent<UnitAttack>();
                    if (attack != null)
                    {
                        attack.OrderMove(hit.point);
                    }
                    else
                    {
                        agent.SetDestination(hit.point);
                    }
                }
            }
        }
    }

    // ★ 커서 바꾸는 함수 (코드를 깔끔하게 쓰기 위해 만듦)
    void SetCursor(Texture2D cursorTexture)
    {
        // cursorTexture: 바꿀 이미지
        // cursorHotspot: 클릭 지점 (기본은 0,0 좌상단)
        // CursorMode.Auto: 하드웨어 커서 사용 (반응 빠름)
        Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
    }

    // (기존 선택 관련 함수들은 그대로 유지...)
    void StartSelection()
    {
        if (!Input.GetKey(KeyCode.LeftShift)) DeselectAll();
        startPos = Input.mousePosition;
        isDragging = true;
        selectionBox.gameObject.SetActive(true);
    }

    void UpdateSelectionBox()
    {
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
        selectionBox.gameObject.SetActive(false);
        if (selectionBox.sizeDelta.magnitude < 10) SelectSingleUnit();
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
            if (agent != null) AddUnitToSelection(agent);
        }
        else if (!Input.GetKey(KeyCode.LeftShift)) DeselectAll();
    }

    void SelectUnitsInBox()
    {
        NavMeshAgent[] allUnits = FindObjectsOfType<NavMeshAgent>();
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

            // 원 켜기
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
                // 원 끄기
                UnitSelection selection = unit.GetComponent<UnitSelection>();
                if (selection != null) selection.SetSelected(false);
            }
        }
        selectedUnits.Clear();
        UpdateSelectionUI(); // UI 초기화
    }

    public void ClearSelection() { DeselectAll(); }

    void SortSelectedUnitsByPower()
    {
        if (selectedUnits.Count <= 1) return;
        selectedUnits.Sort((a, b) => {
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
        if (selectedUnits.Count > 0 && selectedUnits[0] != null)
        {
            UnitStat stat = selectedUnits[0].GetComponent<UnitStat>();
            infoPanel.UpdateInfo(stat);
        }
        else
        {
            infoPanel.UpdateInfo(null);
        }
    }

    // --- 공격 명령 관련 수정됨 ---

    void PerformAttackCommand()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            EnemyHP enemy = hit.collider.GetComponent<EnemyHP>();
            if (enemy != null)
            {
                foreach (var agent in selectedUnits)
                {
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
                    var attack = agent.GetComponent<UnitAttack>();
                    if (attack != null) attack.OrderAttackMove(hit.point);
                }
            }
        }

        // 명령 끝났으니 초기화
        isAttackCommand = false;
        SetCursor(defaultCursor); // ★ 공격 끝났으니 기본 커서로 복구
    }

    // 외부에서 호출할 공격 모드 진입 함수
    public void EnterAttackMode()
    {
        isAttackCommand = true;

        // ★ 공격 커서로 변경
        // 만약 attackCursor가 비어있으면 변경 안 함 (오류 방지)
        if (attackCursor != null)
        {
            SetCursor(attackCursor);
        }

        Debug.Log("공격 모드: 목표를 찍으세요");
    }
}

