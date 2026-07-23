using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : MonoBehaviour
{
    [Header("설정 컨트롤")]
    [SerializeField] private Dropdown graphicDropdown;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider accelSlider;
    [SerializeField] private Slider shakeSlider;

    [Header("수치 표시")]
    [SerializeField] private Text sensitivityValue;
    [SerializeField] private Text accelValue;
    [SerializeField] private Text shakeValue;

    [Header("버튼")]
    [SerializeField] private Button applyButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button defaultButton;

    [Header("품질 단계 표기")]
    [SerializeField]
    private string[] qualityDisplayNames =
    {
        "매우 낮음", "낮음", "보통", "높음", "매우 높음", "최상"
    };

    private GameSetting draft;
    private bool isBound;

    private void Awake()
    {
        draft = new GameSetting();
        AutoBindUI();
        SetupControls();
        BindEvents();
    }

    public void Open()
    {
        if (SettingManager.instance == null)
        {
            Debug.LogError("[설정] SettingManager가 없습니다.");
            return;
        }

        gameObject.SetActive(true);
        draft = SettingManager.instance.Current.Clone();
        RefreshUI();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            OnCancel();
    }

    private void SetupControls()
    {
        SetupQualityDropdown();

        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = GameSetting.MinSensitivity;
            sensitivitySlider.maxValue = GameSetting.MaxSensitivity;
            sensitivitySlider.wholeNumbers = false;
        }

        if (accelSlider != null)
        {
            accelSlider.minValue = 0f;
            accelSlider.maxValue = 1f;
        }

        if (shakeSlider != null)
        {
            shakeSlider.minValue = 0f;
            shakeSlider.maxValue = 1f;
        }
    }

    private void SetupQualityDropdown()
    {
        if(graphicDropdown == null)
            return;

        string[] engineNames = QualitySettings.names;
        int count = (engineNames != null) ? engineNames.Length : 0;

        if (count == 0)
        {
            Debug.LogWarning("[설정] Quality 단계가 없습니다.");
            return;
        }

        bool useKorean = qualityDisplayNames != null && qualityDisplayNames.Length == count;

        if (!useKorean)
            Debug.LogWarning($"[설정] 한글 이름 개수와 실제 단계 {count}개가 달라 영문 이름을 사용합니다.");

        List<string> options = new List<string>(count);
        for (int i = 0; i < count; i++)
            options.Add(useKorean ? qualityDisplayNames[i] : engineNames[i]);

        graphicDropdown.ClearOptions();
        graphicDropdown.AddOptions(options);
    }

    private void BindEvents()
    {
        if (isBound) 
            return;

        if (graphicDropdown != null)
            graphicDropdown.onValueChanged.AddListener(v => { draft.qualityLevel = v; MarkDirty(); });

        if (shakeSlider != null)
            shakeSlider.onValueChanged.AddListener(v => { draft.cameraShake = v; MarkDirty(); });

        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.AddListener(v => { draft.mouseSensitivity = v; MarkDirty(); });

        if (accelSlider != null)
            accelSlider.onValueChanged.AddListener(v => { draft.cameraAccel = v; MarkDirty(); });

        if (applyButton != null) 
            applyButton.onClick.AddListener(OnApply);
        if (cancelButton != null) 
            cancelButton.onClick.AddListener(OnCancel);
        if (defaultButton != null) 
            defaultButton.onClick.AddListener(OnDefault);

        isBound = true;
    }

    private void RefreshUI()
    {
        if (draft == null) 
            return;

        if (graphicDropdown != null)
        {
            graphicDropdown.SetValueWithoutNotify(draft.qualityLevel);
            graphicDropdown.RefreshShownValue();
        }

        if (sensitivitySlider != null) 
            sensitivitySlider.SetValueWithoutNotify(draft.mouseSensitivity);
        if (accelSlider != null) 
            accelSlider.SetValueWithoutNotify(draft.cameraAccel);
        if (shakeSlider != null) 
            shakeSlider.SetValueWithoutNotify(draft.cameraShake);

        MarkDirty();
    }

    private void MarkDirty()
    {
        UpdateValueTexts();
        if (applyButton != null && SettingManager.instance != null)
            applyButton.interactable = !draft.IsSameAs(SettingManager.instance.Current);
    }

    private void UpdateValueTexts()
    {
        if (sensitivityValue != null)
            sensitivityValue.text = draft.mouseSensitivity.ToString("0.0");

        if (accelValue != null)
            accelValue.text = Mathf.RoundToInt(draft.cameraAccel * 100f) + "%";

        if (shakeValue != null)
            shakeValue.text = Mathf.RoundToInt(draft.cameraShake * 100f) + "%";
    }

    private void OnApply()
    {
        PlayButtonSound();
        SettingManager.instance.Commit(draft);
        Close();
    }

    private void OnCancel()
    {
        PlayButtonSound();
        Close();
    }

    private void OnDefault()
    {
        PlayButtonSound();
        draft = new GameSetting();
        RefreshUI();
    }

    private void PlayButtonSound()
    {
        if (SoundManager.instance != null)
            SoundManager.instance.PlayButtonSound();
    }

    private void AutoBindUI()
    {
        if (graphicDropdown == null) 
            graphicDropdown = FindInRow<Dropdown>("Graphic Preset");
        if (sensitivitySlider == null) 
            sensitivitySlider = FindInRow<Slider>("Camera Sensitivity");
        if (accelSlider == null) 
            accelSlider = FindInRow<Slider>("Camera Acceleration");
        if (shakeSlider == null) 
            shakeSlider = FindInRow<Slider>("Camera Shake");

        if (sensitivityValue == null) 
            sensitivityValue = FindInRow<Text>("Camera Sensitivity", "Value Text");
        if (accelValue == null) 
            accelValue = FindInRow<Text>("Camera Acceleration", "Value Text");
        if (shakeValue == null) 
            shakeValue = FindInRow<Text>("Camera Shake", "Value Text");

        if (applyButton == null) 
            applyButton = FindByName<Button>("Apply Button");
        if (cancelButton == null) 
            cancelButton = FindByName<Button>("Cancel Button");
        if (defaultButton == null) 
            defaultButton = FindByName<Button>("Default Button");
    }

    private T FindInRow<T>(string rowName, string childName = null) where T : Component
    {
        Transform row = FindTransform(rowName);
        if (row == null)
        {
            Debug.LogWarning($"[설정] '{rowName}' 오브젝트를 찾지 못했습니다.");
            return null;
        }

        if (string.IsNullOrEmpty(childName))
            return row.GetComponentInChildren<T>(true);

        foreach (Transform child in row.GetComponentsInChildren<Transform>(true))
            if (child.name == childName) return child.GetComponent<T>();

        return null;
    }

    private T FindByName<T>(string objectName) where T : Component
    {
        Transform target = FindTransform(objectName);
        return target != null ? target.GetComponent<T>() : null;
    }

    private Transform FindTransform(string objectName)
    {
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
            if (t.name == objectName) return t;

        return null;
    }
}