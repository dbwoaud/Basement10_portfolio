using UnityEngine;

[System.Serializable]
public class GameSetting
{
    public const int CurrentVersion = 2;

    public const float MinSensitivity = 0.5f;
    public const float MaxSensitivity = 10f;

    public static int QualityLevelCount
    {
        get
        {
            string[] names = QualitySettings.names;
            return (names != null && names.Length > 0) ? names.Length : 1;
        }
    }

    public int version = CurrentVersion;

    [Header("언어")]
    public string languageCode = GameLanguages.Korean;

    [Header("그래픽")]
    public int qualityLevel = 3;
    public int displayModeIndex = 0;
    public int resolutionIndex = -1;

    [Header("오디오")]
    public float masterVolume = 1f;
    public float bgmVolume = 0.8f;
    public float sfxVolume = 1f;

    [Header("카메라")]
    public float mouseSensitivity = 2f;
    public float cameraAccel = 0f;
    public float cameraShake = 0.6f;

    public GameSetting Clone() => (GameSetting)MemberwiseClone(); // 복사본을 생성하는 함수

    public static GameSetting CreateDefault() // 시스템 언어를 기본값으로 설정하는 함수
    {
        return new GameSetting
        {
            languageCode = GameLanguages.FromSystemLanguage(Application.systemLanguage)
        };
    }

    public void Validate() // 설정 값을 유효값으로 변환하는 함수
    {
        if (!GameLanguages.IsSupported(languageCode))
            languageCode = GameLanguages.Korean;

        qualityLevel = Mathf.Clamp(qualityLevel, 0, QualityLevelCount - 1);
        displayModeIndex = Mathf.Clamp(displayModeIndex, 0, DisplayOptions.DisplayModeNames.Length - 1);

        if (resolutionIndex >= 0)
            resolutionIndex = Mathf.Clamp(resolutionIndex, 0, DisplayOptions.ResolutionCount - 1);

        masterVolume = Mathf.Clamp01(masterVolume);
        bgmVolume = Mathf.Clamp01(bgmVolume);
        sfxVolume = Mathf.Clamp01(sfxVolume);

        mouseSensitivity = Mathf.Clamp(mouseSensitivity, MinSensitivity, MaxSensitivity);
        cameraAccel = Mathf.Clamp01(cameraAccel);
        cameraShake = Mathf.Clamp01(cameraShake);

        version = CurrentVersion;
    }

    public bool IsSameAs(GameSetting other) // 설정 값이 동일한지 확인하는 함수
    {
        if (other == null)
            return false;

        return languageCode == other.languageCode
            && qualityLevel == other.qualityLevel
            && displayModeIndex == other.displayModeIndex
            && resolutionIndex == other.resolutionIndex
            && Mathf.Approximately(masterVolume, other.masterVolume)
            && Mathf.Approximately(bgmVolume, other.bgmVolume)
            && Mathf.Approximately(sfxVolume, other.sfxVolume)
            && Mathf.Approximately(mouseSensitivity, other.mouseSensitivity)
            && Mathf.Approximately(cameraAccel, other.cameraAccel)
            && Mathf.Approximately(cameraShake, other.cameraShake);
    }

    public bool IsDisplayChangedFrom(GameSetting other) // 해상도나 창모드가 달라졌는지 확인하는 함수
    {
        if (other == null)
            return false;

        return resolutionIndex != other.resolutionIndex
            || displayModeIndex != other.displayModeIndex;
    }
}