using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

public class TextSizeSynchronizer : MonoBehaviour
{
    [SerializeField] private List<Text> targetTexts = new List<Text>();
    [SerializeField] private int minSizeLimit = 15;
    [SerializeField] private int maxSizeLimit = 30;

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(SyncRoutine());
        }
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(UnityEngine.Localization.Locale locale)
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(SyncRoutine());
        }
    }

    public void Synchronize()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(SyncRoutine());
        }
    }

    private IEnumerator SyncRoutine()
    {
        // Wait for end of frame to ensure all UI Layout Groups and Text generators are fully updated
        yield return new WaitForEndOfFrame();

        if (targetTexts == null || targetTexts.Count == 0) yield break;

        int minFontSize = maxSizeLimit;

        // Step 1: Temporarily enable Best Fit to calculate the maximum size each label can occupy
        foreach (Text txt in targetTexts)
        {
            if (txt == null) continue;
            txt.resizeTextForBestFit = true;
            txt.resizeTextMinSize = minSizeLimit;
            txt.resizeTextMaxSize = maxSizeLimit;
        }

        // Force Canvas update so Unity's TextGenerator runs Best Fit calculation immediately
        Canvas.ForceUpdateCanvases();

        // Step 2: Find the smallest font size used among all text components
        foreach (Text txt in targetTexts)
        {
            if (txt == null) continue;
            int usedSize = txt.cachedTextGenerator.fontSizeUsedForBestFit;
            if (usedSize > 0 && usedSize < minFontSize)
            {
                minFontSize = usedSize;
            }
        }

        // Step 3: Turn off Best Fit and strictly lock all labels to the synchronized minimum font size
        foreach (Text txt in targetTexts)
        {
            if (txt == null) continue;
            txt.resizeTextForBestFit = false;
            txt.fontSize = minFontSize;
        }
    }
}
