using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Trickcal/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("적 기본 정보")]
    public string enemyName;
    public Sprite icon;            //  적 얼굴 아이콘
    public Sprite inGameSprite;     // 인게임 맵에서 껍데기에 덮어씌울 실제 전신 이미지
    public float unitSize = 1f; // (예: 고블린 1f, 오크 1.5f, 쥐 0.5f)

    public GameObject prefab;      //  보스전용 프리팹 
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
    public string description;      //  적 설명 ( 플레이버 텍스트용)
    [Header("대사")]
    [TextArea]
    public string spawnQuote;

    [Header("보상 설정")]
    public int dropGold = 10;
    public int dropElif = 0;
}