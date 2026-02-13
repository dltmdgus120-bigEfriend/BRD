using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class UnitInfoPanel : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject contentRoot; // 정보창 전체를 끄고 켜기 위한 부모 객체
    public Image portraitImage;    // 초상화 이미지

    [Header("스탯 텍스트 연결")]
    public TMP_Text damageText; 
    public TMP_Text speedText;
    public TMP_Text rangeText;

    void Start()
    {
        // 시작할 땐 선택된 게 없으니 숨김
        ClosePanel();
    }

    // 외부(RTSController)에서 유닛 정보를 넘겨줄 함수
    public void UpdateInfo(UnitStat stat)
    {
        // 선택된 유닛이 없거나 데이터가 없으면 패널 닫기
        if (stat == null || stat.data == null)
        {
            ClosePanel();
            return;
        }

        // 정보가 있으면 패널 열기
        OpenPanel();

        // 1. 포트레이트 설정 (UnitData에 있는 icon 사용)
        if (portraitImage != null)
        {
            portraitImage.sprite = stat.data.icon;
        }

        // 2. 스탯 텍스트 설정
        // 공격력
        damageText.text = $"공격력: {stat.data.damage}";

        // 공격속도 (소수점 2자리까지 표시: F2)
        // *참고: 현재 데이터상 숫자가 낮을수록 빠른 것입니다. (공격 딜레이)
        speedText.text = $"공격속도: {stat.data.attackSpeed:F2}";

        // 사거리
        rangeText.text = $"사거리: {stat.data.attackRange}";
    }

    void OpenPanel()
    {
        if (contentRoot != null) contentRoot.SetActive(true);
    }

    public void ClosePanel()
    {
        if (contentRoot != null) contentRoot.SetActive(false);
    }
}