using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [HideInInspector]
    public float speed;
    private Transform[] waypoints;
    private int wavepointIndex = 0;

    private Animator anim;
    private Vector3 lastPosition; // 움직였는지 확인용


    void Start()
    {
        //보통 스프라이트는 자식 오브젝트에 있는 경우가 많으므로 Children으로 찾습니다.
        anim = GetComponentInChildren<Animator>();
        lastPosition = transform.position;
    }

    public void Setup(Transform[] path)
    {
        waypoints = path;
        transform.position = waypoints[0].position;

        // 풀링에서 꺼내올 때마다 "너의 다음 목표는 무조건 1번이야!" 라고 뇌를 씻어줍니다!
        // (0번은 방금 스폰된 위치니까 1번으로 출발해야 합니다)
        wavepointIndex = 1;
    }

    void Update()
    {
        // 웨이포인트가 없거나 배열을 초과하면 에러 방지
        if (waypoints == null || wavepointIndex >= waypoints.Length) return;

        Transform target = waypoints[wavepointIndex];
        Vector3 dir = target.position - transform.position;

        // 이번 1프레임 동안 내가 이동할 실제 거리 계산
        float distanceThisFrame = speed * Time.deltaTime;

        //  목표까지 남은 거리가 내가 지금 움직일 거리보다 짧거나 같으면? (즉, 이번 프레임에 도착한다면)
        if (dir.magnitude <= distanceThisFrame)
        {
            // 목표 지점을 뚫고 지나가지 않게 정확히 목표 지점에 강제로 안착시킵니다!
            transform.position = target.position;
            GetNextWaypoint();
        }
        else
        {
            // 아직 멀었으면 원래대로 이동
            transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        }

        // ★ 애니메이션 처리
        if (anim != null)
        {
            // 현재 위치와 아까 위치가 다르면 -> 움직이는 중!
            bool isMoving = (transform.position - lastPosition).sqrMagnitude > 0.0001f;
            anim.SetBool("IsMoving", isMoving);       
        }

        lastPosition = transform.position; // 위치 갱신
    }

    void GetNextWaypoint()
    {
        wavepointIndex++;

        // 마지막 지점을 넘어가면?
        if (wavepointIndex >= waypoints.Length)
        {
            // 1. 인덱스 초기화 (처음으로)
            wavepointIndex = 0;

            // 2. 위치도 시작점으로 강제 이동        
            transform.position = waypoints[0].position;
        }
    }
}