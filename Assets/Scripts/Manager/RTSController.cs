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
    public List<NavMeshAgent> selectedUnits = new List<NavMeshAgent>();
    private Vector2 startPos;
    private bool isDragging = false;

    void Update()
    {
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
        if (Input.GetMouseButtonDown(1))
        {
            MoveSelectedUnits();
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
}