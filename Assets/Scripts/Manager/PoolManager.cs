using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PoolManager : MonoBehaviour
{
    // 어디서든 쉽게 접근할 수 있도록 싱글톤 패턴 적용
    public static PoolManager Instance;

    [Header("적 풀링 설정")]
    public GameObject enemyPrefab; // 아까 만든 완벽한 껍데기 적 프리팹
    public int enemyPoolSize = 80; // 70마리 제한이므로 80개면 아주 넉넉함

    [Header("이펙트(VFX) 풀링 설정")]
    public GameObject vfxPrefab;   // 사망 파티클/이펙트 프리팹
    public int vfxPoolSize = 20;   // 2초면 사라지므로 20개면 충분함

    // 대기실 역할을 할 큐(Queue) 자료구조
    private Queue<GameObject> enemyPool = new Queue<GameObject>();
    private Queue<GameObject> vfxPool = new Queue<GameObject>();

    void Awake()
    {
        Instance = this;
        InitializePools();
    }

    // 게임 시작 시 미리 정해진 개수만큼 만들어서 큐에 집어넣습니다.
    void InitializePools()
    {
        // 1. 적 풀 생성
        for (int i = 0; i < enemyPoolSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab, transform);
            enemy.SetActive(false);
            enemyPool.Enqueue(enemy);
        }

        // 2. VFX 풀 생성
        for (int i = 0; i < vfxPoolSize; i++)
        {
            GameObject vfx = Instantiate(vfxPrefab, transform);
            vfx.SetActive(false);
            vfxPool.Enqueue(vfx);
        }
    }

    // 스포너가 적을 소환할 때 부를 함수
    public GameObject GetEnemy(Vector3 position)
    {
        if (enemyPool.Count > 0)
        {
            GameObject enemy = enemyPool.Dequeue();
            enemy.transform.position = position;
            enemy.SetActive(true);
            return enemy;
        }

        // 만약 80개를 다 썼는데 또 부른다면? (에러 방지용 비상 생성)
        GameObject newEnemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        return newEnemy;
    }

    // 적이 죽었을 때 대기실로 돌려보내는 함수
    public void ReturnEnemy(GameObject enemy)
    {
        enemy.SetActive(false);
        enemyPool.Enqueue(enemy);
    }

    //  적이 죽을 때 이펙트를 꺼내는 함수
    public void GetVFX(Vector3 position)
    {
        if (vfxPool.Count > 0)
        {
            GameObject vfx = vfxPool.Dequeue();
            vfx.transform.position = position;
            vfx.SetActive(true);

            // 이펙트는 2초 뒤에 알아서 대기실로 돌아가도록 코루틴 실행
            StartCoroutine(ReturnVFXRoutine(vfx, 2f));
        }
    }

    // 이펙트 자동 반환 타이머
    private IEnumerator ReturnVFXRoutine(GameObject vfx, float delay)
    {
        yield return new WaitForSeconds(delay);
        vfx.SetActive(false);
        vfxPool.Enqueue(vfx);
    }
}