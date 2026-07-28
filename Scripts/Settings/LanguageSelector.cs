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
                item.label.text = GameLanguages.GetLanguageName(item.localeCode);

            string captured = item.localeCode;
            item.toggle.onValueChanged.AddListener(isOn => OnToggleChanged(captured, isOn));
        }
    }

    private void OnToggleChanged(string localeCode, bool isOn) // 토글 값 변경 시 실행되는 함수
    {
        if (suppressNotify || !isOn)
            return;

        Selected?.Invoke(localeCode);
    }

    public void SetWithoutNotify(string localeCode) // 노티파이 없이 토글 값을 변경하는 함수
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