using UnityEngine;

public class DisplayApplier : SettingApplierBase
{
    protected override void Apply(GameSetting settings)
    {
        int modeIndex = Mathf.Clamp(settings.displayModeIndex, 0, DisplayOptions.DisplayModes.Length - 1);
        FullScreenMode mode = DisplayOptions.DisplayModes[modeIndex];

        int resIndex = DisplayOptions.ResolveResolutionIndex(settings.resolutionIndex);
        Resolution target = DisplayOptions.Resolutions[resIndex];

        bool sameSize = Screen.width == target.width && Screen.height == target.height;
        bool sameMode = Screen.fullScreenMode == mode;

        if (sameSize && sameMode)
            return;

        Screen.SetResolution(target.width, target.height, mode, target.refreshRateRatio);
    }
}