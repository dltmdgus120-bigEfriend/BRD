using UnityEngine;

// 속성과 종족이 공통적으로 가질 데이터
public abstract class UnitTrait : ScriptableObject
{
    public string traitName; // 이름 (예: 광기, 엘프)
    public Sprite icon;      // 아이콘
    public Color color = Color.white; // 텍스트 색상 (예: 불은 빨강, 물은 파랑)
    [TextArea] public string description; // 설명
}