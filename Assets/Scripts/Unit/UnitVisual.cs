using UnityEngine;
using UnityEngine.AI;

public class UnitVisual : MonoBehaviour
{
    [Header("연결 정보")]
    public SpriteRenderer modelRenderer; // 캐릭터 그림(자식 오브젝트)의 스프라이트 렌더러

    [Header("설정")]
    public bool isOriginalFacingRight = false; // 원본 그림이 오른쪽을 보고 있으면 체크!

    // 내부 컴포넌트 가져오기
    private NavMeshAgent agent;
    private UnitAttack attack;

    void Start()
    {
        // 부모(Root)에 있는 컴포넌트들을 자동으로 찾아옵니다.
        agent = GetComponent<NavMeshAgent>();
        attack = GetComponent<UnitAttack>();

        // 만약 인스펙터에 연결 안 했으면 자식에서 찾기
        if (modelRenderer == null)
            modelRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (modelRenderer == null) return;

        // 1순위: 공격 타겟이 있으면 타겟을 바라봄
        if (attack != null && attack.target != null)
        {
            LookAt(attack.target.position);
        }
        // 2순위: 이동 중이면 이동 방향을 바라봄
        else if (agent != null && agent.velocity.sqrMagnitude > 0.1f)
        {
            LookAt(transform.position + agent.velocity);
        }
    }

    void LookAt(Vector3 targetPos)
    {
        // 내 위치보다 타겟이 오른쪽에 있는가? (x 좌표 비교)
        bool isRightSide = targetPos.x > transform.position.x;

        // 원본 그림 방향에 따라 flipX 결정
        if (isOriginalFacingRight)
        {   
            // 원본이 오른쪽: 타겟이 왼쪽일 때 뒤집어야 함
            modelRenderer.flipX = !isRightSide;
        }
        else
        {
            // 원본이 왼쪽(보통): 타겟이 오른쪽일 때 뒤집어야 함
            modelRenderer.flipX = isRightSide;
        }
    }
}