using UnityEngine;

public abstract class SettingApplierBase : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        SettingManager.OnSettingsApplied += HandleApplied;

        if (SettingManager.HasInstance && SettingManager.instance.Current != null)
            HandleApplied(SettingManager.instance.Current);
    }

    protected virtual void OnDisable()
    {
        SettingManager.OnSettingsApplied -= HandleApplied;
    }

    private void HandleApplied(GameSetting settings)
    {
        if (settings != null)
            Apply(settings);
    }

    protected abstract void Apply(GameSetting settings);
}