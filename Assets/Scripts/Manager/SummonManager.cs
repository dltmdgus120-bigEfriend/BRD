using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SummonManager : MonoBehaviour
{
    public static SummonManager Instance;

    [Header("--- 유닛 데이터 리스트 ---")]
    public List<UnitData> rank1Units;   // 1성 (일반)
    public List<UnitData> rank2Units;   // 2성 (희귀)
    public List<UnitData> rank3Units;   // 3성 (전설)
    public List<UnitData> hiddenUnits;  // 히든 

    [Header("--- 확률 설정 (단위: %, 합계 100 권장) ---")]
    // 소수점 확률을 위해 float로 변경했습니다. (예: 0.5%)
    [Range(0, 100)] public float probRank1 = 69f;  // 69%
    [Range(0, 100)] public float probRank2 = 25f;  // 25%
    [Range(0, 100)] public float probRank3 = 5f;   // 5%
    [Range(0, 100)] public float probHidden = 1f;  // 1% (히든!)

    [Header("--- 소환 설정 ---")]
    public float spawnRadius = 3f;
    public Transform spawnPoint; // 소환 기준점

    [Header("--- 자원 시스템 ---")]
    public int currentTickets = 10;
    public float ticketInterval = 5f;
    public int ticketAmount = 1;
    private float timer = 0f;

    [Header("--- UI 연결 ---")]
    public Text ticketText;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= ticketInterval)
        {
            currentTickets += ticketAmount;
            timer = 0f;
            UpdateUI();
        }
    }

    public void OnClickSummon()
    {
        if (currentTickets > 0)
        {
            currentTickets--;
            SpawnRandomUnit();
            UpdateUI();
        }
        else
        {           
            if (LogManager.Instance != null)
                LogManager.Instance.ShowLog("티켓이 부족합니다!", LogType.System);
            else
                Debug.Log("티켓이 부족합니다!");
        }
    }

    // 핵심: 히든 포함 4단계 확률 로직
    void SpawnRandomUnit()
    {
        // 1. 0.0 ~ 100.0 사이의 실수 랜덤 뽑기
        float randomValue = Random.Range(0f, 100f);

        List<UnitData> selectedPool = null;
        string rankLog = "";

        // 2. 확률 체크 (누적 방식)
        // 예: 1성(69), 2성(25), 3성(5), 히든(1) 일 때
        // 0 ~ 69     : 1성
        // 69 ~ 94    : 2성 (69+25)
        // 94 ~ 99    : 3성 (69+25+5)
        // 99 ~ 100   : 히든

        float cumulative1 = probRank1;
        float cumulative2 = probRank1 + probRank2;
        float cumulative3 = probRank1 + probRank2 + probRank3;

        if (randomValue < cumulative1)
        {
            selectedPool = rank1Units;
            rankLog = "1성";
        }
        else if (randomValue < cumulative2)
        {
            selectedPool = rank2Units;
            rankLog = "2성";
        }
        else if (randomValue < cumulative3)
        {
            selectedPool = rank3Units;
            rankLog = "3성";
        }
        else
        {
            // 나머지는 전부 히든!
            selectedPool = hiddenUnits;
            rankLog = "<color=red>★히든★</color>";
        }

        // 3. 결과 소환
        if (selectedPool != null && selectedPool.Count > 0)
        {
            int unitIndex = Random.Range(0, selectedPool.Count);
            UnitData finalUnitData = selectedPool[unitIndex];

            CreateUnitObject(finalUnitData);

            
            if (LogManager.Instance != null)
            {
                if (selectedPool == hiddenUnits)
                    LogManager.Instance.ShowLog($"대박! {finalUnitData.unitName} 소환 성공!", LogType.Mission); // 히든은 특별하게 하늘색(Mission)
               
            }
        }
        else
        {
            Debug.LogError($"{rankLog} 리스트가 비어있습니다! Inspector를 확인해주세요.");
        }
    }

    void CreateUnitObject(UnitData data)
    {      

        // 1. 기준 위치 및 랜덤 위치 계산
        Vector3 center = (spawnPoint != null) ? spawnPoint.position : Vector3.zero;
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 randomPos = center + new Vector3(randomCircle.x, 0f, randomCircle.y);

        // 2. 바닥 찾기 (SamplePosition)
        UnityEngine.AI.NavMeshHit hit;

        // 반경 10.0f 내에서 유효한 바닥을 찾음
        if (UnityEngine.AI.NavMesh.SamplePosition(randomPos, out hit, 10.0f, UnityEngine.AI.NavMesh.AllAreas))
        {
            // 3. 유닛 생성 (프리팹에서 Agent를 꺼놨으므로 에러 안 남!)
            GameObject newUnit = PoolManager.Instance.GetAlly(hit.position);

            // 4. NavMesh Agent 활성화 및 위치 고정 (Warp 사용)
            var agent = newUnit.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                // 위치를 잡고
                newUnit.transform.position = hit.position;

                // 켜면서 동시에 위치를 '순간이동' 시켜서 네비메쉬에 안착시킴
                agent.enabled = true;
                bool warped = agent.Warp(hit.position);

                if (!warped)
                {
                    Debug.LogWarning("Warp 실패: 유닛이 바닥에 제대로 안착하지 못했습니다.");
                }
            }
            // 5, 데이터 주입
            UnitStat stat = newUnit.GetComponent<UnitStat>();
            if (stat != null)
            {
                stat.InitAlly(data); // <- 여기서 애니메이션, 스탯, 크기까지 싹 다 덮어씌워짐!

                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlayVoice(data.summonVoice);

                if (LogManager.Instance != null && !string.IsNullOrEmpty(data.summonQuote))
                {
                    string dialogueText = $"<color=orange>[{data.unitName}]</color> {data.summonQuote}";
                    LogManager.Instance.ShowLog(dialogueText, LogType.Dialogue);
                }
            }
        }
        else
        {
            Debug.LogWarning("소환 실패: 근처에 네비메쉬가 없습니다.");
        }
    }

    void UpdateUI()
    {
        if (ticketText != null)
        {
            ticketText.text = $"티켓: {currentTickets}";
        }
    }

    // 3성 확정 뽑기 ( 억까 방지용)
    public void SpawnGuaranteed3Star()
    {
        if (rank3Units == null || rank3Units.Count == 0)
        {
            Debug.LogError("3성 유닛 리스트가 비어있습니다!");
            return;
        }

        // 3성 리스트에서 무작위로 하나 뽑기
        int index = Random.Range(0, rank3Units.Count);
        UnitData guaranteedUnit = rank3Units[index];

        // 생성 (기존에 만들어둔 함수 재활용!)
        CreateUnitObject(guaranteedUnit);
    }
}