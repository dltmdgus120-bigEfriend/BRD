using UnityEngine;
using UnityEngine.SceneManagement; 

public class GameMenu : MonoBehaviour
{
    [Header("패널 연결")]
    public GameObject menuPanel;   // ESC 누르면 뜰 메뉴창
    public GameObject optionPanel; // 옵션 버튼 누르면 뜰 창
    public GameObject blockerPanel;

    [Header("가이드 패널 설정")]
    public GameObject guidePanel;  // 가이드 패널 전체 (배경 포함)
    public GameObject[] guidePages; // 가이드 내용이 들어갈 페이지(이미지)들
    private int currentPageIndex = 0;

    private bool isPaused = false; // 현재 멈췄는지 체크

    void Start()
    {
        // 시작할 때 UI들이 켜져 있다면 깔끔하게 다 끄고 시작
        if (blockerPanel != null) blockerPanel.SetActive(false);
        if (menuPanel != null) menuPanel.SetActive(false);
        if (optionPanel != null) optionPanel.SetActive(false);
        if (guidePanel != null) guidePanel.SetActive(false);
    }

    void Update()
    {
        //게임 오버 상태라면 ESC 입력을 아예 무시하고 함수 종료!
        if (DefenseManager.Instance != null && DefenseManager.Instance.isGameOver)
        {
            return;
        }

        // ESC 키 입력 감지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame(); // 이미 열려있으면 닫기
            }
            else
            {
                PauseGame(); // 닫혀있으면 열기
            }
        }
    }

    // 메뉴 열기 (일시정지)
    public void PauseGame()
    {
        isPaused = true;

        if (blockerPanel != null) blockerPanel.SetActive(true); 
        if (menuPanel != null) menuPanel.SetActive(true);

        Time.timeScale = 0f; // 시간 정지
    }

    // 메뉴 닫기 (게임 재개)
    public void ResumeGame()
    {
        isPaused = false;

        if (blockerPanel != null) blockerPanel.SetActive(false); 
        if (menuPanel != null) menuPanel.SetActive(false);
        if (optionPanel != null) optionPanel.SetActive(false);
        if (guidePanel != null) guidePanel.SetActive(false);

        Time.timeScale = 1f; // 시간 다시 흐름
    }

    // 옵션 버튼 기능
    public void OnClickOption()
    {
        if (optionPanel != null) optionPanel.SetActive(true);
    }

    // 가이드 버튼
    public void OnClickGuide()
    {
        if (guidePanel != null)
        {
            guidePanel.SetActive(true);
            currentPageIndex = 0; // 항상 1페이지부터 보여주기
            UpdatePageVisibility();
        }
    }

    public void OnClickNextPage()
    {
        if (guidePages == null || guidePages.Length == 0) return;

        // 마지막 페이지가 아닐 때만 넘어감
        if (currentPageIndex < guidePages.Length - 1)
        {
            currentPageIndex++;
            UpdatePageVisibility();
        }
    }

    public void OnClickPreviousPage()
    {
        if (guidePages == null || guidePages.Length == 0) return;

        // 첫 페이지가 아닐 때만 넘어감
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdatePageVisibility();
        }
    }

    //  현재 인덱스에 맞춰 페이지들 켜고 끄기
    private void UpdatePageVisibility()
    {
        for (int i = 0; i < guidePages.Length; i++)
        {
            if (guidePages[i] != null)
            {
                // 현재 내 번호(i)가 currentPageIndex와 같으면 켜고(true), 다르면 끕니다(false).
                guidePages[i].SetActive(i == currentPageIndex);
            }
        }
    }

    public void OnClickCloseGuide()
    {
        if (guidePanel != null) guidePanel.SetActive(false);
    }

    // 타이틀로 가기 버튼 기능
    public void OnClickToTitle()
    {
        Time.timeScale = 1f; // 시간 정상화
        SceneManager.LoadScene("TitleScene"); // 타이틀 씬으로 이동
    }
}