using UnityEngine;


[System.Serializable]
public class GameSetting
{
    public const int CurrentVersion = 1;
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

    public int qualityLevel = 3;
    public float mouseSensitivity = 2f;
    public float cameraAccel = 0f;
    public float cameraShake = 0.6f;

    public GameSetting Clone()
    {
        return (GameSetting)MemberwiseClone();
    }

    public void Validate()
    {
        qualityLevel = Mathf.Clamp(qualityLevel, 0, QualityLevelCount - 1);
        mouseSensitivity = Mathf.Clamp(mouseSensitivity, MinSensitivity, MaxSensitivity);
        cameraAccel = Mathf.Clamp01(cameraAccel);
        cameraShake = Mathf.Clamp01(cameraShake);
        version = CurrentVersion;
    }

    public bool IsSameAs(GameSetting other)
    {
        if (other == null) 
            return false;

        return qualityLevel == other.qualityLevel
            && Mathf.Approximately(mouseSensitivity, other.mouseSensitivity)
            && Mathf.Approximately(cameraAccel, other.cameraAccel)
            && Mathf.Approximately(cameraShake, other.cameraShake);
    }
}