using UnityEngine;

public class FallingMeteor : MonoBehaviour
{
    private Vector3 startPos;
    private Vector3 targetPos;
    private float duration;
    private float timer = 0f;

    // 매니저(SO)가 하늘에 빵을 생성할 때 이 함수로 목적지와 시간을 알려줍니다.
    public void Setup(Vector3 start, Vector3 target, float time)
    {
        startPos = start;
        targetPos = target;
        duration = time;

        //  대기실에서 꺼내 재사용할 때마다 타이머를 꼭 0으로 초기화해야 합니다!
        timer = 0f;
        transform.position = startPos;
    }

    void Update()
    {
        if (duration <= 0) return;

        // 타이머를 돌려서 0.0 ~ 1.0 (0% ~ 100%) 사이의 진행률을 구합니다.
        timer += Time.deltaTime;
        float percent = timer / duration;

        // 시작점과 도착점 사이를 진행률에 맞춰서 이동 (Lerp)
        transform.position = Vector3.Lerp(startPos, targetPos, percent);

        // 바닥(목표점)에 도달하면 스스로 대기실로 돌아갑니다.
        if (percent >= 1f)
        {
            PoolManager.Instance.ReturnProjectile(gameObject);
        }
    }
}