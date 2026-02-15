using UnityEngine;

// 스킬의 기본 뼈대 (이걸로 직접 만들진 않고 상속해서 씁니다)
public abstract class SkillBase : ScriptableObject
{
    [Header("기본 정보")]
    public string skillName;
    public Sprite icon;
    [TextArea] public string description;

    [Header("설정")]
    public bool isPassive; // 패시브 여부
    public float cooldown; // 쿨타임 (패시브면 0)

    // 스킬을 사용했을 때 일어날 일 (자식들이 내용을 채워야 함)
    // user: 스킬을 쓴 유닛
    public abstract void Execute(UnitStat user);

    // (옵션) 패시브라면 장착하자마자 발동할 효과
    public virtual void OnEquip(UnitStat user) { }
}