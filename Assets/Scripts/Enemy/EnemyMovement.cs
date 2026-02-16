using UnityEngine;


public class EnemyMovement : MonoBehaviour
{
    [Header("설정")]
    public float speed = 5f; // 이동 속도
    public int damage = 1;   // 플레이어에게 입히는 데미지

    [Header("보스 전용 설정")]
    public bool isLooping = false; // ★ 체크하면 도착 지점에서 죽지 않고 순환함

    private Transform[] waypoints; // 가야 할 길 목록
    private int wavepointIndex = 0; // 현재 목표 지점 번호



    // 스포너(매니저)가 소환하자마자 길을 알려주는 함수
    public void Setup(Transform[] path)
    {
        waypoints = path;
        transform.position = waypoints[0].position; // 시작점으로 이동
    }

    void Update()
    {
        if (waypoints == null) return;

        // 1. 현재 목표 지점을 향해 이동
        Transform target = waypoints[wavepointIndex];
        Vector3 dir = target.position - transform.position;
        transform.Translate(dir.normalized * speed * Time.deltaTime, Space.World);

        // 2. 목표에 거의 도착했는지 확인 (거리 0.2 이하)
        if (Vector3.Distance(transform.position, target.position) <= 0.2f)
        {
            GetNextWaypoint();
        }

        // (선택) 적도 빌보드(카메라 보기)가 필요하면 아까 만든 Billboard 스크립트를 붙이세요!
    }

    void GetNextWaypoint()
    {
        wavepointIndex++;

        // 마지막 지점을 넘어섰을 때
        if (wavepointIndex >= waypoints.Length)
        {
            if (isLooping)
            {
                // ★ 보스: 인덱스를 0으로 초기화하여 다시 첫 번째 웨이포인트로 이동 (순환)
                wavepointIndex = 0;
            }
            else
            {
                // ★ 일반 몹: 플레이어 데미지 입히고 자폭
                DefenseManager.Instance.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}