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

    private Camera mainCam;  // 매 프레임 카메라를 찾으면 렉이 걸리니 저장해 둘 변수

    void Start()
    {
        // 부모(Root)에 있는 컴포넌트들을 자동으로 찾아옵니다.
        agent = GetComponent<NavMeshAgent>();
        attack = GetComponent<UnitAttack>();

        // 만약 인스펙터에 연결 안 했으면 자식에서 찾기
        if (modelRenderer == null)
            modelRenderer = GetComponentInChildren<SpriteRenderer>();

        mainCam = Camera.main;
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
        // 나와 타겟 사이의 방향 벡터
        Vector3 dir = targetPos - transform.position;

        // 카메라의 '오른쪽(right)' 방향과 타겟 방향을 비교(Dot)합니다!
        // 결과가 양수(+)면 타겟이 카메라 화면상 내 오른쪽에 있는 것이고,
        // 결과가 음수(-)면 타겟이 카메라 화면상 내 왼쪽에 있는 것입니다!
        float rightDot = Vector3.Dot(dir, mainCam.transform.right);

        // 깜빡임 방지 데드존: 타겟이 내 몸통 한가운데랑 거의 겹쳐있을 때는 무시 (0.05 기준)
        if (Mathf.Abs(rightDot) < 0.05f) return;

        // 양수면 오른쪽!
        bool isRightSide = rightDot > 0;

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