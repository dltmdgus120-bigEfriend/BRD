using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // 리스트를 쓰기 위해 필수!

public class SummonManager : MonoBehaviour
{
    [Header("--- 설정 ---")]
    // 기존의 GameObject 하나짜리는 지우고, 데이터 리스트로 변경!
    public List<UnitData> rank1Units;

    public float spawnRadius = 3f;   // 맵 중앙에서 얼마나 퍼져서 나올지

    [Header("--- 자원 시스템 ---")]
    public int currentTickets = 0;   // 현재 보유 티켓
    public float ticketInterval = 5f; // 티켓 얻는 시간 (초)
    public int ticketAmount = 1;      // 한 번에 얻는 개수
    private float timer = 0f;

    [Header("--- UI 연결 ---")]
    public Text ticketText; // 티켓 개수 보여줄 텍스트 (Legacy Text)

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
            SpawnRandomApostle();   // 이름 변경: 랜덤 소환
            UpdateUI();
        }
        else
        {
            Debug.Log("티켓이 부족합니다!");
        }
    }

    void SpawnRandomApostle()
    {
        // 안전장치: 리스트가 비어있으면 에러 방지
        if (rank1Units == null || rank1Units.Count == 0)
        {
            Debug.LogError("소환할 1성 유닛 데이터가 리스트에 없습니다!");
            return;
        }

        // 1. 랜덤 뽑기 (0번부터 리스트 개수 사이의 숫자 하나 뽑기)
        int randomIndex = Random.Range(0, rank1Units.Count);
        UnitData randomUnitData = rank1Units[randomIndex];

        // 2. 위치 잡기
        Vector2 randomPoint = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = new Vector3(randomPoint.x, 3.0f, randomPoint.y); // 공중에서 소환

        // 3. 진짜 소환! (데이터에 연결된 프리팹을 사용)
        if (randomUnitData.prefab != null)
        {
            GameObject newUnit = Instantiate(randomUnitData.prefab, spawnPos, Quaternion.identity);

            // ★ 핵심: 소환된 유닛에게 "너는 이 데이터야"라고 신분증 쥐여주기
            // (이게 있어야 나중에 조합할 때 에러가 안 납니다)
            UnitStat stat = newUnit.GetComponent<UnitStat>();
            if (stat != null)
            {
                stat.data = randomUnitData;
                SoundManager.Instance.PlayVoice(randomUnitData.summonVoice);
            }
        }
    }

    void UpdateUI()
    {
        if (ticketText != null)
        {
            ticketText.text = "티켓: " + currentTickets;
        }
    }


}