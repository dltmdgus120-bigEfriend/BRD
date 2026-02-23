using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Tip Data", menuName = "Trickcal/Tip Data")]
public class GameTipData : ScriptableObject
{
    [Header("게임 로딩 팁 목록")]
    [TextArea(2, 5)] 
    public List<string> tips;
}