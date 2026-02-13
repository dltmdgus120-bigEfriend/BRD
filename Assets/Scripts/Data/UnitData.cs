using UnityEngine;


[CreateAssetMenu(fileName = "New Unit", menuName = "Trickcal/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("사도 정보")]
    public string unitName;       // 이름 (예: 에르핀)
    public int rank;              // 등급 (1, 2, 3...)
    public GameObject prefab;     // 소환될 프리팹 (모델링)
    public Sprite icon;
    public AudioClip summonVoice;

    [Header("전투 스펙")] // 
    public float attackRange = 5f;  // 사거리
    public float attackSpeed = 1f;  // 공격 속도 (초당 공격 횟수 or 쿨타임)
    public int damage = 10;         // 공격력
    public GameObject projectilePrefab;

    [Header("설명")]
    [TextArea]
    public string description;    // (선택) 유닛 설명
}