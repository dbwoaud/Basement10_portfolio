using System.Collections.Generic;
using UnityEngine;

public static class GameLanguages
{
    public const string Korean = "ko";
    public const string English = "en";
    public const string Japanese = "ja";
    public const string ChineseSimplified = "zh-Hans";

    public static readonly string[] Supported =
    {
        Korean, English, Japanese, ChineseSimplified
    };

    public static string ToDisplayName(string localeCode) // 지역에 따라 언어 텍스트를 반환하는 함수
    {
        switch (localeCode)
        {
            case Korean: 
                return "한국어";
            case English: 
                return "English";
            case Japanese: 
                return "日本語";
            case ChineseSimplified: 
                return "简体中文";
            default: 
                return localeCode;
        }
    }

    public static bool IsSupported(string localeCode) // 로컬라이징을 지원하는 언어인지 확인하는 함수
    {
        if (string.IsNullOrEmpty(localeCode))
            return false;

        foreach (string code in Supported)
        {
            if (code == localeCode)
                return true;
        }

        return false;
    }

    public static string FromSystemLanguage(SystemLanguage system) // 시스템 언어에 따라 언어를 설정하는 함수
    {
        switch (system)
        {
            case SystemLanguage.Korean:
                return Korean;

            case SystemLanguage.Japanese:
                return Japanese;

            case SystemLanguage.Chinese:
            case SystemLanguage.ChineseSimplified:
                return ChineseSimplified;

            default:
                return English;
        }
    }
}