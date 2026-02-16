using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;


public class CommandSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 연결")]
    public Button button;
    public Image iconImage;
    public Image lockIcon;

    // 내부 데이터
    private string myTitle;
    private string myDesc;
    private bool isLocked = false;
    private string warningMessage;

    public void Setup(Sprite icon, string title, string desc, bool locked, UnityAction onClickAction, string lockedMessage = "사용할 수 없습니다.")
    {
        gameObject.SetActive(true);

        // 1. 아이콘 설정
        if (iconImage != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = icon;
            iconImage.color = locked ? Color.gray : Color.white;
        }

        if (lockIcon != null) lockIcon.enabled = locked;

        myTitle = title;
        myDesc = desc;
        isLocked = locked;
        warningMessage = lockedMessage;

        // 4. 버튼 초기화 
        if (button != null)
        {
            button.interactable = true;
            button.onClick.RemoveAllListeners(); // 기존 연결 삭제

            // ★ 버튼에 직접 기능 연결 (OnPointerClick 대신 이걸 씀)
            button.onClick.AddListener(() =>
            {
                HandleClick(onClickAction);
            });
        }
    }

    // ★ 클릭 처리 로직을 별도 함수로 분리
    void HandleClick(UnityAction action)
    {
        // 1. 함수가 호출되는지 확인
        Debug.Log($"[클릭 감지] 제목: {myTitle}, 잠김여부: {isLocked}");

        if (string.IsNullOrEmpty(myTitle))
        {
            Debug.LogError("오류: 스킬 이름(Title)이 비어있습니다!"); // 이게 뜨는지 확인
            return;
        }

        if (isLocked)
        {
            Debug.Log($"[잠김] {warningMessage}");
        }
        else
        {
            // 2. 액션이 연결되어 있는지 확인
            if (action == null) Debug.LogError("오류: 실행할 Action이 연결되지 않았습니다!");
            else
            {
                Debug.Log("--> 스킬 실행 신호 보냄!");
                action.Invoke();
            }
        }
    }

    public void Clear()
    {
        myTitle = "";
        myDesc = "";
        isLocked = false;
        warningMessage = "";

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
        if (lockIcon != null) lockIcon.enabled = false;

        if (button != null)
        {
            button.onClick.RemoveAllListeners(); // 연결 끊기
            button.interactable = false;
        }
    }

    // --- 마우스 이벤트 (툴팁용) ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!string.IsNullOrEmpty(myTitle) && TooltipManager.Instance != null)
        {
            TooltipManager.Instance.ShowTooltip(myTitle, myDesc);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }

    
}