using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class SettingPanel : MonoBehaviour
{
    [Header("언어")]
    [SerializeField] private LanguageSelector languageSelector;

    [Header("그래픽")]
    [SerializeField] private Dropdown graphicDropdown;
    [SerializeField] private Dropdown displayModeDropdown;
    [SerializeField] private Dropdown resolutionDropdown;

    [Header("오디오")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("카메라")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider accelSlider;
    [SerializeField] private Slider shakeSlider;

    [Header("수치 표시")]
    [SerializeField] private Text masterValue;
    [SerializeField] private Text bgmValue;
    [SerializeField] private Text sfxValue;
    [SerializeField] private Text sensitivityValue;
    [SerializeField] private Text accelValue;
    [SerializeField] private Text shakeValue;

    [Header("버튼")]
    [SerializeField] private Button applyButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button defaultButton;

    [Header("번역 키")]
    [SerializeField]
    private string[] qualityKeys =
    {
        "settings.quality.veryLow", "settings.quality.low", "settings.quality.medium",
        "settings.quality.high", "settings.quality.veryHigh", "settings.quality.ultra"
    };

    private GameSetting draft;
    private bool isBound;

    public event Action Closed;

    public bool IsOpen => gameObject.activeSelf;


    private void Awake()
    {
        draft = new GameSetting();
        AutoBindUI();
        SetupControls();
        BindEvents();
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void AutoBindUI() // UI 자동화 함수
    {
        Transform root = transform;

        if (languageSelector == null) 
            languageSelector = GetComponentInChildren<LanguageSelector>(true);

        if (graphicDropdown == null) 
            graphicDropdown = UIBinder.FindInRow<Dropdown>(root, "Graphic Preset");
        if (displayModeDropdown == null) 
            displayModeDropdown = UIBinder.FindInRow<Dropdown>(root, "Display Mode");
        if (resolutionDropdown == null) 
            resolutionDropdown = UIBinder.FindInRow<Dropdown>(root, "Resolution");

        if (masterSlider == null) 
            masterSlider = UIBinder.FindInRow<Slider>(root, "Master Volume");
        if (bgmSlider == null) 
            bgmSlider = UIBinder.FindInRow<Slider>(root, "BGM Volume");
        if (sfxSlider == null) 
            sfxSlider = UIBinder.FindInRow<Slider>(root, "SFX Volume");

        if (sensitivitySlider == null) 
            sensitivitySlider = UIBinder.FindInRow<Slider>(root, "Camera Sensitivity");
        if (accelSlider == null) 
            accelSlider = UIBinder.FindInRow<Slider>(root, "Camera Acceleration");
        if (shakeSlider == null) 
            shakeSlider = UIBinder.FindInRow<Slider>(root, "Camera Shake");

        if (masterValue == null) 
            masterValue = UIBinder.FindInRow<Text>(root, "Master Volume", "Value Text");
        if (bgmValue == null) 
            bgmValue = UIBinder.FindInRow<Text>(root, "BGM Volume", "Value Text");
        if (sfxValue == null) 
            sfxValue = UIBinder.FindInRow<Text>(root, "SFX Volume", "Value Text");

        if (sensitivityValue == null) 
            sensitivityValue = UIBinder.FindInRow<Text>(root, "Camera Sensitivity", "Value Text");
        if (accelValue == null) 
            accelValue = UIBinder.FindInRow<Text>(root, "Camera Acceleration", "Value Text");
        if (shakeValue == null) 
            shakeValue = UIBinder.FindInRow<Text>(root, "Camera Shake", "Value Text");

        if (applyButton == null) 
            applyButton = UIBinder.Find<Button>(root, "Apply Button");
        if (cancelButton == null) 
            cancelButton = UIBinder.Find<Button>(root, "Cancel Button");
        if (defaultButton == null) 
            defaultButton = UIBinder.Find<Button>(root, "Default Button");
    }

    private void SetupControls() // 슬라이더 바를 설정하는 함수
    {
        SetupSlider(masterSlider, 0f, 1f);
        SetupSlider(bgmSlider, 0f, 1f);
        SetupSlider(sfxSlider, 0f, 1f);

        SetupSlider(sensitivitySlider, GameSetting.MinSensitivity, GameSetting.MaxSensitivity);
        SetupSlider(accelSlider, 0f, 1f);
        SetupSlider(shakeSlider, 0f, 1f);
    }

    private void BindEvents() // 각 UI에 이벤트를 할당하는 함수
    {
        if (isBound)
            return;

        if (languageSelector != null)
            languageSelector.Selected += OnLanguageSelected;

        BindDropdown(graphicDropdown, v => draft.qualityLevel = v);
        BindDropdown(displayModeDropdown, v => draft.displayModeIndex = v);
        BindDropdown(resolutionDropdown, v => draft.resolutionIndex = v);

        BindSlider(masterSlider, v => draft.masterVolume = v);
        BindSlider(bgmSlider, v => draft.bgmVolume = v);
        BindSlider(sfxSlider, v => draft.sfxVolume = v);

        BindSlider(sensitivitySlider, v => draft.mouseSensitivity = v);
        BindSlider(accelSlider, v => draft.cameraAccel = v);
        BindSlider(shakeSlider, v => draft.cameraShake = v);

        if (applyButton != null) 
            applyButton.onClick.AddListener(OnApply);
        if (cancelButton != null) 
            cancelButton.onClick.AddListener(OnCancel);
        if (defaultButton != null) 
            defaultButton.onClick.AddListener(OnDefault);

        isBound = true;
    }

    public void Open() // 설정 패널 열기 시 실행되는 함수
    {
        if (!SettingManager.HasInstance)
            return;

        gameObject.SetActive(true);
        draft = SettingManager.Instance.Current.Clone();
        RefreshDropdownLabels();
        RefreshUI();
    }

    public void Close() // 설정 패널 닫기 시 실행되는 함수
    {
        gameObject.SetActive(false);
        Closed?.Invoke();
    }

    public bool HandleCancelInput() // 설정 패널에서 게임 설정 적용 취소 시 실행되는 함수
    {
        if (!IsOpen)
            return false;

        OnCancel();
        return true;
    }

    private void RefreshDropdownLabels() // 드롭다운 메뉴 이름을 업데이트 하는 함수
    {
        SetupDropdown(graphicDropdown, GetQualityLabels());
        SetupDropdown(displayModeDropdown, GetDisplayModeLabels());
        SetupDropdown(resolutionDropdown, DisplayOptions.ResolutionNames);
    }

    private IReadOnlyList<string> GetQualityLabels() // 그래픽 프리셋 메뉴 이름을 반환하는 함수
    {
        int count = GameSetting.QualityLevelCount;
        string[] result = new string[count];

        for (int i = 0; i < count; i++)
        {
            bool hasKey = qualityKeys != null && i < qualityKeys.Length;
            result[i] = hasKey ? Loc.UI(qualityKeys[i]) : QualitySettings.names[i];
        }

        return result;
    }

    private static IReadOnlyList<string> GetDisplayModeLabels() // 출력 모드 이름을 반환하는 함수
    {
        int count = DisplayOptions.DisplayModes.Length;
        string[] result = new string[count];

        for (int i = 0; i < count; i++)
            result[i] = Loc.UI(DisplayOptions.DisplayModeKeys[i]);

        return result;
    }

    private static void SetupDropdown(Dropdown dropdown, IReadOnlyList<string> options) // 드롭다운 UI를 설정하는 함수
    {
        if (dropdown == null || options == null || options.Count == 0)
            return;

        int previous = dropdown.value;

        List<string> buffer = new List<string>(options.Count);
        for (int i = 0; i < options.Count; i++)
            buffer.Add(options[i]);

        dropdown.ClearOptions();
        dropdown.AddOptions(buffer);
        dropdown.SetValueWithoutNotify(Mathf.Clamp(previous, 0, options.Count - 1));
        dropdown.RefreshShownValue();
    }

    private static void SetupSlider(Slider slider, float min, float max) // 슬라이더 UI를 설정하는 함수
    {
        if (slider == null)
            return;

        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = false;
    }

    private void BindSlider(Slider slider, Action<float> setter) // 슬라이더에 함수를 할당하는 함수
    {
        if (slider == null)
            return;

        slider.onValueChanged.AddListener(v =>
        {
            setter(v);
            MarkDirty();
        });
    }

    private void BindDropdown(Dropdown dropdown, Action<int> setter) // 드롭다운에 함수를 할당하는 함수
    {
        if (dropdown == null)
            return;

        dropdown.onValueChanged.AddListener(v =>
        {
            setter(v);
            MarkDirty();
        });
    }

    private void OnLanguageSelected(string localeCode) // 언어를 선택하는 함수
    {
        PlayButtonSound();
        draft.languageCode = localeCode;
        LanguageApplier.SelectLocale(localeCode);
        MarkDirty();
    }

    private void OnLocaleChanged(Locale locale) // 언어 변경 시 실행되는 함수
    {
        RefreshDropdownLabels();
        RefreshDropdownValues();
        UpdateValueTexts();
    }

    private void RefreshUI() // UI를 업데이트하는 함수
    {
        if (draft == null)
            return;

        if (languageSelector != null)
            languageSelector.SetWithoutNotify(draft.languageCode);

        RefreshDropdownValues();

        SetSlider(masterSlider, draft.masterVolume);
        SetSlider(bgmSlider, draft.bgmVolume);
        SetSlider(sfxSlider, draft.sfxVolume);

        SetSlider(sensitivitySlider, draft.mouseSensitivity);
        SetSlider(accelSlider, draft.cameraAccel);
        SetSlider(shakeSlider, draft.cameraShake);

        MarkDirty();
    }

    private void RefreshDropdownValues() // 드롭다운 값을 업데이트하는 함수
    {
        if (draft == null)
            return;

        SetDropdown(graphicDropdown, draft.qualityLevel);
        SetDropdown(displayModeDropdown, draft.displayModeIndex);
        SetDropdown(resolutionDropdown, DisplayOptions.ResolveResolutionIndex(draft.resolutionIndex));
    }

    private static void SetDropdown(Dropdown dropdown, int value) // 드롭다운에 값을 설정하는 함수
    {
        if (dropdown == null)
            return;

        dropdown.SetValueWithoutNotify(value);
        dropdown.RefreshShownValue();
    }

    private static void SetSlider(Slider slider, float value) // 슬라이더에 값을 설정하는 함수
    {
        if (slider != null)
            slider.SetValueWithoutNotify(value);
    }

    private void MarkDirty() // 설정 변경 시 실행되는 함수
    {
        UpdateValueTexts();

        if (applyButton != null && SettingManager.HasInstance)
            applyButton.interactable = !draft.IsSameAs(SettingManager.Instance.Current);
    }

    private void UpdateValueTexts() // 값 텍스트를 업데이트하는 함수
    {
        if (draft == null)
            return;

        SetPercentText(masterValue, draft.masterVolume);
        SetPercentText(bgmValue, draft.bgmVolume);
        SetPercentText(sfxValue, draft.sfxVolume);
        SetPercentText(accelValue, draft.cameraAccel);
        SetPercentText(shakeValue, draft.cameraShake);

        if (sensitivityValue != null)
            sensitivityValue.text = draft.mouseSensitivity.ToString("0.0");
    }

    private static void SetPercentText(Text text, float value01) // 퍼센트 단위 텍스트를 설정하는 함수
    {
        if (text != null)
            text.text = Mathf.RoundToInt(value01 * 100f) + "%";
    }

    private void OnApply() // 설정 적용 버튼 클릭 시 실행되는 함수
    {
        PlayButtonSound();
        SettingManager.Instance.Commit(draft);
        draft = SettingManager.Instance.Current.Clone();
        RefreshUI();
        Close();
    }

    private void OnCancel() // 취소 버튼 클릭 시 실행되는 함수
    {
        PlayButtonSound();

        GameSetting current = SettingManager.Instance.Current;

        if (draft.languageCode != current.languageCode)
            LanguageApplier.SelectLocale(current.languageCode);

        draft = current.Clone();
        RefreshUI();
        Close();
    }

    private void OnDefault() // 기본값 버튼 클릭 시 실행되는 함수
    {
        PlayButtonSound();

        GameSetting defaults = new GameSetting();

        defaults.resolutionIndex = draft.resolutionIndex;
        defaults.displayModeIndex = draft.displayModeIndex;
        defaults.languageCode = draft.languageCode;

        draft = defaults;
        RefreshUI();
    }

    private static void PlayButtonSound() // 버튼음을 재생하는 함수
    {
        if (SoundManager.HasInstance)
            SoundManager.Instance.PlayButtonSound();
    }
}