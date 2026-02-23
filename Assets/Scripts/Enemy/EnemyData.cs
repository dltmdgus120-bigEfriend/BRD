using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Trickcal/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("적 기본 정보")]
    public string enemyName;
    public Sprite icon;            //  적 얼굴 아이콘
    public GameObject prefab;      //  적 프리팹 (스폰용)
    public bool isBoss = false;

    [Header("태그 정보 (아군과 동일한 SO 사용!)")]
    public UnitAttribute attribute; // (예: 순수, 광기, 활발...)
    public UnitRace race;           // (예: 요정, 수인, 마녀...)

    [Header("전투 스펙")]
    public int maxHP;
    public int armor;               // 물리 방어력
    public int magicResist;         // 마법 저항력
    public float moveSpeed = 3f;

    [Header("설명")]
    [TextArea]
    public string description;      //  적 설명 (보스몹 플레이버 텍스트용)
    [Header("대사")]
    [TextArea]
    public string spawnQuote;

    [Header("보상 설정")]
    public int dropGold = 10;
    public int dropElif = 0;
}