using UnityEngine;
using UnityEngine.UI; // ★ UI를 다루기 위해 꼭 추가해야 합니다!

public class EnemyHP : MonoBehaviour
{
    public int maxHP = 100;
    private int currentHP;

    [Header("보상 설정 (잡으면 주는 돈)")]
    public int dropGold = 10;   // 일반 몹은 10원
    public int dropElif = 0;    // 보스나 히든 몹만 값을 넣으세요

    [Header("UI 연결")]
    public Image hpFillImage; // 체력바(채워지는 부분) 이미지

    void Start()
    {
        currentHP = maxHP;
        UpdateHPBar(); // 태어날 때 체력바 꽉 채우기
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        UpdateHPBar(); // 맞을 때마다 체력바 갱신

        if (currentHP <= 0)
        {
            Die();
        }
    }

    // ★ 체력바 길이를 조절하는 함수
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
            // ★ [핵심] 적 숫자 카운트 감소
            DefenseManager.Instance.UnregisterEnemy();
        }

        // (추후 사망 이펙트 추가 가능)
        Destroy(gameObject);
    }
}