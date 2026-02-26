using UnityEngine;

// 스킬의 기본 뼈대 (이걸로 직접 만들진 않고 상속해서 씁니다)
public abstract class SkillBase : ScriptableObject
{
    [Header("기본 정보")]
    public string skillName;
    public Sprite icon;
    [TextArea] public string description;

    [Header("사운드 & 애니메이션")]
    public AudioClip castVoice;
    public AudioClip skillSound;
    public string animTriggerName = "Skill"; // 유니티 애니메이터에서 쓸 트리거 이름
    public float castTime = 0.5f;     // 1. 유닛이 멈춰서 주문을 외우는 시간 (애니메이션 길이)
    public float actionDelay = 0f;  // 2. 주문이 끝난 후, 스킬이 하늘에서 떨어지는 데 걸리는 시간

    [Header("설정")]
    public AttackType attackType;
    public bool isPassive; // 패시브 여부
    public float cooldown; // 쿨타임 (패시브면 0)


    [Header("타겟팅 설정")]
    public bool needTarget; // 체크하면 조준 모드로 진입 (메테오 등)
    public float targetRange;     // 사거리 (커서 범위 제한용, 0이면 제한 없음)   
    public float effectRadius = 0f;  // 스킬 범위 (폭발 반경) - 이걸로 원 크기를 조절함

    [Header("돌진 설정")]
    public bool isDashSkill; // 체크하면 제자리가 아니라 마우스 위치로 몸을 날립니다!

    [Header("일반공격 발동 설정")]
    public bool isAttackProc;   // 체크하면 "공격 시 확률 발동" 스킬이 됨
    [Range(0, 100)]
    public float procChance;    // 발동 확률 (0 ~ 100%)


    // 시전 시작 직후에 실행될 함수 (가상 함수라 안 써도 그만)
    // 예: 메테오가 하늘에 생성됨, 투사체 발사 등
    public virtual void OnCastStart(UnitStat user, Vector3 targetPos = default) { }

    // 시간(castTime)이 다 지나고 최종적으로 실행될 함수
    // 예: 메테오 폭발 및 데미지 적용
    public abstract void Execute(UnitStat user, Vector3 targetPos = default);

    // 패시브라면 장착하자마자 발동할 효과
    public virtual void OnEquip(UnitStat user) { }
}