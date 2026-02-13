using UnityEngine;

public class RTSCamera : MonoBehaviour
{
    [Header("이동 설정")]
    public float panSpeed = 20f;        // 이동 속도
    public float panBorderThickness = 10f; // 엣지 스크롤 범위
    public bool useEdgeScrolling = true;   // 마우스 엣지 스크롤 켜기/끄기
    public Vector2 mapLimit = new Vector2(50, 50); // 맵 제한

    [Header("줌 설정")]
    public float zoomSpeed = 20f;
    public float minSize = 3f;
    public float maxSize = 15f;
    public float smoothTime = 10f;

    private Camera cam;
    private float targetSize;

    void Start()
    {
        cam = GetComponent<Camera>();
        targetSize = cam.orthographicSize;
    }

    void Update()
    {
        Move();
        Zoom();
    }

    void Move()
    {
        // 1. 입력값 받기 (키보드 + 마우스 엣지)
        float xInput = 0f;
        float zInput = 0f;

        // 키보드 입력
        xInput += Input.GetAxis("Horizontal"); // A, D
        zInput += Input.GetAxis("Vertical");   // W, S

        // 마우스 엣지 스크롤 (원하면 켜세요)
        if (useEdgeScrolling)
        {
            if (Input.mousePosition.y >= Screen.height - panBorderThickness) zInput += 1;
            if (Input.mousePosition.y <= panBorderThickness) zInput -= 1;
            if (Input.mousePosition.x >= Screen.width - panBorderThickness) xInput += 1;
            if (Input.mousePosition.x <= panBorderThickness) xInput -= 1;
        }

        // 2. 카메라가 보고 있는 방향 알아내기
        // (그냥 forward를 쓰면 땅으로 파고드니까, y값을 0으로 없애서 수평으로 만듦)
        Vector3 forward = transform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = transform.right;
        right.y = 0;
        right.Normalize();

        // 3. 방향과 입력값을 섞어서 최종 이동 방향 결정
        // (카메라 정면 * 위아래 입력) + (카메라 우측 * 좌우 입력)
        Vector3 moveDir = (forward * zInput) + (right * xInput);

        // 4. 이동 적용 (정규화해서 대각선 이동 시 빨라지는 것 방지)
        if (moveDir.magnitude > 0) moveDir.Normalize();

        Vector3 targetPos = transform.position + moveDir * panSpeed * Time.deltaTime;

        // 5. 맵 밖으로 못 나가게 가두기
        targetPos.x = Mathf.Clamp(targetPos.x, -mapLimit.x, mapLimit.x);
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

        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * smoothTime);
    }
}