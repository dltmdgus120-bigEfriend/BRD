using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DefenseManager : MonoBehaviour
{
    public static DefenseManager Instance;

    [Header("--- 설정 ---")]
    public Transform waypointsParent;
    private Transform[] pathPoints;

    [Header("--- 웨이브 데이터 (일반) ---")]
    public List<WaveData> waves;

    [Header("--- 보스 데이터 (10라운드마다 순서대로) ---")]
    public List<GameObject> bossPrefabs; // 인스펙터에서 보스 프리팹들을 순서대로 넣으세요.
    public float bossTimeLimit = 120f;     //  보스 제한시간   

    [Header("--- 게임 상태 ---")]
    public int playerHP = 40;
    public int currentRound = 0;
    public float roundTime = 40f;    
    public float prepTime = 30f;
    public bool isGameOver = false;

    private GameObject currentBossInstance;
   
   
    [Header("--- UI 연결 (꼭 확인하세요!) ---")]
    public Text timerText; 
    public Text hpText;
    public Text roundText;
    public GameObject gameOverPanel;

    [Header("--- 경제 시스템 ---")]
    public int gold = 0;    // 기본 골드
    public int elif = 0;    // 특수 재화 (엘리프)

    [Header("--- 경제 UI ---")]
    public Text goldText;   // 골드 표시 텍스트
    public Text elifText;   // 엘리프 표시 텍스트

    [Header("--- 오디오 ---")]
    public AudioClip gameBGM;

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
        // 1. 초기 준비 시간
        yield return StartCoroutine(RunTimer("준비 시간", prepTime));

        // 2. 라운드 무한 반복
        while (playerHP > 0 && !isGameOver)
        {
            currentRound++;
            UpdateUI();

            // ★ 10라운드 단위 체크 (10, 20, 30...)
            if (currentRound % 10 == 0)
            {
                yield return StartCoroutine(RunBossRound());
            }
            else
            {
                yield return StartCoroutine(RunNormalRound());
            }

            // 라운드 사이 짧은 대기 (선택사항, 필요 없으면 삭제)
            yield return new WaitForSeconds(1f);
        }
    }

    // --- 일반 라운드 로직 ---
    IEnumerator RunNormalRound()
    {
        // 웨이브 데이터 가져오기 (데이터가 부족하면 마지막 데이터 반복)
        int waveIndex = Mathf.Clamp(currentRound - 1, 0, waves.Count - 1);
        if (waves.Count > 0)
        {
            StartCoroutine(SpawnWave(waves[waveIndex]));
        }

        // 40초 버티기 타이머
        yield return StartCoroutine(RunTimer($"{currentRound} 라운드", roundTime));
    }

    // --- 보스 라운드 로직 ---
    IEnumerator RunBossRound()
    {
        // 보스 인덱스 계산 (10라운드->0번 보스, 20라운드->1번 보스...)
        int bossIndex = (currentRound / 10) - 1;

        // 보스 리스트 범위를 넘어가면 마지막 보스 반복
        if (bossPrefabs.Count > 0)
        {
            bossIndex = Mathf.Clamp(bossIndex, 0, bossPrefabs.Count - 1);
            SpawnBoss(bossPrefabs[bossIndex]);
        }
        else
        {
            Debug.LogError("보스 프리팹이 설정되지 않았습니다!");
        }

        // ★ 보스 타이머 로직 (2분)
        float timer = bossTimeLimit;
        while (timer > 0)
        {
            // 보스가 죽었는지 확인 (플레이어가 보스를 잡음 -> 라운드 클리어)
            if (currentBossInstance == null)
            {
                UpdateTimerUI("보스 처치!", 0);
                yield return new WaitForSeconds(2f); // 승리 메시지 잠깐 보여줌
                break; // 루프 탈출 (다음 라운드로)
            }

            timer -= Time.deltaTime;
            UpdateTimerUI($"<color=red>BOSS!!</color>", timer);

            if (playerHP <= 0) break;
            yield return null;
        }

        // 시간이 다 됐는데 아직 보스가 살아있다면? -> 게임 오버
        if (currentBossInstance != null && timer <= 0)
        {
            Debug.Log("보스 타임오버!");
            GameOver();
        }
    }

    // --- 유틸리티 함수들 ---

    IEnumerator RunTimer(string label, float time)
    {
        while (time > 0 && !isGameOver && playerHP > 0)
        {
            time -= Time.deltaTime;
            UpdateTimerUI(label, time);
            yield return null;
        }
    }


  
    void UpdateTimerUI(string label, float timeRemaining)
    {
        if (timerText != null)
        {
            int intTime = Mathf.Max(0, Mathf.CeilToInt(timeRemaining));
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

    //  보스 소환 함수
    void SpawnBoss(GameObject prefab)
    {
        if (pathPoints == null || pathPoints.Length == 0) return;

        // 보스 생성 및 변수에 저장 (추적용)
        currentBossInstance = Instantiate(prefab, pathPoints[0].position, Quaternion.identity);

        // 보스 이동 설정
        EnemyMovement movement = currentBossInstance.GetComponent<EnemyMovement>();
        if (movement != null)
        {
            // 보스 속도는 프리팹에 설정된 값을 따르거나, 필요시 여기서 수정
            movement.isLooping = true; // ★ 보스는 순환하도록 강제 설정
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

    //돈을 버는 함수 (적이 죽을 때 호출)
    public void AddCurrency(int _gold, int _elif)
    {
        gold += _gold;
        elif += _elif;
        UpdateCurrencyUI(); // UI 갱신
    }

    // 돈을 쓰는 함수 (업그레이드 등) - 리턴값: 성공 여부
    public bool SpendCurrency(int _goldCost, int _elifCost)
    {
        if (gold >= _goldCost && elif >= _elifCost)
        {
            gold -= _goldCost;
            elif -= _elifCost;
            UpdateCurrencyUI();
            return true; // 구매 성공
        }
        return false; // 구매 실패 (돈 부족)
    }

    void UpdateCurrencyUI()
    {
        // "N0"은 천 단위 쉼표 포맷 (1,000)
        if (goldText != null) goldText.text = $"{gold:N0}";
        if (elifText != null) elifText.text = $"{elif:N0}";
    }

}