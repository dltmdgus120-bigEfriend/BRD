using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // 리스트 검색용

// 인스펙터에서 히든 조합을 설정하기 위한 데이터 틀
[System.Serializable]
public struct HiddenCommandData
{
    public string command;           // 명령어 (예: "에르핀 굶음")
    public CombinationRecipe recipe; // 연결될 레시피 (예: 에르핀+네르 -> 각성 에르핀)
}

public class ChatSystem : MonoBehaviour
{
    public static ChatSystem Instance;

    [Header("UI 연결")]
    public TMP_InputField chatInput;
    public Transform chatContent;
    public ScrollRect chatScrollRect;
    public CanvasGroup chatCanvasGroup;

    [Header("설정")]
    public float hideDelay = 3.0f;      // 채팅창 꺼지는 시간
    public GameObject chatTextPrefab;

    [Header("★ 히든 조합 리스트")]
    public List<HiddenCommandData> hiddenCommands; // 여기에 명령어를 쭉 추가하면 됩니다!

    private float lastInteractionTime;
    private bool isChatVisible = true;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        chatInput.onSubmit.AddListener(OnSubmitChat);
        ShowChatUI(); // 시작할 때 한 번 보여줌
    }

    void Update()
    {
        // 1. 엔터 키 감지
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // 엔터를 누르면 무조건 채팅창을 켬 (순서 중요!)
            ShowChatUI();

            // 입력창에 포커스가 없다면 포커스 줌
            if (!chatInput.isFocused)
            {
                chatInput.ActivateInputField();
            }
        }

        // 2. 자동 숨김 로직
        // (채팅창이 켜져있고 + 입력 중이 아닐 때만 시간을 잼)
        if (isChatVisible && !chatInput.isFocused)
        {
            if (Time.time - lastInteractionTime > hideDelay)
            {
                HideChatUI();
            }
        }
    }

    // 채팅창 켜기 (상호작용 가능하게 만듦)
    public void ShowChatUI()
    {
        isChatVisible = true;
        lastInteractionTime = Time.time; // 시간 갱신

        if (chatCanvasGroup != null)
        {
            chatCanvasGroup.alpha = 1f;
            chatCanvasGroup.interactable = true; // ★ 이게 켜져야 입력이 됩니다
            chatCanvasGroup.blocksRaycasts = true;
        }
    }

    // 채팅창 숨기기
    void HideChatUI()
    {
        isChatVisible = false;

        if (chatCanvasGroup != null)
        {
            chatCanvasGroup.alpha = 0f; // 투명하게
            chatCanvasGroup.interactable = false; // 클릭 방지
            chatCanvasGroup.blocksRaycasts = false;
        }
    }

    public void OnSubmitChat(string text)
    {
        // 엔터 쳤으니 시간 초기화
        lastInteractionTime = Time.time;

        // 내용 없으면 그냥 커서만 끄기 (채팅창은 유지)
        if (string.IsNullOrWhiteSpace(text))
        {
            chatInput.DeactivateInputField();
            return;
        }

        // 채팅 출력
        AddChatMessage($"<color=yellow>플레이어:</color> {text}");

        // ★ 히든 명령어 체크
        CheckHiddenCommand(text);

        // 입력창 비우고 커서 유지 (계속 채팅 칠 수 있게)
        chatInput.text = "";
        chatInput.ActivateInputField();
    }

    public void AddChatMessage(string message)
    {
        ShowChatUI(); // 메시지 오면 켜기

        GameObject newText = Instantiate(chatTextPrefab, chatContent);
        newText.GetComponent<TMP_Text>().text = message;

        StartCoroutine(AutoScroll());
    }

    System.Collections.IEnumerator AutoScroll()
    {
        yield return null;
        chatScrollRect.verticalNormalizedPosition = 0f;
    }

    // --- ★ 확장성 있는 히든 조합 로직 ---
    void CheckHiddenCommand(string input)
    {
        string cleanInput = input.Trim(); // 공백 제거

        // 1. 리스트에서 명령어와 일치하는 데이터 찾기
        foreach (var hidden in hiddenCommands)
        {
            if (hidden.command == cleanInput)
            {
                // 찾았다! 레시피 실행 시도
                TryHiddenRecipe(hidden.recipe);
                return;
            }
        }

        // (치트키 등은 여기에 하드코딩으로 남겨도 됨)
        if (cleanInput == "showmethemoney")
        {
            AddChatMessage("<color=red>[치트] 자원이 증가했습니다.</color>");
        }
    }


    // 맵 전체에서 재료를 찾아 조합하는 함수  
    void TryHiddenRecipe(CombinationRecipe recipe)
    {
        if (recipe == null) return;

        // 1. 재료 찾기
        List<UnitStat> allUnits = FindObjectsOfType<UnitStat>().ToList();
        List<UnitStat> ingredientsToDestroy = new List<UnitStat>();

        foreach (var requiredData in recipe.ingredients)
        {
            var target = allUnits.FirstOrDefault(u => u.data == requiredData && !ingredientsToDestroy.Contains(u));

            if (target != null)
            {
                ingredientsToDestroy.Add(target);
            }          
      }

        // 2. 소환 위치 잡기 (첫 번째 재료 위치)
        Vector3 spawnPos = ingredientsToDestroy[0].transform.position;

        // 3. 재료 삭제 (선택 해제 포함)
        var rts = FindObjectOfType<RTSController>();
        foreach (var unit in ingredientsToDestroy)
        {
            if (rts != null && rts.selectedUnits.Contains(unit.GetComponent<UnityEngine.AI.NavMeshAgent>()))
            {
                rts.selectedUnits.Remove(unit.GetComponent<UnityEngine.AI.NavMeshAgent>());
            }
            Destroy(unit.gameObject);
        }

        // 선택 초기화 (찌꺼기 제거)
        if (rts != null) rts.ClearSelection();

        // 4. 핵심 수정: 5성 유닛 소환 및 Warp 적용 (에러 방지)
        GameObject newUnit = Instantiate(recipe.resultUnit.prefab, spawnPos, Quaternion.identity);

        var agent = newUnit.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            // (1) 일단 끈다 (생성 직후 에러 방지)
            agent.enabled = false;

            // (2) 위치를 확실하게 잡는다
            newUnit.transform.position = spawnPos;

            // (3) 다시 켠다
            agent.enabled = true;

            // (4) Warp로 바닥에 안착시킨다 (반경 5m 내 바닥 찾기)
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(spawnPos, out hit, 5.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
        }

        // 5. 데이터 주입
        UnitStat newStat = newUnit.GetComponent<UnitStat>();
        if (newStat != null) newStat.data = recipe.resultUnit;

        // 6. 등장 사운드
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayVoice(recipe.resultUnit.summonVoice);
        }
       

     
       //조합 성공 알림 및 캐릭터 등장 대사 로그 띄우기
       
        if (LogManager.Instance != null)
        {
            // 1. 히든 조합 성공 시스템 로그 (하늘색으로 강조)
            LogManager.Instance.ShowLog($"<color=red>[히든 조합 성공]</color> {recipe.resultUnit.unitName} 등장!", LogType.Mission);

            // 2. 캐릭터 대사 띄우기 (설명이 있을 때만)
            if (!string.IsNullOrEmpty(recipe.resultUnit.description))
            {
                string dialogueText = $"<color=orange>[{recipe.resultUnit.unitName}]</color> {recipe.resultUnit.description}";
                LogManager.Instance.ShowLog(dialogueText, LogType.Dialogue);
            }
        }
        

        // 7. 편의성: 소환된 히든 유닛 바로 선택해주기
        if (rts != null && agent != null)
        {
            rts.SelectUnit(agent);
        }
    }
}
