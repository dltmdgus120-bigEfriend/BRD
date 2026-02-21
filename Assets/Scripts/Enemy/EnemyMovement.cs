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
    }

    void Update()
    {
        if (waypoints == null) return;

        Transform target = waypoints[wavepointIndex];
        Vector3 dir = target.position - transform.position;
        transform.Translate(dir.normalized * speed * Time.deltaTime, Space.World);

        if (Vector3.Distance(transform.position, target.position) <= 0.2f)
        {
            GetNextWaypoint();
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