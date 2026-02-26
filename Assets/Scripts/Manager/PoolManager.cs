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

    [Header("아군 풀링 설정")]
    public GameObject allyPrefab; // 아군 공용 껍데기 프리팹
    public int allyPoolSize = 100; // 넉넉하게 100개

    [Header("--- 투사체 풀 (자동 관리) ---")]
    // 프리팹 이름을 Key로 사용하는 스마트 풀 (사전 형태)
    private Dictionary<string, Queue<GameObject>> projectilePools = new Dictionary<string, Queue<GameObject>>();

    [Header("이펙트(VFX) 풀링 설정")]
    public GameObject vfxPrefab;   // 사망 파티클/이펙트 프리팹
    public int vfxPoolSize = 20;   // 2초면 사라지므로 20개면 충분함

    [Header("데미지 팝업 설정")]
    public GameObject damagePopupPrefab;
    public int damagePopupPoolSize = 50;

    // 대기실 역할을 할 큐(Queue) 자료구조
    private Queue<GameObject> enemyPool = new Queue<GameObject>();
    private Queue<GameObject> vfxPool = new Queue<GameObject>();
    private Queue<GameObject> allyPool = new Queue<GameObject>();
    private Queue<GameObject> damagePopupPool = new Queue<GameObject>();

    void Awake()
    {
        Instance = this;
        InitializePools();
    }

    // 게임 시작 시 미리 정해진 개수만큼 만들어서 큐에 집어넣습니다.
    void InitializePools()
    {
        //  적 풀 생성
        for (int i = 0; i < enemyPoolSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab, transform);
            enemy.SetActive(false);
            enemyPool.Enqueue(enemy);
        }

        //  VFX 풀 생성
        for (int i = 0; i < vfxPoolSize; i++)
        {
            GameObject vfx = Instantiate(vfxPrefab, transform);
            vfx.SetActive(false);
            vfxPool.Enqueue(vfx);
        }

        // 아군 풀 생성
        for (int i = 0; i < allyPoolSize; i++)
        {
            GameObject ally = Instantiate(allyPrefab, transform);
            ally.SetActive(false);
            allyPool.Enqueue(ally);
        }

        for (int i = 0; i < damagePopupPoolSize; i++)
        {
            GameObject popup = Instantiate(damagePopupPrefab, transform);
            popup.SetActive(false);
            damagePopupPool.Enqueue(popup);
        }
    }

    public GameObject GetAlly(Vector3 position)
    {
        if (allyPool.Count > 0)
        {
            GameObject ally = allyPool.Dequeue();
            ally.transform.position = position;
            ally.SetActive(true);
            return ally;
        }
        return Instantiate(allyPrefab, position, Quaternion.identity);
    }

    public void ReturnAlly(GameObject ally)
    {
        ally.SetActive(false);
        allyPool.Enqueue(ally);
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
        // [방어막 추가] 이미 꺼져있으면 무시!
        if (!enemy.activeSelf) return;

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

    //  투사체 꺼내기 (종류 불문 다 받아줌!)
    public GameObject GetProjectile(GameObject prefab, Vector3 position)
    {
        string key = prefab.name; // 예: "Arrow_Prefab"

        // 처음 보는 투사체면 대기실을 새로 하나 파줍니다.
        if (!projectilePools.ContainsKey(key))
        {
            projectilePools[key] = new Queue<GameObject>();
        }

        // 대기실에 남은 투사체가 있으면 꺼내줍니다.
        if (projectilePools[key].Count > 0)
        {
            GameObject proj = projectilePools[key].Dequeue();
            proj.transform.position = position;
            proj.SetActive(true);
            return proj;
        }

        // 대기실이 비었으면 새로 만들어서 줍니다!
        GameObject newProj = Instantiate(prefab, position, Quaternion.identity);
        newProj.name = prefab.name; //  "(Clone)" 글자가 붙지 않게 원본 이름 유지 (아주 중요!)
        return newProj;
    }

    //  투사체 집어넣기
  public void ReturnProjectile(GameObject proj)
    {
        // [방어막 추가] 이미 꺼져있으면 무시! (투사체 증발 버그의 원인)
        if (!proj.activeSelf) return; 

        proj.SetActive(false);

        if (!projectilePools.ContainsKey(proj.name))
        {
            projectilePools[proj.name] = new Queue<GameObject>();
        }
        projectilePools[proj.name].Enqueue(proj);
    }

    // 강타(Proc) 스킬이나 치명타가 터졌을 때 부를 함수
    public void ShowDamagePopup(Vector3 position, int damageAmount, AttackType type, bool isCrit = false)
    {
        GameObject popupObj = null;

        if (damagePopupPool.Count > 0)
        {
            popupObj = damagePopupPool.Dequeue();
        }
        else
        {
            popupObj = Instantiate(damagePopupPrefab, transform);
        }

        popupObj.transform.position = position;
        popupObj.SetActive(true);

        DamagePopup popupScript = popupObj.GetComponent<DamagePopup>();
        if (popupScript != null)
        {
            // 아까 만든 DamagePopup 스크립트의 Setup 함수 실행!
            popupScript.Setup(damageAmount, type, isCrit);
        }
    }

    // 팝업 텍스트가 수명을 다하면 다시 대기실로 돌아가는 함수
    public void ReturnDamagePopup(GameObject popup)
    {
        // [방어막 추가] 
        if (!popup.activeSelf) return;

        popup.SetActive(false);
        damagePopupPool.Enqueue(popup);
    }
}