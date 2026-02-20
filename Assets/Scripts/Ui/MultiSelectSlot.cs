using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MultiSelectSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI 연결")]
    public Image iconImage;   

    private UnityEngine.AI.NavMeshAgent myAgent;
    private RTSController rtsController;

    // 슬롯 초기화
    public void Setup(UnityEngine.AI.NavMeshAgent agent, RTSController controller)
    {
        myAgent = agent;
        rtsController = controller;

        UnitStat stat = agent.GetComponent<UnitStat>();
        if (stat != null && stat.data != null)
        {
            iconImage.sprite = stat.data.icon; // 아이콘 설정
        }

        
        
    }

    void Update()
    {
        
        
    }

   

    // 클릭하면 나만 선택하기!
    public void OnPointerClick(PointerEventData eventData)
    {
        if (rtsController != null && myAgent != null)
        {
            // 컨트롤러에게 "나만 선택해!" 명령 (아까 만든 함수 사용)
            rtsController.SelectUnit(myAgent);
        }
    }
}