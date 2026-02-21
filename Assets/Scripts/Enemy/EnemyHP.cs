using UnityEngine;
using UnityEngine.UI; 

public class EnemyHP : MonoBehaviour
{
    [Header("적 데이터 (SO 연결)")]
    public EnemyData data; // ★ 이제 모든 스탯은 여기서 가져옵니다!
    public int currentHP { get; private set; } // 밖에서 읽을 수만 있게 설정

    [Header("UI 연결")]
    public Image hpFillImage; // 체력바(채워지는 부분) 이미지

    [Header("타겟팅 마크")]
    public GameObject focusIndicator;

    [Header("사망 효과")]
    public GameObject deathVFX;  // 파티클 프리팹
    public AudioClip deathSound; // 몬스터 사망 효과음

    void Start()
    {
        if (data != null)
        {
            currentHP = data.maxHP; // SO에서 최대 체력 가져오기
        }
        UpdateHPBar();
    }

    //공격 타입을 인자로 받아서 데미지 계산
    public void TakeDamage(int damage, AttackType type)
    {
        if (data == null) return; 

        int finalDamage = damage;

        switch (type)
        {
            case AttackType.Physical:
                finalDamage = Mathf.Max(1, damage - data.armor);
                break;
            case AttackType.Magic:
                finalDamage = Mathf.Max(1, damage - data.magicResist);
                break;
            case AttackType.Fixed:
                finalDamage = damage;
                break;
        }

        currentHP -= finalDamage;
        UpdateHPBar();

        // 실시간 UI 갱신 핵심 로직
        // 만약 지금 정보창이 열려있고, 그 창에 떠있는 게 '나(this)'라면? 패널 새로고침!
        if (EnemyInfoPanel.Instance != null && EnemyInfoPanel.Instance.currentSelectedEnemy == this)
        {
            EnemyInfoPanel.Instance.RefreshPanel();
        }

        if (currentHP <= 0) Die();
    }

    // 체력바 길이를 조절하는 함수
    void UpdateHPBar()
    {
        if (hpFillImage != null && data != null)
        {
            hpFillImage.fillAmount = (float)currentHP / data.maxHP;
        }
    }

    void Die()
    {
        if (DefenseManager.Instance != null && data != null)
        {
            DefenseManager.Instance.AddCurrency(data.dropGold, data.dropElif); // SO에서 보상 가져오기
            DefenseManager.Instance.UnregisterEnemy();
        }

        if (deathSound != null && SoundManager.Instance != null) SoundManager.Instance.PlaySFX(deathSound);

        if (deathVFX != null)
        {
            GameObject effect = Instantiate(deathVFX, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        Destroy(gameObject);
    }

    public void SetFocusMark(bool isOn)
    {
        if (focusIndicator != null)
        {
            focusIndicator.SetActive(isOn);
        }
    }
}
