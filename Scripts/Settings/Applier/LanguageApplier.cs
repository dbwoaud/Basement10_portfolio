using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LanguageApplier : SettingApplierBase
{
    private string pendingLocaleCode; // 현재 설정 언어 코드
    private Coroutine applyRoutine;

    protected override void Apply(GameSetting settings) // 게임 설정을 적용하는 함수
    {
        pendingLocaleCode = settings.languageCode;

        if (!isActiveAndEnabled)
            return;

        if (applyRoutine != null)
            StopCoroutine(applyRoutine);

        applyRoutine = StartCoroutine(ApplyCoroutine());
    }

    private IEnumerator ApplyCoroutine() // 게임 설정을 적용하는 코루틴 
    {
        if (!LocalizationSettings.HasSettings)
        {
            applyRoutine = null;
            yield break;
        }

        yield return LocalizationSettings.InitializationOperation;

        SelectLocale(pendingLocaleCode);
        applyRoutine = null;
    }

    public static void SelectLocale(string localeCode) // 언어를 선택하는 함수
    {
        Locale target = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier(localeCode));

        if (target == null)
            return;
        
        if (LocalizationSettings.SelectedLocale == target)
            return;

        LocalizationSettings.SelectedLocale = target;
    }
}