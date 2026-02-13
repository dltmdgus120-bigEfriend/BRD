using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; //  마우스 이벤트 필수
using UnityEngine.Events;

// 마우스 올리기(Enter), 내리기(Exit), 클릭(Click)을 감지하는 인터페이스 추가
public class CommandSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Button button;
    public Image iconImage;
    public Image lockIcon; //  재료 부족할 때 띄울 자물쇠 아이콘 (반투명 검은 이미지 등)

    // 데이터 저장용
    private string myTitle;
    private string myDesc;
    private bool isLocked = false;
    private UnityAction myAction; // 실행할 함수

    // 초기화 (제목, 설명, 잠김 여부 추가됨)
    public void Setup(Sprite icon, string title, string desc, bool locked, UnityAction onClickAction)
    {
        gameObject.SetActive(true);

        // 아이콘 설정
        if (iconImage != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = icon;
            // 잠겨있으면 색을 어둡게, 아니면 밝게
            iconImage.color = locked ? Color.gray : Color.white;
        }

        // 잠김 표시 (자물쇠 아이콘이 있다면)
        if (lockIcon != null) lockIcon.enabled = locked;

        // 데이터 저장
        myTitle = title;
        myDesc = desc;
        isLocked = locked;
        myAction = onClickAction;

        // 버튼 컴포넌트의 기본 클릭 기능은 끕니다. (우리가 직접 제어할 거니까요)
        button.interactable = true; // 모양은 켜두되
        button.onClick.RemoveAllListeners(); // 자동 연결은 해제
    }

    public void Clear()
    {
        // 빈칸 만들기
        gameObject.SetActive(true);
        if (iconImage != null) iconImage.enabled = false;
        if (lockIcon != null) lockIcon.enabled = false;
        button.interactable = false;

        myTitle = "";
        myDesc = "";
        isLocked = false;
        myAction = null;
    }

    // --- 마우스 이벤트 ---

    // 1. 마우스가 버튼 위에 올라왔을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 내용이 있을 때만 툴팁 표시
        if (!string.IsNullOrEmpty(myTitle))
        {
            TooltipManager.Instance.ShowTooltip(myTitle, myDesc);
        }
    }

    // 2. 마우스가 버튼에서 나갔을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.Instance.HideTooltip();
    }

    // 3. 버튼을 클릭했을 때 (가장 중요 ★)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(myTitle)) return; // 빈칸이면 무시

        if (isLocked)
        {
            // 잠겨있으면 경고 메시지 & 경고음
            TooltipManager.Instance.ShowWarning("재료가 부족합니다!");
            // SoundManager.Instance.PlaySFX(errorSound); // 나중에 추가
            Debug.Log("재료 부족!");
        }
        else
        {
            // 잠겨있지 않으면 원래 기능 실행
            if (myAction != null) myAction.Invoke();
        }
    }
}