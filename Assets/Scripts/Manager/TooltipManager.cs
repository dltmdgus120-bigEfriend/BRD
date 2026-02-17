using UnityEngine;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [Header("UI 연결")]
    public GameObject tooltipPanel;
    public Text titleText;
    public Text descText;

    [Header("경고 메시지 (화면 중앙)")]
    public Text warningText; // "재료가 부족합니다!" 띄울 곳
    public float warningDuration = 1.5f;

    private RectTransform tooltipRect;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
        tooltipPanel.SetActive(false);        
        tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        if (warningText != null) warningText.gameObject.SetActive(false);
    }

    void Update()
    {
        // 툴팁 위치 및 피벗 자동 보정 로직
        if (tooltipPanel.activeSelf && tooltipRect != null)
        {
           
            Vector3 mousePos = Input.mousePosition;

            // 1. 기본 설정 (마우스의 "오른쪽 아래"에 표시)
            float pivotX = 0f;
            float pivotY = 1f;
            float offsetX = 15f;
            float offsetY = -15f;

            // 2. 화면 가로 체크: 마우스가 화면 오른쪽 절반을 넘어갔다면?
            if (mousePos.x > Screen.width * 0.5f)
            {
                pivotX = 1f;    // 왼쪽으로 뻗음
                offsetX = -15f;
            }

            // 3. 화면 세로 체크: 마우스가 화면 아래쪽 절반에 있다면?
            if (mousePos.y < Screen.height * 0.5f)
            {
                pivotY = 0f;   // 위쪽으로 뻗음
                offsetY = 15f;
            }

            // 4. 피벗 적용
            tooltipRect.pivot = new Vector2(pivotX, pivotY);
          
            tooltipPanel.transform.position = mousePos + new Vector3(offsetX, offsetY, 0);
        }
    }

    public void ShowTooltip(string title, string desc)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
            titleText.text = title;
            descText.text = desc;

            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
        }
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    // 화면 중앙에 경고 메시지 띄우기
    public void ShowWarning(string message)
    {
        if (warningText != null)
        {
            StopAllCoroutines(); // 기존 메시지 끄고
            StartCoroutine(WarningRoutine(message));
        }
    }

    System.Collections.IEnumerator WarningRoutine(string message)
    {
        warningText.gameObject.SetActive(true);
        warningText.text = message;
        // 띠링~ 소리 추가 가능 (SoundManager.Instance.PlaySFX(...))

        yield return new WaitForSeconds(warningDuration);

        warningText.gameObject.SetActive(false);
    }
}