using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    [Header("해상도 되돌리기 확인")]
    [SerializeField] private GameObject confirmPopup;
    [SerializeField] private Text confirmCountdownText;
    [SerializeField] private Button confirmKeepButton;
    [SerializeField] private float revertCountdown = 15f;

    [Header("품질 단계 번역 키")]
    [SerializeField]
    private string[] qualityKeys =
    {
        "settings.quality.veryLow", "settings.quality.low", "settings.quality.medium",
        "settings.quality.high", "settings.quality.veryHigh", "settings.quality.ultra"
    };

    private GameSetting draft;
    private GameSetting snapshotBeforeApply;
    private Coroutine revertRoutine;
    private bool isBound;

    public event System.Action Closed;

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
        LocalizationManager.LanguageChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        LocalizationManager.LanguageChanged -= OnLanguageChanged;
    }

    public void Open()
    {
        if (!SettingManager.HasInstance)
        {
            Debug.LogError("[설정] SettingManager가 없습니다.");
            return;
        }

        gameObject.SetActive(true);
        draft = SettingManager.Instance.Current.Clone();
        RefreshUI();
    }

    public void Close()
    {
        EndCountdown();
        gameObject.SetActive(false);
        Closed?.Invoke();
    }

    public bool HandleCancelInput()
    {
        if (!IsOpen)
            return false;

        if (confirmPopup != null && confirmPopup.activeSelf)
            return true;

        OnCancel();
        return true;
    }
    private void AutoBindUI()
    {
        Transform root = transform;

        if (languageSelector == null) languageSelector = GetComponentInChildren<LanguageSelector>(true);

        if (graphicDropdown == null) graphicDropdown = UIBinder.FindInRow<Dropdown>(root, "Graphic Preset");
        if (displayModeDropdown == null) displayModeDropdown = UIBinder.FindInRow<Dropdown>(root, "Display Mode");
        if (resolutionDropdown == null) resolutionDropdown = UIBinder.FindInRow<Dropdown>(root, "Resolution");

        if (masterSlider == null) masterSlider = UIBinder.FindInRow<Slider>(root, "Master Volume");
        if (bgmSlider == null) bgmSlider = UIBinder.FindInRow<Slider>(root, "BGM Volume");
        if (sfxSlider == null) sfxSlider = UIBinder.FindInRow<Slider>(root, "SFX Volume");

        if (sensitivitySlider == null) sensitivitySlider = UIBinder.FindInRow<Slider>(root, "Camera Sensitivity");
        if (accelSlider == null) accelSlider = UIBinder.FindInRow<Slider>(root, "Camera Acceleration");
        if (shakeSlider == null) shakeSlider = UIBinder.FindInRow<Slider>(root, "Camera Shake");

        if (masterValue == null) masterValue = UIBinder.FindInRow<Text>(root, "Master Volume", "Value Text");
        if (bgmValue == null) bgmValue = UIBinder.FindInRow<Text>(root, "BGM Volume", "Value Text");
        if (sfxValue == null) sfxValue = UIBinder.FindInRow<Text>(root, "SFX Volume", "Value Text");
        if (sensitivityValue == null) sensitivityValue = UIBinder.FindInRow<Text>(root, "Camera Sensitivity", "Value Text");
        if (accelValue == null) accelValue = UIBinder.FindInRow<Text>(root, "Camera Acceleration", "Value Text");
        if (shakeValue == null) shakeValue = UIBinder.FindInRow<Text>(root, "Camera Shake", "Value Text");

        if (applyButton == null) applyButton = UIBinder.Find<Button>(root, "Apply Button");
        if (cancelButton == null) cancelButton = UIBinder.Find<Button>(root, "Cancel Button");
        if (defaultButton == null) defaultButton = UIBinder.Find<Button>(root, "Default Button");

        if (confirmPopup == null) confirmPopup = UIBinder.FindObject(root, "ResolutionConfirmPopup");

        if (confirmPopup != null)
        {
            if (confirmCountdownText == null)
                confirmCountdownText = UIBinder.Find<Text>(confirmPopup.transform, "CountdownText");

            if (confirmKeepButton == null)
                confirmKeepButton = UIBinder.Find<Button>(confirmPopup.transform, "KeepButton");
        }
    }

    private void SetupControls()
    {
        RefreshDropdownLabels();

        SetupSlider(masterSlider, 0f, 1f);
        SetupSlider(bgmSlider, 0f, 1f);
        SetupSlider(sfxSlider, 0f, 1f);

        SetupSlider(sensitivitySlider, GameSetting.MinSensitivity, GameSetting.MaxSensitivity);
        SetupSlider(accelSlider, 0f, 1f);
        SetupSlider(shakeSlider, 0f, 1f);

        if (confirmPopup != null)
            confirmPopup.SetActive(false);
    }

    private void RefreshDropdownLabels()
    {
        SetupDropdown(graphicDropdown, ResolveQualityLabels());
        SetupDropdown(displayModeDropdown, ResolveDisplayModeLabels());
        SetupDropdown(resolutionDropdown, DisplayOptions.ResolutionNames); // 숫자라 번역 불필요
    }

    private IReadOnlyList<string> ResolveQualityLabels()
    {
        int count = GameSetting.QualityLevelCount;
        string[] result = new string[count];

        for (int i = 0; i < count; i++)
        {
            bool hasKey = qualityKeys != null && i < qualityKeys.Length;
            result[i] = hasKey ? LocalizationManager.Text(qualityKeys[i]) : QualitySettings.names[i];
        }

        return result;
    }

    private static IReadOnlyList<string> ResolveDisplayModeLabels()
    {
        int count = DisplayOptions.DisplayModes.Length;
        string[] result = new string[count];

        for (int i = 0; i < count; i++)
            result[i] = LocalizationManager.Text(DisplayOptions.DisplayModeKeys[i]);

        return result;
    }

    private static void SetupDropdown(Dropdown dropdown, IReadOnlyList<string> options)
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

    private static void SetupSlider(Slider slider, float min, float max)
    {
        if (slider == null)
            return;

        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = false;
    }

    private void BindEvents()
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

        if (applyButton != null) applyButton.onClick.AddListener(OnApply);
        if (cancelButton != null) cancelButton.onClick.AddListener(OnCancel);
        if (defaultButton != null) defaultButton.onClick.AddListener(OnDefault);
        if (confirmKeepButton != null) confirmKeepButton.onClick.AddListener(OnConfirmKeep);

        isBound = true;
    }

    private void BindSlider(Slider slider, System.Action<float> setter)
    {
        if (slider == null)
            return;

        slider.onValueChanged.AddListener(v =>
        {
            setter(v);
            MarkDirty();
        });
    }

    private void BindDropdown(Dropdown dropdown, System.Action<int> setter)
    {
        if (dropdown == null)
            return;

        dropdown.onValueChanged.AddListener(v =>
        {
            setter(v);
            MarkDirty();
        });
    }

    private void OnLanguageSelected(GameLanguage language)
    {
        PlayButtonSound();

        draft.language = language;

        if (LocalizationManager.HasInstance)
            LocalizationManager.instance.SetLanguage(language);

        MarkDirty();
    }

    private void OnLanguageChanged(GameLanguage language)
    {
        RefreshDropdownLabels();
        UpdateValueTexts();

        if (languageSelector != null)
            languageSelector.RefreshLabelFonts();
    }

    private void RefreshUI()
    {
        if (draft == null)
            return;

        if (languageSelector != null)
            languageSelector.SetWithoutNotify(draft.language);

        SetDropdown(graphicDropdown, draft.qualityLevel);
        SetDropdown(displayModeDropdown, draft.displayModeIndex);
        SetDropdown(resolutionDropdown, DisplayOptions.ResolveResolutionIndex(draft.resolutionIndex));

        SetSlider(masterSlider, draft.masterVolume);
        SetSlider(bgmSlider, draft.bgmVolume);
        SetSlider(sfxSlider, draft.sfxVolume);

        SetSlider(sensitivitySlider, draft.mouseSensitivity);
        SetSlider(accelSlider, draft.cameraAccel);
        SetSlider(shakeSlider, draft.cameraShake);

        MarkDirty();
    }

    private static void SetDropdown(Dropdown dropdown, int value)
    {
        if (dropdown == null)
            return;

        dropdown.SetValueWithoutNotify(value);
        dropdown.RefreshShownValue();
    }

    private static void SetSlider(Slider slider, float value)
    {
        if (slider != null)
            slider.SetValueWithoutNotify(value);
    }

    private void MarkDirty()
    {
        UpdateValueTexts();

        if (applyButton != null && SettingManager.HasInstance)
            applyButton.interactable = !draft.IsSameAs(SettingManager.instance.Current);
    }

    private void UpdateValueTexts()
    {
        SetPercentText(masterValue, draft.masterVolume);
        SetPercentText(bgmValue, draft.bgmVolume);
        SetPercentText(sfxValue, draft.sfxVolume);
        SetPercentText(accelValue, draft.cameraAccel);
        SetPercentText(shakeValue, draft.cameraShake);

        if (sensitivityValue != null)
            sensitivityValue.text = draft.mouseSensitivity.ToString("0.0");
    }

    private static void SetPercentText(Text text, float value01)
    {
        if (text != null)
            text.text = Mathf.RoundToInt(value01 * 100f) + "%";
    }

    private void OnApply()
    {
        PlayButtonSound();

        GameSetting current = SettingManager.instance.Current;
        bool displayChanged = draft.IsDisplayChangedFrom(current);
        snapshotBeforeApply = current.Clone();

        SettingManager.instance.Commit(draft);
        draft = SettingManager.instance.Current.Clone();
        RefreshUI();

        if (displayChanged && confirmPopup != null)
            StartRevertCountdown();
        else
            Close();
    }

    private void OnCancel()
    {
        PlayButtonSound();

        GameSetting current = SettingManager.instance.Current;
        if (draft.language != current.language && LocalizationManager.HasInstance)
            LocalizationManager.instance.SetLanguage(current.language);

        draft = current.Clone();
        RefreshUI();
        Close();
    }

    private void OnDefault()
    {
        PlayButtonSound();

        GameSetting defaults = new GameSetting();

        defaults.resolutionIndex = draft.resolutionIndex;
        defaults.displayModeIndex = draft.displayModeIndex;
        defaults.language = draft.language;

        draft = defaults;
        RefreshUI();
    }

    private void StartRevertCountdown()
    {
        confirmPopup.SetActive(true);

        if (revertRoutine != null)
            StopCoroutine(revertRoutine);

        revertRoutine = StartCoroutine(RevertCountdownRoutine());
    }

    private IEnumerator RevertCountdownRoutine()
    {
        float remain = revertCountdown;

        while (remain > 0f)
        {
            if (confirmCountdownText != null)
                confirmCountdownText.text = Mathf.CeilToInt(remain).ToString();

            remain -= Time.unscaledDeltaTime;
            yield return null;
        }

        SettingManager.instance.Commit(snapshotBeforeApply);
        draft = SettingManager.instance.Current.Clone();
        RefreshUI();
        EndCountdown();
    }

    private void OnConfirmKeep()
    {
        PlayButtonSound();
        EndCountdown();
        Close();
    }

    private void EndCountdown()
    {
        if (revertRoutine != null)
        {
            StopCoroutine(revertRoutine);
            revertRoutine = null;
        }

        if (confirmPopup != null)
            confirmPopup.SetActive(false);
    }

    private static void PlayButtonSound()
    {
        if (SoundManager.HasInstance)
            SoundManager.instance.PlayButtonSound();
    }
}