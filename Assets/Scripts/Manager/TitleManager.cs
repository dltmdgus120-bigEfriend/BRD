using System.Collections; 
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class TitleManager : MonoBehaviour
{
    [Header("설정")]
    public string gameSceneName = "GameScene"; 
    public AudioClip titleBGM;
    public AudioClip clickSound;

    [Header("캐릭터 음성 (보이스)")]
    public AudioClip startVoice;   
    public AudioClip optionVoice;  
    public AudioClip creditVoice;  

    [Header("로딩 설정")]
    public float minLoadingTime = 2.0f; // 최소 로딩 시간 

    [Header("UI 패널")]
    public GameObject optionPanel;
    public GameObject creditPanel;

    [Header("로딩 UI")]
    public GameObject loadingPanel; 
    public Slider loadingBar;       // 게이지 바
    public TMP_Text loadingText;        // 퍼센트 텍스트 

    [Header("팁 시스템")]
    public GameTipData tipData; 
    public TMP_Text tipText;

    [Header("로딩 연출")]
    public GameObject[] foods; 
    public float[] eatThresholds = { 0.25f, 0.5f, 0.75f }; // 먹는 타이밍 (25%, 50%, 75% 지점)

    void Start()
    {
        
        if (SoundManager.Instance != null && titleBGM != null)
        {
            SoundManager.Instance.PlayBGM(titleBGM);
        }

       
        CloseAllPanels();

        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    private void PlayClickSound()
    {
        if (clickSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(clickSound);
        }
    }

    private void PlayVoice(AudioClip voiceClip)
    {
        if (voiceClip != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(voiceClip);
        }
    }

    public void OnClickStart()
    {
        PlayClickSound();
        PlayVoice(startVoice);
        //  백그라운드 비동기 로딩 코루틴을 실행합니다!
        StartCoroutine(LoadGameSceneAsync());
    }

    public void OnClickOption()
    {
        PlayClickSound();
        PlayVoice(optionVoice);
        optionPanel.SetActive(true);
    }

    public void OnClickCredit()
    {
        PlayClickSound();
        PlayVoice(creditVoice);
        creditPanel.SetActive(true);
    }

    public void OnClickExit()
    {
        PlayClickSound();
        Debug.Log("게임 종료");
        Application.Quit(); // 에디터에선 안 꺼지고 실제 빌드된 게임에서만 꺼짐
    }

    public void CloseAllPanels()
    {
        
        if (optionPanel != null) optionPanel.SetActive(false);
        if (creditPanel != null) creditPanel.SetActive(false);
    }

    IEnumerator LoadGameSceneAsync()
    {
        
        CloseAllPanels();
        if (loadingPanel != null) loadingPanel.SetActive(true);

        // 로딩바와 텍스트 0으로 초기화
        if (loadingBar != null) loadingBar.value = 0f;
        if (loadingText != null) loadingText.text = "0%";

        // 로딩 시작 전, 음식들을 다시 화면에 보이게 초기화 (나갔다 들어올 때 대비)
        if (foods != null)
        {
            foreach (var food in foods)
            {
                if (food != null) food.SetActive(true);
            }
        }

        //팁 표시 
        if (tipData != null && tipData.tips.Count > 0 && tipText != null)
        {
            int randomIndex = Random.Range(0, tipData.tips.Count);
            tipText.text = "Tip - " + tipData.tips[randomIndex];
        }
     
        // 백그라운드에서 게임 씬을 불러오기 시작!
        AsyncOperation op = SceneManager.LoadSceneAsync(gameSceneName);

        // 씬 로딩이 100% 다 되어도 지 혼자 안 넘어가게 꽉 붙잡아둠!
        op.allowSceneActivation = false;

        float timer = 0f;

        // 진짜 로딩(op.progress)과 가짜 타이머(timer) 둘 다 완료될 때까지 대기
        while (timer < minLoadingTime || op.progress < 0.9f)
        {
            timer += Time.deltaTime;

            // 1. 시간상 몇 퍼센트 왔는지 (가짜 로딩)
            float timeProgress = timer / minLoadingTime;
            // 2. 실제 파일 로딩은 몇 퍼센트 왔는지 (진짜 로딩)
            float loadProgress = op.progress / 0.9f;

            // 둘 중 '더 느린 놈'을 기준으로 화면에 보여줌
            // (로딩이 0.1초만에 끝나도, timeProgress 때문에 천천히 2초동안 차오름!)
            float displayProgress = Mathf.Min(timeProgress, loadProgress);

            if (loadingBar != null) loadingBar.value = displayProgress;
            if (loadingText != null) loadingText.text = $"{(displayProgress * 100f):F0}%";

            // [먹방 로직] 현재 퍼센트가 음식 위치를 지나가면 냠냠! (SetActive 끄기)
            if (foods != null && eatThresholds != null)
            {
                for (int i = 0; i < foods.Length; i++)
                {
                    if (i < eatThresholds.Length && foods[i] != null && foods[i].activeSelf)
                    {
                        // 캐릭터가 해당 퍼센트를 넘어섰다면?
                        if (displayProgress >= eatThresholds[i])
                        {
                            foods[i].SetActive(false); // 이미지 숨기기 (먹음!)
                        }
                    }
                }
            }

            yield return null;
        }

        // 100% 꽉 찬 모습 0.2초 정도 시원하게 보여주기 (타격감!)
        if (loadingBar != null) loadingBar.value = 1f;
        if (loadingText != null) loadingText.text = "100%";
        yield return new WaitForSeconds(0.2f);

        // 이제 붙잡았던 멱살을 놓고 진짜로 씬 이동!
        op.allowSceneActivation = true;
    }
}