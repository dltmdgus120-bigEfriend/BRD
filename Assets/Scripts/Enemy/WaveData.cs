using UnityEngine;

[CreateAssetMenu(fileName = "New Wave", menuName = "Trickcal/Wave Data")]
public class WaveData : ScriptableObject
{
    [Header("웨이브 설정")]
   
    public EnemyData enemyToSpawn;

    public int count;              // 몇 마리 나올지
    public float spawnRate;        // 몇 초마다 나올지 (예: 0.5초에 1마리)

    
}