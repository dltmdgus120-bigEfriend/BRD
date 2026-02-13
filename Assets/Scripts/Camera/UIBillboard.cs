using UnityEngine;

public class UIBillboard : MonoBehaviour
{
    private Camera mainCam;

    void Start()
    {
        // 메인 카메라 찾기
        mainCam = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCam == null) return;

        // 캔버스가 항상 카메라를 정면으로 쳐다보게 회전시킴
        transform.LookAt(transform.position + mainCam.transform.rotation * Vector3.forward,
                         mainCam.transform.rotation * Vector3.up);
    }
}