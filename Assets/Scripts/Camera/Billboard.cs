using UnityEngine;

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        // 내 회전값을 메인 카메라의 회전값과 똑같이 맞춘다!
        transform.rotation = Camera.main.transform.rotation;
    }
}