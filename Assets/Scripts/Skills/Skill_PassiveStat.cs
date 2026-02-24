using UnityEngine;

[CreateAssetMenu(menuName = "Trickcal/Skills/Passive Stat Boost")]
public class Skill_PassiveStat : SkillBase
{
    [Header("패시브 스탯 증가량")]
    public int bonusDamage = 0;
    public float bonusAttackSpeed = 0f;

    // 주의: 스크립터블 오브젝트(SO)의 원본 데이터를 직접 건드리면 안 됩니다!

    public override void Execute(UnitStat user, Vector3 targetPos = default)
    {       
        ApplyPassive(user);
    }

    public override void OnEquip(UnitStat user)
    {
        // 만약 나중에 RPG처럼 스킬을 꼈다 뺐다 하는 시스템을 만드신다면,
        // 이 OnEquip과 OnUnequip(직접 만들어야 함)을 활용하면 됩니다.
        // 지금은 Execute에서 처리하므로 비워둬도 무방합니다.
    }

    private void ApplyPassive(UnitStat user)
    {
        if (user == null) return;

        //  원본(data)이 아니라 내 개인 스탯(current)에 더합니다.
        user.currentDamage += bonusDamage;
        user.currentAttackSpeed += bonusAttackSpeed;

        Debug.Log($"[패시브 발동] {skillName} 적용 완료! (공격력 +{bonusDamage}, 공속 +{bonusAttackSpeed})");

        // 만약 이펙트가 있다면 여기서 한 번 터뜨려줘도 좋습니다!
        if (skillSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(skillSound);
        }
    }
}