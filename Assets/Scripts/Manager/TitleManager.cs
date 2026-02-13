using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동을 위해 필수!

public class TitleManager : MonoBehaviour
{
    [Header("설정")]
    public string gameSceneName = "GameScene"; // 이동할 게임 씬 이름
    public AudioClip titleBGM; // 타이틀 화면 전용 BGM

    [Header("UI 패널")]
    public GameObject optionPanel;
    public GameObject creditPanel;

    void Start()
    {
        // 타이틀 씬이 시작되자마자 타이틀 BGM 재생 요청
        if (SoundManager.Instance != null && titleBGM != null)
        {
            SoundManager.Instance.PlayBGM(titleBGM);
        }

        // 시작할 때 패널들은 꺼두기
        CloseAllPanels();
    }

    public void OnClickStart()
    {
        // 게임 씬으로 이동!
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnClickOption()
    {
        optionPanel.SetActive(true);
    }

    public void OnClickCredit()
    {
        creditPanel.SetActive(true);
    }

    public void OnClickExit()
    {
        Debug.Log("게임 종료");
        Application.Quit(); // 에디터에선 안 꺼지고 실제 빌드된 게임에서만 꺼짐
    }

    public void CloseAllPanels()
    {
        if (optionPanel != null) optionPanel.SetActive(false);
        if (creditPanel != null) creditPanel.SetActive(false);
    }
}