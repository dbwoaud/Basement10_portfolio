using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public static class Loc
{
    public const string UITable = "UI";
    public const string StoryTable = "Story";

    public static bool IsReady =>
        LocalizationSettings.HasSettings && LocalizationSettings.InitializationOperation.IsDone;

    public static string UI(string key) => Get(UITable, key); // UI 테이블의 Key를 반환하는 함수

    public static string Story(string key) => Get(StoryTable, key); // Story 테이블의 Key를 반환하는 함수


    public static IEnumerator EnsureReady() // 로컬라이제이션 초기화가 끝날 때까지 대기하는 코루틴
    {
        if (!LocalizationSettings.HasSettings)
            yield break;
        
        yield return LocalizationSettings.InitializationOperation;
    }

    public static string Get(string table, string key) // 테이블의 Key를 반환하는 함수
    {
        return Resolve(table, key, null);
    }

    public static string Get(string table, string key, params object[] args) // 서식 인자를 적용해 Key를 반환하는 함수
    {
        return Resolve(table, key, args);
    }

    private static string Resolve(string table, string key, object[] args) // 우선순위대로 번역을 조회하는 함수
    {
        if (string.IsNullOrEmpty(key))
            return string.Empty;

        if (!IsReady)
            return Fallback(key);
        
        string current = TryGetFrom(table, key, null, args);
        if (current != null)
            return current;

        string english = TryGetFrom(table, key, FindLocale(GameLanguages.English), args);
        if (english != null)
            return english;

        string korean = TryGetFrom(table, key, FindLocale(GameLanguages.Korean), args);
        if (korean != null)
            return korean;
        
        return Fallback(key);
    }

    private static string TryGetFrom(string table, string key, Locale locale, object[] args) // 특정 언어 테이블에서 값을 읽는 함수
    {
        try
        {
            StringTable stringTable = LocalizationSettings.StringDatabase.GetTable(table, locale);
            if (stringTable == null)
                return null;

            StringTableEntry entry = stringTable.GetEntry(key);
            if (entry == null)
                return null;

            string value = (args != null && args.Length > 0)
                ? entry.GetLocalizedString(args)
                : entry.GetLocalizedString();

            return string.IsNullOrEmpty(value) ? null : value;
        }
        catch
        {
            return null;
        }
    }

    private static Locale FindLocale(string localeCode) // 언어 코드로 지역을 찾는 함수
    {
        if (!LocalizationSettings.HasSettings)
            return null;

        return LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier(localeCode));
    }


    private static string Fallback(string key) // 조회 실패 시 대체 문자열을 반환하는 함수
    {
#if UNITY_EDITOR
        return key;
#else
        return string.Empty;
#endif
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

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/번역/누락 항목 검사")]
    private static void ValidateAllTables()
    {
        if (!LocalizationSettings.HasSettings)
        {
            Debug.LogError("[번역] LocalizationSettings 에셋이 없습니다.");
            return;
        }

        int missing = 0;
        foreach (string tableName in new[] { UITable, StoryTable })
        {
            foreach (string code in GameLanguages.Supported)
            {
                Locale locale = FindLocale(code);
                if (locale == null)
                {
                    Debug.LogError($"[번역] 로케일 '{code}'가 Available Locales에 없습니다.");
                    continue;
                }

                StringTable table = LocalizationSettings.StringDatabase.GetTable(tableName, locale);
                if (table == null)
                {
                    Debug.LogError($"[번역] '{tableName}' 테이블의 '{code}' 버전이 없습니다.");
                    continue;
                }

                foreach (var shared in table.SharedData.Entries)
                {
                    StringTableEntry entry = table.GetEntry(shared.Id);

                    if (entry == null || string.IsNullOrWhiteSpace(entry.Value))
                    {
                        missing++;
                        Debug.LogWarning($"[번역] 비어 있음 — {tableName} / {shared.Key} / {code}");
                    }
                }
            }
        }

        if (missing == 0)
            Debug.Log("[번역] 검사 완료. 누락 항목이 없습니다.");
        else
            Debug.LogError($"[번역] 검사 완료. 누락 항목 {missing}개를 발견했습니다.");
    }
#endif
}