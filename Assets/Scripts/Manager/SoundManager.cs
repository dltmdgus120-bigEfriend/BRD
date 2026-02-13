using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // 어디서든 SoundManager.Instance로 부를 수 있게 함
    public static SoundManager Instance;

    [Header("오디오 소스 연결")]
    public AudioSource voiceSource; // 목소리 전용 스피커
    public AudioSource sfxSource;   // 효과음 전용 스피커 (나중에 필요할까봐)
    public AudioSource bgmSource;  //배경 음악용 스피커

    [Header("배경음악 설정")]
    public AudioClip defaultBGM;    // ★ 추가: 게임 시작하면 틀 음악

    void Awake()
    {
        // 싱글톤 패턴 (이미 있으면 나를 파괴, 없으면 나를 등록)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ★ 씬이 넘어가도 파괴되지 않음!
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // ★ 추가: 시작하자마자 BGM 재생
        if (defaultBGM != null)
        {
            PlayBGM(defaultBGM);
        }
    }

    // 외부에서 이 함수를 호출해서 소리를 냄
    public void PlayVoice(AudioClip clip)
    {
        if (clip != null && voiceSource != null)
        {
            // PlayOneShot은 소리가 겹쳐도 끊기지 않고 겹쳐서 재생해줍니다!
            voiceSource.PlayOneShot(clip);
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        // 이미 같은 음악이 나오고 있다면 다시 틀지 않음 (끊김 방지)
        if (bgmSource.clip == clip) return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // 옵션창 슬라이더에서 호출할 볼륨 조절 함수들
    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
        voiceSource.volume = volume; // 목소리랑 효과음 같이 조절
    }
}