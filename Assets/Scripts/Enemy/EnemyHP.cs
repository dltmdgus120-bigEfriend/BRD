using UnityEngine;


public class EnemyHP : MonoBehaviour
{
    [Header("적 데이터 (SO 연결)")]
    public EnemyData data;
    public int currentHP { get; private set; }

    [Header("시각 요소 연결")]
    public SpriteRenderer mainSpriteRenderer; // 껍데기 프리팹의 SpriteRenderer를 연결

    [Header("UI (Sprite) 연결")]
    public Transform hpFillTransform;   // Image hpFillImage 대신 Transform을 받아서 Scale X 값을 조절합니다.

    [Header("타겟팅 마크")]
    public GameObject focusIndicator;

    [Header("사망 효과")]
    public GameObject deathVFX;
    public AudioClip deathSound;

    // 풀링된 객체가 다시 켜질 때마다(라운드 등장 시) 스탯을 초기화합니다.
    void OnEnable()
    {
        if (data != null)
        {
            currentHP = data.maxHP;
            UpdateHPBar();
        }
    }

    // 외부(스포너)에서 몹의 종류(데이터)를 갈아끼워줄 때 호출할 함수
    public void InitEnemy(EnemyData newData)
    {
        data = newData;
        currentHP = data.maxHP;
        UpdateHPBar();

        // 풀에서 꺼내온 껍데기의 이미지를 이번 라운드 몹 이미지로 싹 갈아끼웁니다!
        if (mainSpriteRenderer != null && data.inGameSprite != null)
        {
            mainSpriteRenderer.sprite = data.inGameSprite;
        }

        transform.localScale = new Vector3(data.unitSize, data.unitSize, 1f);
    }

    public void TakeDamage(int damage, AttackType type)
    {
        if (data == null || currentHP <= 0) return; // 이미 죽은 애 때리기 방지

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

        if (EnemyInfoPanel.Instance != null && EnemyInfoPanel.Instance.currentSelectedEnemy == this)
        {
            EnemyInfoPanel.Instance.RefreshPanel();
        }

        if (currentHP <= 0) Die();
    }

    // 체력바 스프라이트의 가로 길이(Scale X)를 조절하는 방식
    void UpdateHPBar()
    {
        if (hpFillTransform != null && data != null)
        {
            float hpRatio = (float)currentHP / data.maxHP;
            // Z축은 건드리지 않고, X축 스케일만 비율에 맞춰 줄입니다.
            hpFillTransform.localScale = new Vector3(hpRatio, 1f, 1f);
        }
    }

    void Die()
    {
        if (DefenseManager.Instance != null && data != null)
        {
            DefenseManager.Instance.AddCurrency(data.dropGold, data.dropElif);
            DefenseManager.Instance.UnregisterEnemy();
        }

        if (deathSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(deathSound);
        }

        if (deathVFX != null)
        {          
            // 몬스터 크기에 따라 0.5f 값을 1.0f 등으로 조절해서 예쁜 위치를 찾아주세요.
            Vector3 vfxOffsetPosition = transform.position + new Vector3(0f, 1f, 0f);
            PoolManager.Instance.GetVFX(vfxOffsetPosition);
        }

        
        PoolManager.Instance.ReturnEnemy(gameObject);
    }

    public void SetFocusMark(bool isOn)
    {
        if (focusIndicator != null)
        {
            focusIndicator.SetActive(isOn);
        }
    }
}
