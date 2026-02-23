using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyInfoPanel : MonoBehaviour
{
    public static EnemyInfoPanel Instance;

    [Header("UI 연결")]
    public GameObject panelObject;
    public Image portraitImage;    
    public TMP_Text nameText;
    public TMP_Text descriptionText;

    [Header("속성 & 종족 표시")]
    public Image attributeIcon;    
    public TMP_Text attributeText;
    public Image raceIcon;         
    public TMP_Text raceText;

    [Header("스탯 아이콘 (이미지)")]
    public Image hpIcon;
    public Image armorIcon;
    public Image magicResistIcon;

    [Header("스탯 텍스트")]
    public TMP_Text hpText;
    public TMP_Text defenseText;

    public EnemyHP currentSelectedEnemy { get; private set; }

    void Awake()
    {
        Instance = this;
        HidePanel(); // 시작할 땐 꺼두기
    }

    public void ShowEnemyInfo(EnemyHP enemy)
    {
        currentSelectedEnemy = enemy; // 누구를 눌렀는지 기억
        panelObject.SetActive(true);
  
        if (UnitInfoPanel.Instance != null)
        {
            UnitInfoPanel.Instance.CloseAllPanels();
        }

        RefreshPanel(); // UI 그리기
    }

    public void RefreshPanel()
    {
        if (currentSelectedEnemy == null || currentSelectedEnemy.data == null) return;

        EnemyData data = currentSelectedEnemy.data;

        // 1. 기본 정보 갱신
        if (portraitImage != null) portraitImage.sprite = data.icon;
        if (nameText != null) nameText.text = data.enemyName;
        if (descriptionText != null) descriptionText.text = data.description;

        // 2. 속성 정보 갱신 (UnitInfoPanel과 동일한 방식!)
        if (data.attribute != null)
        {
            if (attributeIcon != null)
            {
                attributeIcon.gameObject.SetActive(true);
                attributeIcon.sprite = data.attribute.icon;
            }
            if (attributeText != null)
            {
                attributeText.text = data.attribute.traitName;
                attributeText.color = data.attribute.color;
            }
        }
        else
        {
            if (attributeIcon != null) attributeIcon.gameObject.SetActive(false);
            if (attributeText != null) attributeText.text = "-";
        }

        // 3. 종족 정보 갱신
        if (data.race != null)
        {
            if (raceIcon != null)
            {
                raceIcon.gameObject.SetActive(true);
                raceIcon.sprite = data.race.icon;
            }
            if (raceText != null)
            {
                raceText.text = data.race.traitName;
                raceText.color = data.race.color;
            }
        }
        else
        {
            if (raceIcon != null) raceIcon.gameObject.SetActive(false);
            if (raceText != null) raceText.text = "-";
        }

        // 4. 전투 스탯 갱신 (순수 숫자만!)
        if (hpText != null)
        {
            // 예: "1500 / 1500"
            hpText.text = $"{currentSelectedEnemy.currentHP} / {data.maxHP}";
        }

        if (defenseText != null)
        {
            // 방어력과 마저를 한 줄로 예쁘게 표시 (예: "50 / 30")
            
            defenseText.text = $"{data.armor}                {data.magicResist}";
        }
    }

    public void HidePanel()
    {
        currentSelectedEnemy = null;
        panelObject.SetActive(false);
    }
}