using UnityEngine;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    public Slider bgmSlider;
    public Slider sfxSlider;

    void Start()
    {
        // ★ 핵심: 슬라이더가 움직일 때 실행할 함수를 '코드로' 연결합니다.
        // 이렇게 하면 인스펙터에서 일일이 연결 안 해도 됩니다!

        // 1. BGM 슬라이더 연결
        bgmSlider.onValueChanged.AddListener((float val) =>
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.SetBGMVolume(val);
        });

        // 2. SFX 슬라이더 연결
        sfxSlider.onValueChanged.AddListener((float val) =>
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.SetSFXVolume(val);
        });
    }

    void OnEnable()
    {
        // 패널이 켜질 때, 현재 볼륨 상태를 슬라이더에 반영 (동기화)
        if (SoundManager.Instance != null)
        {
            bgmSlider.value = SoundManager.Instance.bgmSource.volume;
            sfxSlider.value = SoundManager.Instance.sfxSource.volume;
        }
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}