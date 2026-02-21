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
            // 깜빡임 방지: 속도의 X축 방향이 아주 미세할 때는 덜덜거리지 않게 무시!
            if (Mathf.Abs(agent.velocity.x) > 0.05f)
            {
                LookAt(transform.position + agent.velocity);
            }
        }
    }

    void LookAt(Vector3 targetPos)
    {
        // 깜빡임 방지 데드존: 나와 타겟의 X 좌표 차이를 계산
        float diffX = targetPos.x - transform.position.x;

        // 타겟과의 거리가 쥐똥만 할 때(0.05 이하)는 방향을 바꾸지 않고 무시합니다!
        if (Mathf.Abs(diffX) < 0.05f) return;

        bool isRightSide = diffX > 0;

        if (isOriginalFacingRight)
        {
            modelRenderer.flipX = !isRightSide;
        }
        else
        {
            modelRenderer.flipX = isRightSide;
        }
    }
}