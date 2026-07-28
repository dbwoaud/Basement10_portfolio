using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class TextSizeSynchronizer : MonoBehaviour
{
    [SerializeField] private List<Text> targetTexts = new List<Text>();
    [SerializeField] private int minSizeLimit = 15;
    [SerializeField] private int maxSizeLimit = 30;

    private Coroutine syncRoutine;


    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        Synchronize();
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        syncRoutine = null;
    }

    private void OnLocaleChanged(Locale locale) // 언어 변경 시 실행되는 함수
    {
        Synchronize();
    }

    public void Synchronize() // 언어에 따라 바뀌는 텍스트 길이에 맞춰 텍스트 크기를 설정하는 함수
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (syncRoutine != null)
            StopCoroutine(syncRoutine);

        syncRoutine = StartCoroutine(SyncRoutine());
    }

    private IEnumerator SyncRoutine() // 언어에 따라 바뀌는 텍스트 길이에 맞춰 텍스트 크기를 설정하는 코루틴
    {
        yield return new WaitForEndOfFrame();

        if (targetTexts == null || targetTexts.Count == 0)
        {
            syncRoutine = null;
            yield break;
        }

        int minFontSize = maxSizeLimit;
        bool anyMeasured = false;

        foreach (Text txt in targetTexts)
        {
            if (!IsMeasurable(txt))
                continue;

            txt.resizeTextForBestFit = true;
            txt.resizeTextMinSize = minSizeLimit;
            txt.resizeTextMaxSize = maxSizeLimit;
        }

        Canvas.ForceUpdateCanvases();

        foreach (Text txt in targetTexts)
        {
            if (!IsMeasurable(txt))
                continue;

            float scale = (txt.canvas != null) ? txt.canvas.scaleFactor : 1f;
            if (scale <= 0f)
                scale = 1f;

            int usedSize = Mathf.RoundToInt(txt.cachedTextGenerator.fontSizeUsedForBestFit / scale);

            if (usedSize > 0)
            {
                anyMeasured = true;

                if (usedSize < minFontSize)
                    minFontSize = usedSize;
            }
        }

        if (!anyMeasured)
        {
            RestoreBestFitOff();
            syncRoutine = null;
            yield break;
        }

        minFontSize = Mathf.Clamp(minFontSize, minSizeLimit, maxSizeLimit);
        foreach (Text txt in targetTexts)
        {
            if (txt == null)
                continue;

            txt.resizeTextForBestFit = false;
            txt.fontSize = minFontSize;
        }

        syncRoutine = null;
    }

    private static bool IsMeasurable(Text txt) // 크기 측정이 가능한 텍스트인지 확인하는 함수
    {
        if (txt == null || !txt.gameObject.activeInHierarchy)
            return false;

        Rect rect = txt.rectTransform.rect;
        return rect.width > 1f && rect.height > 1f;
    }

    private void RestoreBestFitOff() // 측정 실패 시 Best Fit 상태를 복구하는 함수
    {
        foreach (Text txt in targetTexts)
        {
            if (txt != null)
                txt.resizeTextForBestFit = false;
        }
    }
}
