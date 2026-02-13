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

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
        if (warningText != null) warningText.gameObject.SetActive(false);
    }

    void Update()
    {
        // 툴팁이 마우스를 따라다니게 하기
        if (tooltipPanel.activeSelf)
        {
            // 마우스 위치에서 살짝 오른쪽 아래에 표시
            tooltipPanel.transform.position = Input.mousePosition + new Vector3(15, -15, 0);
        }
    }

    public void ShowTooltip(string title, string desc)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
            titleText.text = title;
            descText.text = desc;
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