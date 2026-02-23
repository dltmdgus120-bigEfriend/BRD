using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Unit", menuName = "Trickcal/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("사도 정보")]
    public string unitName;
    public string level;
    public int rank;
    public GameObject prefab;
    public Sprite icon;
    public AudioClip summonVoice;
    public AudioClip attackSound;


    public int sellPrice = 10;
    public int sellElif = 1;

    [Header("태그 정보")]
    public UnitAttribute attribute; // (예: 순수, 광기, 냉정...)
    public UnitRace race;           // (예: 요정, 정령, 유령...)

    [Header("스킬 (Q, W, E, R 순서)")]
    // 건물의 기능(생산, 업그레이드 등)은 여기에 스킬로 등록하면 됩니다.
    public List<SkillBase> skills;

    [Header("전투 스펙")]
    public AttackType attackType; // 공격 타입 
    public float attackRange = 0f;  // 건물은 공격 안 하니까 0
    public float attackSpeed = 1f;
    public int damage = 0;
    public GameObject projectilePrefab;

    [Header("설명 및 대사")]
    [TextArea]
    public string description; 
    [TextArea]
    public string summonQuote;

    [Header("조합 가능 목록")]  // 조합 메인에만
    public List<CombinationRecipe> availableRecipes;

    //  건물 설정
    [Header("건물 설정")]
    public bool isBuilding = false; // 체크하면 이동/공격/판매 불가
}