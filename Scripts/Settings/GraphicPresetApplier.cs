using UnityEngine;

public class GraphicPresetApplier : SettingApplierBase
{
    [Header("추가 제어")]
    [SerializeField] private bool overrideShadowDistance = false;
    [SerializeField] private float[] shadowDistances = { 20f, 35f, 50f, 80f, 120f, 160f };

    protected override void Apply(GameSetting settings)
    {
        int level = Mathf.Clamp(settings.qualityLevel, 0, GameSetting.QualityLevelCount - 1);
        QualitySettings.SetQualityLevel(level, true);
        if (overrideShadowDistance
            && shadowDistances != null
            && level < shadowDistances.Length)
        {
            QualitySettings.shadowDistance = shadowDistances[level];
        }
    }
}