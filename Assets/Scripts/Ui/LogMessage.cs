using UnityEngine;
using TMPro; // TextMeshPro 사용 권장
using System.Collections;


public enum LogType
{
    Dialogue,  // 캐릭터 대사 (예: 노란색)
    System,    // 일반 시스템 (예: 흰색)
    Resource,  // 자원 획득 (예: 초록색)
    Mission    // 미션 성공 (예: 하늘색)
}

public class LogMessage : MonoBehaviour
{
    [Header("UI 연결")]
    public TMP_Text messageText;

    [Header("설정")]
    public float displayTime = 3f; // 화면에 머무는 시간
    public float fadeTime = 1f;    // 서서히 사라지는 시간

    public void Setup(string message, LogType type)
    {
        messageText.text = message;

        // 타입에 따라 색상 다르게 지정
        switch (type)
        {
            case LogType.Dialogue:
                messageText.color = Color.yellow;
                break;
            case LogType.System:
                messageText.color = Color.white;
                break;
            case LogType.Resource:
                messageText.color = Color.green;
                break;
            case LogType.Mission:
                messageText.color = Color.cyan;
                break;
        }

        // 수명 주기 시작
        StartCoroutine(LifeCycleRoutine());
    }

    IEnumerator LifeCycleRoutine()
    {
        // 1. 지정된 시간 동안 대기
        yield return new WaitForSeconds(displayTime);

        // 2. 서서히 투명해지기 (Fade Out)
        float elapsed = 0f;
        Color startColor = messageText.color;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            // 알파(투명도) 값을 1에서 0으로 줄임
            startColor.a = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            messageText.color = startColor;
            yield return null;
        }

        // 3. 완전히 투명해지면 삭제 (Vertical Layout Group이 알아서 자리 당겨줌)
        Destroy(gameObject);
    }
}