using System;
using System.Collections.Generic;
using UnityEngine;


public static class DisplayOptions
{
    public static readonly FullScreenMode[] DisplayModes = 
    {
        FullScreenMode.ExclusiveFullScreen,
        FullScreenMode.FullScreenWindow,
        FullScreenMode.Windowed
    };

    public static readonly string[] DisplayModeKeys =
    {
        "settings.display.fullscreen",
        "settings.display.borderless",
        "settings.display.windowed"
    };

    public static readonly string[] DisplayModeNames =
    {
        "전체 화면", "테두리 없는 창", "창 모드"
    };

    private static Resolution[] cachedResolutions;
    private static string[] cachedResolutionNames;

    public static int ResolutionCount
    {
        get
        {
            EnsureCache();
            return cachedResolutions.Length;
        }
    }

    public static IReadOnlyList<Resolution> Resolutions
    {
        get
        {
            EnsureCache();
            return cachedResolutions;
        }
    }

    public static IReadOnlyList<string> ResolutionNames
    {
        get
        {
            EnsureCache();
            return cachedResolutionNames;
        }
    }

    public static int GetCurrentResolutionIndex() // 현재 해상도 인덱스를 반환하는 함수
    {
        EnsureCache();

        int best = 0;
        long bestDiff = long.MaxValue;

        for (int i = 0; i < cachedResolutions.Length; i++)
        {
            long dw = cachedResolutions[i].width - Screen.width;
            long dh = cachedResolutions[i].height - Screen.height;
            long diff = dw * dw + dh * dh;

            if (diff < bestDiff)
            {
                bestDiff = diff;
                best = i;
            }
        }

        return best;
    }

    public static int ResolveResolutionIndex(int stored) // 해상도 인덱스를 적용하는 함수
    {
        EnsureCache();

        if (stored < 0)
            return GetCurrentResolutionIndex();

        return Mathf.Clamp(stored, 0, cachedResolutions.Length - 1);
    }

    private static void EnsureCache() // 현재 해상도를 캐시에 저장하는 함수
    {
        if (cachedResolutions != null)
            return;

        Resolution[] raw = Screen.resolutions;

        if (raw == null || raw.Length == 0)
            cachedResolutions = new[] { Screen.currentResolution };
        
        else
        {
            Dictionary<long, Resolution> unique = new Dictionary<long, Resolution>(raw.Length);
            foreach (Resolution r in raw)
            {
                long key = ((long)r.width << 32) | (uint)r.height;
                if (!unique.TryGetValue(key, out Resolution exist) || RefreshRateOf(r) > RefreshRateOf(exist))
                    unique[key] = r;
            }

            cachedResolutions = new Resolution[unique.Count];
            unique.Values.CopyTo(cachedResolutions, 0);
            Array.Sort(cachedResolutions, (a, b) => (a.width * a.height).CompareTo(b.width * b.height));
        }

        cachedResolutionNames = new string[cachedResolutions.Length];

        for (int i = 0; i < cachedResolutions.Length; i++)
            cachedResolutionNames[i] = $"{cachedResolutions[i].width} x {cachedResolutions[i].height}";
    }

    private static double RefreshRateOf(Resolution r) // 주사율을 갱신하는 함수
    {
        return r.refreshRateRatio.value;
    }
}