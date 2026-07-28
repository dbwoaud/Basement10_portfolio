using UnityEngine;

public abstract class SettingApplierBase : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        SettingManager.OnSettingsApplied += HandleApplied;

        if (SettingManager.HasInstance && SettingManager.Instance.Current != null)
            HandleApplied(SettingManager.Instance.Current);
    }

    protected virtual void OnDisable()
    {
        SettingManager.OnSettingsApplied -= HandleApplied;
    }

    private void HandleApplied(GameSetting settings) // 게임 설정 적용을 처리하는 함수
    {
        if (settings != null)
            Apply(settings);
    }

    protected abstract void Apply(GameSetting settings); // 게임 설정을 적용하는 함수
}