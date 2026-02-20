using UnityEngine;
using UnityEngine.SceneManagement; 

public class GameMenu : MonoBehaviour
{
    [Header("패널 연결")]
    public GameObject menuPanel;   // ESC 누르면 뜰 메뉴창
    public GameObject optionPanel; // 옵션 버튼 누르면 뜰 창
    public GameObject blockerPanel;

    private bool isPaused = false; // 현재 멈췄는지 체크

    void Start()
    {
        // 시작할 때 UI들이 켜져 있다면 깔끔하게 다 끄고 시작
        if (blockerPanel != null) blockerPanel.SetActive(false);
        if (menuPanel != null) menuPanel.SetActive(false);
        if (optionPanel != null) optionPanel.SetActive(false);
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

        Time.timeScale = 1f; // 시간 다시 흐름
    }

    // 옵션 버튼 기능
    public void OnClickOption()
    {
        if (optionPanel != null) optionPanel.SetActive(true);
    }

    // 재시작 버튼 기능
    public void OnClickRestart()
    {
        Time.timeScale = 1f; // (중요) 시간 다시 흐르게 하고 리셋해야 함
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // 현재 씬 다시 로드
    }

    // 타이틀로 가기 버튼 기능
    public void OnClickToTitle()
    {
        Time.timeScale = 1f; // 시간 정상화
        SceneManager.LoadScene("TitleScene"); // 타이틀 씬으로 이동
    }
}