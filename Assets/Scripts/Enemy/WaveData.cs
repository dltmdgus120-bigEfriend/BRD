using UnityEngine;

[CreateAssetMenu(fileName = "New Wave", menuName = "Trickcal/Wave Data")]
public class WaveData : ScriptableObject
{
    public GameObject enemyPrefab; // 소환할 적 프리팹
    public int count;              // 몇 마리 나올지
    public float spawnRate;        // 몇 초마다 나올지 (예: 0.5초에 1마리)
    public float moveSpeed;        // 적 속도
}