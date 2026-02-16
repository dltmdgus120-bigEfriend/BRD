using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("설정")]
    public float speed = 5f;
    private Transform[] waypoints;
    private int wavepointIndex = 0;

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