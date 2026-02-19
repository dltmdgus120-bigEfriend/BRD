using UnityEngine;

public class LogManager : MonoBehaviour
{
    public static LogManager Instance;

    [Header("UI 연결")]
    public Transform logContainer; // 텍스트들이 쌓일 부모 패널 (Vertical Layout Group 필수)
    public GameObject logPrefab;   // 아까 만든 LogMessage가 붙은 프리팹

    void Awake()
    {
        Instance = this;
    }

   
    public void ShowLog(string message, LogType type = LogType.System)
    {
        if (logPrefab == null || logContainer == null) return;

        
        GameObject newLog = Instantiate(logPrefab, logContainer);
      
        LogMessage logScript = newLog.GetComponent<LogMessage>();
        if (logScript != null)
        {
            logScript.Setup(message, type);
        }

        
        // 자식이 10개 이상이면 제일 위에 있는(오래된) 자식을 강제로 지우기 (렉 방지)
        if (logContainer.childCount > 10)
        {
            Destroy(logContainer.GetChild(0).gameObject);
        }
    }
}