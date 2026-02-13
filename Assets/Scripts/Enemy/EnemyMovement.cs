using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("설정")]
    public float speed = 5f; // 이동 속도
    public int damage = 1;   // 플레이어에게 입히는 데미지

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
        // 다음 지점으로 인덱스 증가
        wavepointIndex++;

        // 3. 더 이상 갈 곳이 없다면? (한 바퀴 돔) -> 플레이어 피 깎고 사망
        if (wavepointIndex >= waypoints.Length)
        {
            DefenseManager.Instance.TakeDamage(damage); // 매니저에게 데미지 전달
            Destroy(gameObject); // 자폭
            return;
        }
    }
}