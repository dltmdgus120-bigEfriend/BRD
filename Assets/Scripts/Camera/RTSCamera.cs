using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("이동 설정")]
    public float panSpeed = 20f;        // 이동 속도
    public float panBorderThickness = 20f; // 엣지 스크롤 감지 범위 (픽셀)
    public Vector2 mapLimit = new Vector2(50, 50); // 맵 이동 제한 범위

    [Header("줌 설정")]
    public float zoomSpeed = 20f;
    public float minSize = 3f;   // 줌인 최대치 (작을수록 가까움)
    public float maxSize = 15f;  // 줌아웃 최대치
    public float smoothTime = 10f; // 부드러운 줌

    private Camera cam;
    private float targetSize;

    void Start()
    {
        cam = GetComponent<Camera>();

        // 카메라가 Orthographic(직교)인지 Perspective(원근)인지에 따라 초기값 설정
        if (cam.orthographic)
        {
            targetSize = cam.orthographicSize;
        }
        else
        {
            // 만약 Perspective 카메라를 쓰고 계신다면 FOV를 조절하거나, 
            // Orthographic으로 설정을 바꾸시는 것을 추천합니다. (보여주신 코드는 Ortho 기준)
            targetSize = cam.fieldOfView;
        }
    }

    void Update()
    {
        Move();
        Zoom();
    }

    void Move()
    {
        // 1. 마우스 입력값 받기 (WASD 제거됨)
        float xInput = 0f;
        float zInput = 0f;

        // 마우스가 화면 위쪽 끝
        if (Input.mousePosition.y >= Screen.height - panBorderThickness)
        {
            zInput += 1;
        }
        // 마우스가 화면 아래쪽 끝 (UI가 있어도 작동함)
        if (Input.mousePosition.y <= panBorderThickness)
        {
            zInput -= 1;
        }
        // 마우스가 화면 오른쪽 끝
        if (Input.mousePosition.x >= Screen.width - panBorderThickness)
        {
            xInput += 1;
        }
        // 마우스가 화면 왼쪽 끝
        if (Input.mousePosition.x <= panBorderThickness)
        {
            xInput -= 1;
        }

        // 2. 카메라가 보고 있는 방향 기준으로 이동 방향 계산 (핵심!)
        // y값을 0으로 만들어서 땅바닥과 수평하게 만듦
        Vector3 forward = transform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = transform.right;
        right.y = 0;
        right.Normalize();

        // 3. 최종 이동 방향 결정
        // (카메라 정면 * 위아래) + (카메라 우측 * 좌우)
        Vector3 moveDir = (forward * zInput) + (right * xInput);

        // 4. 대각선 이동 속도 보정 (루트2 만큼 빨라지는 것 방지)
        if (moveDir.sqrMagnitude > 0)
        {
            moveDir.Normalize();
        }

        // 5. 이동 적용
        Vector3 targetPos = transform.position + moveDir * panSpeed * Time.deltaTime;

        // 6. 맵 밖으로 못 나가게 가두기
        targetPos.x = Mathf.Clamp(targetPos.x, -mapLimit.x, mapLimit.x);

        // z축 제한 (쿼터뷰라 z축 이동이 맵의 위아래 이동)
        targetPos.z = Mathf.Clamp(targetPos.z, -mapLimit.y, mapLimit.y);

        transform.position = targetPos;
    }

    void Zoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            targetSize -= scroll * zoomSpeed;
            targetSize = Mathf.Clamp(targetSize, minSize, maxSize);
        }

        // 부드러운 줌 적용
        if (cam.orthographic)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * smoothTime);
        }
        else
        {
            // 혹시 Perspective 카메라일 경우 대비
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetSize, Time.deltaTime * smoothTime);
        }
    }
}