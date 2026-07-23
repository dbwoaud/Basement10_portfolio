using UnityEngine;

public abstract class SettingApplierBase : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        SettingManager.OnSettingsApplied += Apply;
        if (SettingManager.instance != null && SettingManager.instance.Current != null)
            Apply(SettingManager.instance.Current);
    }

    protected virtual void OnDisable()
    {
        SettingManager.OnSettingsApplied -= Apply;
    }

    protected abstract void Apply(GameSetting settings);
}