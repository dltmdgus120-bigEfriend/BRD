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
    public LayerMask groundLayer;

    // 내부 변수
    public List<UnityEngine.AI.NavMeshAgent> selectedUnits = new List<UnityEngine.AI.NavMeshAgent>();
    private Vector2 startPos;
    private bool isDragging = false;

    [Header("커서 설정")]
    public Texture2D attackCursor; // (선택) 공격 모양 커서
    public bool isAttackCommand = false; // ★ 공격 명령 대기 중인가?

    void Update()
    {
        // 1. 공격 명령 대기 상태일 때 클릭 처리
        if (isAttackCommand)
        {
            if (Input.GetMouseButtonDown(0)) // 좌클릭
            {
                PerformAttackCommand();
                return; // 드래그 선택 로직 실행 안 되게 종료
            }
            else if (Input.GetMouseButtonDown(1)) // 우클릭 (취소)
            {
                isAttackCommand = false;
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // 커서 초기화
            }
            return; // 공격 모드일 땐 드래그 선택 막기
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        // 1. 마우스 클릭 (드래그 시작)
        if (Input.GetMouseButtonDown(0))
        {
            StartSelection();
        }

        // 2. 마우스 누르는 중 (박스 그리기)
        if (Input.GetMouseButton(0) && isDragging)
        {
            UpdateSelectionBox();
        }

        // 3. 마우스 뗌 (선택 확정)
        if (Input.GetMouseButtonUp(0))
        {
            EndSelection();
        }

        // 4. 우클릭 (이동)
        if (Input.GetMouseButtonDown(1) && selectedUnits.Count > 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                foreach (var agent in selectedUnits)
                {
                    var attack = agent.GetComponent<UnitAttack>();
                    if (attack != null)
                    {
                        //OrderMove 함수를 통해 "정지 상태 해제"까지 같이 처리
                        attack.OrderMove(hit.point);
                    }
                    else
                    {
                        // UnitAttack이 없는 유닛일 경우 대비
                        agent.SetDestination(hit.point);
                    }
                }
            }
        }
    }

    void StartSelection()
    {
        // 기존 선택 초기화 (Shift 키 안 누른 상태라면)
        if (!Input.GetKey(KeyCode.LeftShift))
        {
            DeselectAll();
        }

        startPos = Input.mousePosition;
        isDragging = true;
        selectionBox.gameObject.SetActive(true);
    }

    void UpdateSelectionBox()
    {
        Vector2 currentPos = Input.mousePosition;

        float width = Mathf.Abs(currentPos.x - startPos.x);
        float height = Mathf.Abs(currentPos.y - startPos.y);

        // 피벗이 (0,0) 좌하단일 때의 계산법
        float x = Mathf.Min(startPos.x, currentPos.x);
        float y = Mathf.Min(startPos.y, currentPos.y);

        selectionBox.anchoredPosition = new Vector2(x, y);
        selectionBox.sizeDelta = new Vector2(width, height);
    }

    void EndSelection()
    {
        isDragging = false;
        selectionBox.gameObject.SetActive(false);

        // 박스가 너무 작으면(단순 클릭) -> 클릭 선택 로직으로
        if (selectionBox.sizeDelta.magnitude < 10)
        {
            SelectSingleUnit();
            return;
        }

        SelectUnitsInBox();
    }

    void SelectSingleUnit()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, unitLayer))
        {
            NavMeshAgent agent = hit.collider.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                AddUnitToSelection(agent); // 유닛 추가
            }
        }
        else
        {
            DeselectAll(); // 빈 땅 클릭하면 선택 해제
        }
    }

    // ★ 핵심 수정: 드래그 범위 안의 유닛 찾기
    void SelectUnitsInBox()
    {
        // 화면에 있는 모든 유닛 가져오기
        NavMeshAgent[] allUnits = FindObjectsOfType<NavMeshAgent>();

        // 현재 드래그 박스의 범위 계산 (Min: 좌하단, Max: 우상단)
        Vector2 min = selectionBox.anchoredPosition;
        Vector2 max = selectionBox.anchoredPosition + selectionBox.sizeDelta;

        foreach (NavMeshAgent unit in allUnits)
        {
            // 유닛의 월드 좌표를 -> 화면(Screen) 좌표로 변환
            Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);

            // 화면 좌표가 박스 범위 안에 있는지 검사
            if (screenPos.x > min.x && screenPos.x < max.x &&
                screenPos.y > min.y && screenPos.y < max.y)
            {
                AddUnitToSelection(unit);
            }
        }
        Debug.Log(selectedUnits.Count + "마리 선택됨");
    }

    // ★ 핵심 수정: 뭉침 방지 이동 로직
    void MoveSelectedUnits()
    {
        if (selectedUnits.Count == 0) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            // 여러 마리가 이동할 때 약간씩 퍼지게 만들기
            foreach (NavMeshAgent unit in selectedUnits)
            {
                // 랜덤한 원 안의 좌표를 구해서 더해줌 (반경 1.5 ~ 2.0 정도)
                Vector2 randomOffset = Random.insideUnitCircle * 2.0f;
                Vector3 dest = hit.point + new Vector3(randomOffset.x, 0, randomOffset.y);

                unit.SetDestination(dest);
            }
        }
    }

    // 유닛 선택 처리 (나중에 이펙트 켜는 곳)
    void AddUnitToSelection(NavMeshAgent unit)
    {
        if (!selectedUnits.Contains(unit))
        {
            selectedUnits.Add(unit);
            // TODO: 여기서 유닛 발밑의 초록색 원을 켜주세요!
            // unit.GetComponent<UnitScript>().ShowCircle(true); 
        }
    }

    // 선택 해제 처리
    void DeselectAll()
    {
        foreach (var unit in selectedUnits)
        {
            // TODO: 여기서 유닛 발밑의 초록색 원을 꺼주세요!
        }
        selectedUnits.Clear();
    }

    public void ClearSelection()
    {
        DeselectAll(); // 기존에 있던 함수 활용
    }

    void PerformAttackCommand()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // 1. 적을 직접 클릭했나? (일점사)
            EnemyHP enemy = hit.collider.GetComponent<EnemyHP>();
            if (enemy != null)
            {
                foreach (var agent in selectedUnits)
                {
                    var attack = agent.GetComponent<UnitAttack>();
                    if (attack != null)
                    {
                        attack.isAttackMoving = false;
                        attack.target = enemy.transform; // 강제 타겟 지정
                    }
                    agent.SetDestination(enemy.transform.position); // 적 위치로 이동
                }
            }
            // 2. 땅을 클릭했나? (어택 땅)
            else
            {
                foreach (var agent in selectedUnits)
                {
                    var attack = agent.GetComponent<UnitAttack>();
                    if (attack != null)
                    {
                        attack.OrderAttackMove(hit.point); // ★ 어택 땅 명령!
                    }
                }
            }
        }

        // 명령 끝났으니 초기화
        isAttackCommand = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    // 외부에서 호출할 공격 모드 진입 함수
    public void EnterAttackMode()
    {
        isAttackCommand = true;
        Cursor.SetCursor(attackCursor, Vector2.zero, CursorMode.Auto);
        Debug.Log("공격 모드: 목표를 찍으세요");
    }
}

