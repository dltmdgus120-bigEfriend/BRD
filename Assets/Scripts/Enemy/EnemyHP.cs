using UnityEngine;
using UnityEngine.UI; 

public class EnemyHP : MonoBehaviour
{
    public int maxHP = 100;
    private int currentHP;

    [Header("방어 스탯")]
    public int armor = 0;       // 물리 방어력
    public int magicResist = 0; // 마법 저항력

    [Header("보상 설정 (잡으면 주는 돈)")]
    public int dropGold = 10;   // 일반 몹은 10원
    public int dropElif = 0;    // 보스나 히든 몹만 값을 넣으세요

    [Header("UI 연결")]
    public Image hpFillImage; // 체력바(채워지는 부분) 이미지

    [Header("사망 효과")]
    public GameObject deathVFX;  // 파티클 프리팹
    public AudioClip deathSound; // 몬스터 사망 효과음

    void Start()
    {
        currentHP = maxHP;
        UpdateHPBar(); // 태어날 때 체력바 꽉 채우기
    }

    // ★ 공격 타입을 인자로 받아서 데미지 계산
    public void TakeDamage(int damage, AttackType type)
    {
        int finalDamage = damage;

        // 공격 타입에 따른 방어력 적용 공식
        switch (type)
        {
            case AttackType.Physical:
                // 물리: 데미지 - 방어력 (최소 1 데미지는 들어감)
                finalDamage = Mathf.Max(1, damage - armor);
                break;

            case AttackType.Magic:
                // 마법: 데미지 - 마법저항력 (최소 1)
                // (나중에 % 감소 공식으로 바꿔도 됨)
                finalDamage = Mathf.Max(1, damage - magicResist);
                break;

            case AttackType.Fixed:
                // 고정: 방어력 무시 (그대로 들어감)
                finalDamage = damage;
                break;
        }

        currentHP -= finalDamage;
        UpdateHPBar();

        // (선택) 데미지 텍스트 띄우기 (타입별 색상 적용 가능)
        Debug.Log($"받은 피해: {finalDamage} ({type})");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    // 체력바 길이를 조절하는 함수
    void UpdateHPBar()
    {
        if (hpFillImage != null)
        {
            // fillAmount는 0.0(빈칸) ~ 1.0(꽉참) 사이의 값입니다.
            // 현재 체력을 최대 체력으로 나누면 비율이 나옵니다. (소수점 계산을 위해 float 형변환)
            hpFillImage.fillAmount = (float)currentHP / maxHP;
        }
    }

    void Die()
    {
        //  죽으면서 매니저에게 돈 입금
        if (DefenseManager.Instance != null)
        {
            DefenseManager.Instance.AddCurrency(dropGold, dropElif);
            // ★ 적 숫자 카운트 감소
            DefenseManager.Instance.UnregisterEnemy();
        }

        if (deathSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(deathSound);
        }

        if (deathVFX != null)
        {
            // 이펙트를 몬스터가 죽은 그 위치(transform.position)에 생성합니다.
            GameObject effect = Instantiate(deathVFX, transform.position, Quaternion.identity);

            // ★ 파티클이 무한히 쌓이지 않도록 2초 뒤에 파괴합니다.
            // (만약 파티클 재생 시간이 더 길다면 2f 숫자를 늘려주세요!)
            Destroy(effect, 2f);
        }

        // (추후 사망 이펙트 추가 가능)
        Destroy(gameObject);
    }
}