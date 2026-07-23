using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public static class Loc
{
    public const string UITable = "UI";
    public const string StoryTable = "Story";

    public static bool IsReady => LocalizationSettings.HasSettings && LocalizationSettings.InitializationOperation.IsDone;

    public static string UI(string key) => Get(UITable, key); // UI 테이블의 Key를 반환하는 함수

    public static string Story(string key) => Get(StoryTable, key); // Story 테이블의 Key를 반환하는 함수

    public static string Get(string table, string key) // 테이블의 Key를 반환하는 함수
    {
        if (string.IsNullOrEmpty(key))
            return string.Empty;

        if (!IsReady)
            return key;
        

        try
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString(table, key);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[번역] '{table}/{key}' 조회 실패: {e.Message}");
            return key;
        }
    }

    public static string Get(string table, string key, params object[] args) // 테이블의 Key를 반환하는 함수
    {
        if (string.IsNullOrEmpty(key))
            return string.Empty;

        if (!IsReady)
            return key;

        try
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString(table, key, args);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[번역] '{table}/{key}' 조회 실패: {e.Message}");
            return key;
        }
    }

    public static string CurrentLocaleCode // 현재 지역 코드를 반환하는 함수
    {
        get
        {
            if (!LocalizationSettings.HasSettings)
                return GameLanguages.Korean;

            Locale locale = LocalizationSettings.SelectedLocale;
            return locale != null ? locale.Identifier.Code : GameLanguages.Korean;
        }
    }
}