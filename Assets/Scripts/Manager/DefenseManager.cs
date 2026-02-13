using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DefenseManager : MonoBehaviour
{
    public static DefenseManager Instance;

    [Header("--- 설정 ---")]
    public Transform waypointsParent;
    public List<WaveData> waves;

    [Header("--- 오디오 ---")]
    public AudioClip gameBGM;

    [Header("--- 게임 상태 ---")]
    public int playerHP = 40;
    public int currentRound = 0;
    public float roundTime = 40f;    
    public float prepTime = 30f;
    public bool isGameOver = false;

    private float timer = 0f;
    private Transform[] pathPoints;

    [Header("--- UI 연결 (꼭 확인하세요!) ---")]
    public Text timerText; 
    public Text hpText;
    public Text roundText;
    public GameObject gameOverPanel;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {

        if (SoundManager.Instance != null && gameBGM != null)
        {
            SoundManager.Instance.PlayBGM(gameBGM);
        }
        // 웨이포인트 세팅
        if (waypointsParent != null)
        {
            pathPoints = new Transform[waypointsParent.childCount];
            for (int i = 0; i < pathPoints.Length; i++)
            {
                pathPoints[i] = waypointsParent.GetChild(i);
            }
        }

        UpdateUI();
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        // 1. 게임 시작 전 준비 시간 (30초 카운트다운)
        timer = prepTime;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            UpdateTimerUI("준비 시간", timer); // ★ 매 프레임 갱신!
            yield return null;
        }

        // 2. 라운드 무한 반복
        while (playerHP > 0)
        {
            currentRound++;
            UpdateUI(); // 라운드 번호 갱신

            // 적 소환 시작
            if (currentRound <= waves.Count)
            {
                StartCoroutine(SpawnWave(waves[currentRound - 1]));
            }

            // 라운드 진행 시간 (40초 카운트다운)
            timer = roundTime;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                UpdateTimerUI($"{currentRound} 라운드", timer); // ★ 매 프레임 갱신!

                if (playerHP <= 0) break; // 죽으면 즉시 중단
                yield return null;
            }
        }
    }

    // ★ 숫자를 예쁘게 보여주는 함수
    void UpdateTimerUI(string label, float timeRemaining)
    {
        if (timerText != null)
        {
            // 1. 소수점 올림 처리 (29.9초 -> 30초)
            int intTime = Mathf.CeilToInt(timeRemaining);

            // 2. 음수 방지 (0초 밑으로 안 내려가게)
            intTime = Mathf.Max(0, intTime);

            // 3. 텍스트 표시 (Rich Text로 숫자만 크게 강조)
            timerText.text = $"{label}\n<size=30>{intTime}</size>";
        }
    }

    

    
    IEnumerator SpawnWave(WaveData data)
    {
        for (int i = 0; i < data.count; i++)
        {
            if (playerHP <= 0) yield break;
            SpawnEnemy(data.enemyPrefab, data.moveSpeed);
            yield return new WaitForSeconds(data.spawnRate);
        }
    }

    void SpawnEnemy(GameObject prefab, float speed)
    {
        if (pathPoints == null || pathPoints.Length == 0) return;

        GameObject enemy = Instantiate(prefab, pathPoints[0].position, Quaternion.identity);
        EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
        if (movement != null)
        {
            movement.speed = speed;
            movement.Setup(pathPoints);
        }
    }

    public void TakeDamage(int dmg)
    {
        playerHP -= dmg;
        UpdateUI();
        if (playerHP <= 0) GameOver();
    }

    void GameOver()
    {
        if (isGameOver) return; // 이미 끝났으면 실행 안 함

        isGameOver = true; // 상태 변경
        Debug.Log("게임 오버!");

        Time.timeScale = 0; // 시간 정지

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // 패널 켜기
        }
    }

    void UpdateUI()
    {
        if (hpText != null) hpText.text = $"HP: {playerHP}";
        if (roundText != null) roundText.text = $"Round: {currentRound}";
    }
}