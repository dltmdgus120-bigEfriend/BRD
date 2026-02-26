using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float delay = 2.0f; // 파티클이 없을 때의 기본 시간
    private float actualDelay;

    // OnEnable()을 써야 풀에서 꺼낼 때마다 실행됩니다.
    void OnEnable()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps != null)
        {
            actualDelay = ps.main.duration + ps.main.startLifetime.constantMax;
        }
        else
        {
            actualDelay = delay;
        }

        // Destroy 대신 Invoke를 써서 지정된 시간 뒤에 ReturnToPool 함수를 실행합니다.
        Invoke("ReturnToPool", actualDelay);
    }

    // 혹시나 시간이 되기 전에 다른 이유로 비활성화되면 Invoke를 취소해줍니다. (안전장치)
    void OnDisable()
    {
        CancelInvoke("ReturnToPool");
    }

    void ReturnToPool()
    {
        // 풀 매니저에게 반납!
        PoolManager.Instance.ReturnProjectile(gameObject);
    }
}
