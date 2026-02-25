using UnityEngine;
using UnityEngine.EventSystems; 
using System.Collections;

// IPointerEnter(마우스 올림), IPointerExit(마우스 나감), IPointerClick(클릭) 인터페이스를 상속받습니다.
public class UIButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("사운드 설정")]
    public AudioClip hoverSound; // 마우스 올렸을 때 날 소리 (선택사항)
    public AudioClip clickSound; // 클릭했을 때 날 소리

    [Header("애니메이션 설정")]
    public float hoverScaleMultiplier = 1.1f; // 마우스를 올리면 1.1배 커짐
    public float animationSpeed = 0.1f;       // 커지거나 작아지는 데 걸리는 시간

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    void Start()
    {
        // 시작할 때 내 원래 크기를 기억해둡니다.
        originalScale = transform.localScale;
    }

    // 1. 마우스가 버튼 위에 올라왔을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 호버 사운드 재생
        if (hoverSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(hoverSound);
        }

        // 버튼 키우기 (1.1배)
        Vector3 targetScale = originalScale * hoverScaleMultiplier;
        ChangeScale(targetScale);
    }

    // 2. 마우스가 버튼에서 나갔을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        // 원래 크기로 복구
        ChangeScale(originalScale);
    }

    // 3. 버튼을 클릭했을 때
    public void OnPointerClick(PointerEventData eventData)
    {
        // 클릭 사운드 재생
        if (clickSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(clickSound);
        }

        // 클릭하는 순간 살짝 찌그러지는(눌리는) 효과를 주고 싶다면 여기서 스케일을 조절해도 좋습니다!
    }

    // 부드럽게 크기를 키우거나 줄이는 함수
    private void ChangeScale(Vector3 targetScale)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleRoutine(targetScale));
    }

    private IEnumerator ScaleRoutine(Vector3 targetScale)
    {
        Vector3 currentScale = transform.localScale;
        float percent = 0f;

        while (percent < 1f)
        {
            //  unscaledDeltaTime을 써야 게임이 일시정지(TimeScale=0) 상태일 때도 UI가 움직입니다!
            percent += Time.unscaledDeltaTime / animationSpeed;
            transform.localScale = Vector3.Lerp(currentScale, targetScale, percent);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    // 버튼이 꺼질 때 크기가 커진 채로 굳어버리는 버그 방지
    void OnDisable()
    {
        transform.localScale = originalScale;
    }
}