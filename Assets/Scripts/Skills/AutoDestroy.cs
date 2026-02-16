using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float delay = 2.0f; // 2초 뒤에 사라짐

    void Start()
    {
        // 파티클 시스템이 있다면, 파티클이 끝나는 시간을 자동으로 계산
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps != null)
        {
            Destroy(gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        }
        else
        {
            // 파티클이 없으면 그냥 설정한 시간 뒤에 삭제
            Destroy(gameObject, delay);
        }
    }
}
