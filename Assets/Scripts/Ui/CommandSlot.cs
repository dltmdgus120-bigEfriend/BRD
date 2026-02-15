using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class CommandSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI 연결")]
    public Button button;
    public Image iconImage;
    public Image lockIcon; // 잠금 표시 (선택사항)

    // 내부 데이터
    private string myTitle;
    private string myDesc;
    private bool isLocked = false;
    private string warningMessage; // 잠긴 이유 (예: "재료 부족", "판매 불가")
    private UnityAction myAction;  // 실행할 함수

    // 초기화 함수 (경고 메시지 파라미터 추가됨)
    public void Setup(Sprite icon, string title, string desc, bool locked, UnityAction onClickAction, string lockedMessage = "사용할 수 없습니다.")
    {
        gameObject.SetActive(true);

        // 1. 아이콘 설정
        if (iconImage != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = icon;
            // 잠겨있으면 어둡게 (회색), 아니면 밝게
            iconImage.color = locked ? Color.gray : Color.white;
        }

        // 2. 잠금 아이콘 설정
        if (lockIcon != null) lockIcon.enabled = locked;

        // 3. 데이터 저장
        myTitle = title;
        myDesc = desc;
        isLocked = locked;
        myAction = onClickAction;
        warningMessage = lockedMessage; // ★ 잠긴 이유 저장

        // 4. 버튼 초기화
        // 중요: UI Button 컴포넌트의 기본 기능은 끄고, 우리가 만든 OnPointerClick을 씁니다.
        // (interactable을 false로 하면 OnPointerClick 이벤트도 안 먹히므로 true로 둡니다)
        if (button != null)
        {
            button.interactable = true;
            button.onClick.RemoveAllListeners(); // 기존 연결 해제 (중복 방지)
        }
    }

    // 슬롯 비우기 (초기화)
    public void Clear()
    {
        // 찌꺼기 데이터 제거
        myTitle = "";
        myDesc = "";
        isLocked = false;
        myAction = null;
        warningMessage = "";

        // UI 숨기기
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
        if (lockIcon != null) lockIcon.enabled = false;

        // 슬롯 자체를 안 보이게 해도 되고, 빈 껍데기로 둬도 됨 (여기선 비활성화)
        // gameObject.SetActive(false); // 레이아웃 유지를 위해 켜두려면 주석 처리

        // 버튼 기능 끄기
        if (button != null) button.interactable = false;
    }

    // --- 마우스 이벤트 ---

    // 1. 마우스 올렸을 때 (툴팁)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!string.IsNullOrEmpty(myTitle) && TooltipManager.Instance != null)
        {
            TooltipManager.Instance.ShowTooltip(myTitle, myDesc);
        }
    }

    // 2. 마우스 나갔을 때 (툴팁 끄기)
    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }

    // 3. 클릭했을 때 (실행 로직)
    public void OnPointerClick(PointerEventData eventData)
    {
        // 빈 슬롯이면 무시
        if (string.IsNullOrEmpty(myTitle)) return;

        if (isLocked)
        {
            // ★ 잠겨있으면, 저장해둔 '이유'를 띄움
            if (TooltipManager.Instance != null)
                TooltipManager.Instance.ShowWarning(warningMessage);

            Debug.Log($"[잠김] {warningMessage}");
        }
        else
        {
            // 잠겨있지 않으면 함수 실행
            if (myAction != null) myAction.Invoke();
        }
    }
}