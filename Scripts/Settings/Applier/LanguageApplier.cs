using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LanguageApplier : SettingApplierBase
{
    private string pendingLocaleCode;
    private Coroutine applyRoutine;

    protected override void Apply(GameSetting settings)
    {
        pendingLocaleCode = settings.languageCode;

        if (!isActiveAndEnabled)
            return;

        if (applyRoutine != null)
            StopCoroutine(applyRoutine);

        applyRoutine = StartCoroutine(ApplyWhenReady());
    }

    private IEnumerator ApplyWhenReady()
    {
        if (!LocalizationSettings.HasSettings)
        {
            Debug.LogError("[번역] LocalizationSettings 에셋이 없습니다. " +
                           "Edit > Project Settings > Localization에서 생성하세요.");
            applyRoutine = null;
            yield break;
        }

        yield return LocalizationSettings.InitializationOperation;

        SelectLocale(pendingLocaleCode);
        applyRoutine = null;
    }

    private static void SelectLocale(string localeCode)
    {
        Locale target = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier(localeCode));

        if (target == null)
        {
            Debug.LogWarning($"[번역] '{localeCode}' 로케일이 없습니다. " +
                             "Localization Settings의 Available Locales를 확인하세요.");
            return;
        }

        if (LocalizationSettings.SelectedLocale == target)
            return;

        LocalizationSettings.SelectedLocale = target;
    }
}