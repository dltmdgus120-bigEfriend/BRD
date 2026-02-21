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
    }
}