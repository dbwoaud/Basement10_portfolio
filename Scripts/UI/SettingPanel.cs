using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

/// <summary>
/// [Unity Localization 전환에 따른 변경]
/// 1. 드롭다운 항목 텍스트를 Loc.UI()로 조회
/// 2. 언어 값이 로케일 코드 문자열
/// 3. LocalizationSettings.SelectedLocaleChanged를 구독해 언어가 바뀌면 항목을 다시 채움
///
/// 씬 고정 라벨(항목 이름, 버튼 글자)은 이 스크립트가 건드리지 않는다.
/// 패키지의 LocalizeStringEvent 컴포넌트가 담당한다.
/// 이 스크립트는 코드가 동적으로 채우는 문자열(드롭다운 항목, 수치 표시)만 다룬다.
///
/// UI 행은 런타임 생성하지 않고 인스펙터에 미리 배치한다.
/// 항목이 10개 수준이고 출시 시점에 고정되므로, 동적 생성 구조를 넣으면
/// 인스펙터에서 UI 구성을 추적할 수 없게 되어 얻는 것보다 잃는 것이 많다.
/// </summary>
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

    [Header("번역 키")]
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

    /// <summary>패널이 닫힐 때 알린다. 상위 메뉴가 커서·일시정지 상태를 되돌릴 때 쓴다.</summary>
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
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    // ── 열기 / 닫기 ────────────────────────────────────────

    public void Open()
    {
        if (!SettingManager.HasInstance)
        {
            Debug.LogError("[설정] SettingManager가 없습니다.");
            return;
        }

        gameObject.SetActive(true);
        draft = SettingManager.Instance.Current.Clone();
        RefreshDropdownLabels();
        RefreshUI();
    }

    public void Close()
    {
        EndCountdown();
        gameObject.SetActive(false);
        Closed?.Invoke();
    }

    /// <summary>
    /// ESC 입력을 외부(메뉴 매니저)에서 전달받는다.
    /// 패널이 자체 Update에서 Input을 읽으면 상위 메뉴의 ESC와 같은 프레임에 처리되어
    /// 메뉴까지 닫히는 문제가 있었다.
    /// </summary>
    /// <returns>입력을 소비했으면 true.</returns>
    public bool HandleCancelInput()
    {
        if (!IsOpen)
            return false;

        // 되돌리기 대기 중에는 ESC로 닫지 않는다.
        if (confirmPopup != null && confirmPopup.activeSelf)
            return true;

        OnCancel();
        return true;
    }

    // ── 초기화 ────────────────────────────────────────────

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
        SetupSlider(masterSlider, 0f, 1f);
        SetupSlider(bgmSlider, 0f, 1f);
        SetupSlider(sfxSlider, 0f, 1f);

        SetupSlider(sensitivitySlider, GameSetting.MinSensitivity, GameSetting.MaxSensitivity);
        SetupSlider(accelSlider, 0f, 1f);
        SetupSlider(shakeSlider, 0f, 1f);

        if (confirmPopup != null)
            confirmPopup.SetActive(false);
    }

    /// <summary>드롭다운 항목 텍스트를 현재 언어로 다시 채운다.</summary>
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
            result[i] = hasKey ? Loc.UI(qualityKeys[i]) : QualitySettings.names[i];
        }

        return result;
    }

    private static IReadOnlyList<string> ResolveDisplayModeLabels()
    {
        int count = DisplayOptions.DisplayModes.Length;
        string[] result = new string[count];

        for (int i = 0; i < count; i++)
            result[i] = Loc.UI(DisplayOptions.DisplayModeKeys[i]);

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

    // ── 언어 ──────────────────────────────────────────────

    /// <summary>
    /// 언어만은 확인 버튼을 기다리지 않고 즉시 적용한다.
    /// 읽지 못하는 언어로 잘못 바꿨을 때 확인·취소 버튼의 글자마저 읽을 수 없으면
    /// 되돌리기가 어려워지기 때문이다. 즉시 반영하면 화면을 보고 판단할 수 있다.
    /// </summary>
    private void OnLanguageSelected(string localeCode)
    {
        PlayButtonSound();

        draft.languageCode = localeCode;
        SelectLocaleImmediately(localeCode);
        MarkDirty();
    }

    private static void SelectLocaleImmediately(string localeCode)
    {
        if (!LocalizationSettings.HasSettings)
            return;

        Locale target = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier(localeCode));

        if (target != null)
            LocalizationSettings.SelectedLocale = target;
    }

    /// <summary>패키지가 로케일을 바꾸면 호출된다. 코드가 채우는 문자열만 갱신한다.</summary>
    private void OnLocaleChanged(Locale locale)
    {
        RefreshDropdownLabels();
        RefreshDropdownValues();
        UpdateValueTexts();
    }

    // ── 갱신 ──────────────────────────────────────────────

    private void RefreshUI()
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

    private void RefreshDropdownValues()
    {
        if (draft == null)
            return;

        SetDropdown(graphicDropdown, draft.qualityLevel);
        SetDropdown(displayModeDropdown, draft.displayModeIndex);
        SetDropdown(resolutionDropdown, DisplayOptions.ResolveResolutionIndex(draft.resolutionIndex));
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
            applyButton.interactable = !draft.IsSameAs(SettingManager.Instance.Current);
    }

    private void UpdateValueTexts()
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

    private static void SetPercentText(Text text, float value01)
    {
        if (text != null)
            text.text = Mathf.RoundToInt(value01 * 100f) + "%";
    }

    // ── 버튼 처리 ──────────────────────────────────────────

    private void OnApply()
    {
        PlayButtonSound();

        GameSetting current = SettingManager.Instance.Current;
        bool displayChanged = draft.IsDisplayChangedFrom(current);
        snapshotBeforeApply = current.Clone();

        SettingManager.Instance.Commit(draft);
        draft = SettingManager.Instance.Current.Clone();
        RefreshUI();

        if (displayChanged && confirmPopup != null)
            StartRevertCountdown();
        else
            Close();
    }

    private void OnCancel()
    {
        PlayButtonSound();

        GameSetting current = SettingManager.Instance.Current;

        // 언어는 즉시 적용했으므로 취소 시 되돌려 준다.
        if (draft.languageCode != current.languageCode)
            SelectLocaleImmediately(current.languageCode);

        draft = current.Clone();
        RefreshUI();
        Close();
    }

    private void OnDefault()
    {
        PlayButtonSound();

        GameSetting defaults = new GameSetting();

        // 기본값 복원이 화면 해상도와 언어까지 바꾸면 사용자가 놀라므로 그대로 둔다.
        defaults.resolutionIndex = draft.resolutionIndex;
        defaults.displayModeIndex = draft.displayModeIndex;
        defaults.languageCode = draft.languageCode;

        draft = defaults;
        RefreshUI();
    }

    // ── 해상도 되돌리기 ────────────────────────────────────

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

            // 일시정지(timeScale = 0) 중에도 카운트다운이 흘러야 한다.
            remain -= Time.unscaledDeltaTime;
            yield return null;
        }

        SettingManager.Instance.Commit(snapshotBeforeApply);
        draft = SettingManager.Instance.Current.Clone();
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
            SoundManager.Instance.PlayButtonSound();
    }
}