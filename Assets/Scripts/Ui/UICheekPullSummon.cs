using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Animator))]
public class UICheekPullSummon : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private enum State { Idle, Pulling, Snapping }
    private State currentState = State.Idle;

    [Header("UI 설정")]
    public float maxPullDistance = 200f;
    public float summonThreshold = 0.7f;

    // 새로 구하신 튕겨나가는(Snap) 애니메이션의 실제 재생 길이를 적어주세요! (예: 1.5초면 1.5)
    public float snapAnimDuration = 1.0f;

    [Header("효과")]
    public AudioClip pullSound;
    public AudioClip snapSound;

    private Animator anim;
    private AudioSource audioSource;
    private Vector2 startMousePos;

    void Start()
    {
        anim = GetComponent<Animator>();

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.clip = pullSound;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (currentState == State.Snapping) return;

        currentState = State.Pulling;
        startMousePos = eventData.position;

        //  [마법의 코드 1] 애니메이션 시간의 흐름을 0(정지)으로 만듭니다!
        anim.speed = 0f;

        // "Anim_Pull" 이라는 이름의 애니메이션을 맨 앞(0f)부터 시작 대기!
        anim.Play("Anim_Bol_Pull", 0, 0f);

        if (pullSound != null) audioSource.Play();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentState != State.Pulling) return;

        // 오직 왼쪽으로 당긴 거리만 계산 (오른쪽으로 가면 0이 되어 덜 당겨짐)
        float pullDistance = Mathf.Max(0, startMousePos.x - eventData.position.x);
        float pullRatio = Mathf.Clamp01(pullDistance / maxPullDistance);

        // ★ [마법의 코드 2] 마우스를 당긴 비율(0.0 ~ 1.0)에 맞춰 애니메이션의 프레임을 강제로 이동시킵니다!
        // 왼쪽으로 쭉 당기면 애니가 재생되고, 다시 오른쪽으로 밀면 애니가 역재생되는 완벽한 쫀득함!
        anim.Play("Anim_Bol_Pull", 0, pullRatio);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (currentState != State.Pulling) return;

        audioSource.Stop();

        float pullDistance = Mathf.Max(0, startMousePos.x - eventData.position.x);
        float pullRatio = Mathf.Clamp01(pullDistance / maxPullDistance);

        bool isSuccess = pullRatio >= summonThreshold;

        // 마우스를 놓았으니 애니메이션 시간의 흐름을 다시 1배속으로 정상화!
        anim.speed = 1f;

        if (isSuccess)
        {
            StartCoroutine(SnapRoutine());
        }
        else
        {
            // 덜 당겼으면 스르륵 원래대로(조는 상태) 복귀
            ResetFace();
        }
    }

    private IEnumerator SnapRoutine()
    {
        currentState = State.Snapping;

        if (snapSound != null && SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(snapSound);

        // ★ "탱!" 하고 뒤로 튕겨져 나가면서 아파하는 애니메이션 발동!
        anim.SetTrigger("Snap");

        // SummonManager 소환 호출!
        if (SummonManager.Instance != null)
        {
            SummonManager.Instance.OnClickSummon();
        }
        else
        {
            Debug.LogError("SummonManager가 없습니다!");
        }

        // 튕겨나가는 애니메이션이 끝날 때까지 대기
        yield return new WaitForSeconds(snapAnimDuration);

        // 다 끝났으면 다시 졸기 시작
        ResetFace();
    }

    private void ResetFace()
    {
        currentState = State.Idle;
        anim.speed = 1f; // 혹시 모르니 1배속 확실히 고정
        anim.Play("Anim_Bol_Idle"); // 다시 꾸벅꾸벅 조는 애니메이션으로!
    }
}