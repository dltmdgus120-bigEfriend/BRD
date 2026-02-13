using UnityEngine;

public class UnitSelection : MonoBehaviour
{
    [Header("연결")]
    public GameObject selectionCircle; 

    void Start()
    {
        // 시작할 땐 무조건 끄기
        if (selectionCircle != null)
            selectionCircle.SetActive(false);
    }

    // 외부(RTSController)에서 부를 함수
    public void SetSelected(bool isSelected)
    {
        if (selectionCircle != null)
        {
            selectionCircle.SetActive(isSelected);
        }
    }
}
