using System;
using UnityEngine;
using UnityEngine.UI;

public class LanguageSelector : MonoBehaviour
{
    [Serializable]
    private class LanguageToggle
    {
        public string localeCode;
        public Toggle toggle;
        public Text label;
    }

    [SerializeField] private LanguageToggle[] toggles;
    [SerializeField] private ToggleGroup toggleGroup;

    public event Action<string> Selected;
    private bool suppressNotify;


    private void Awake()
    {
        if (toggleGroup == null)
            toggleGroup = GetComponent<ToggleGroup>();

        foreach (LanguageToggle item in toggles)
        {
            if (item == null || item.toggle == null)
                continue;

            if (toggleGroup != null)
                item.toggle.group = toggleGroup;

            if (item.label != null)
                item.label.text = GameLanguages.ToDisplayName(item.localeCode);

            string captured = item.localeCode;
            item.toggle.onValueChanged.AddListener(isOn => OnToggleChanged(captured, isOn));
        }
    }

    private void OnToggleChanged(string localeCode, bool isOn)
    {
        if (suppressNotify || !isOn)
            return;

        Selected?.Invoke(localeCode);
    }

    public void SetWithoutNotify(string localeCode)
    {
        suppressNotify = true;
        foreach (LanguageToggle item in toggles)
        {
            if (item?.toggle != null && item.localeCode == localeCode)
                item.toggle.SetIsOnWithoutNotify(true);
        }

        foreach (LanguageToggle item in toggles)
        {
            if (item?.toggle != null && item.localeCode != localeCode)
                item.toggle.SetIsOnWithoutNotify(false);
        }

        suppressNotify = false;
    }
}