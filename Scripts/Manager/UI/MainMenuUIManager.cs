using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainMenuUIManager : BaseUIManager<MainMenuUIManager>
{
    [Header("UI")]
    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private SettingPanel settingPanel;

    [Header("Manager")]
    [SerializeField] private MainMenuManager mainMenuManager;

    private GraphicRaycaster raycaster;
    private bool isProcessing = false;

    private void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (SoundManager.HasInstance)
            SoundManager.Instance.StopBGM();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (descriptionPanel != null) 
            descriptionPanel.SetActive(false);

        if (settingPanel != null)
            settingPanel.Close();
    }

    protected override void AutoBindUI()
    {
        if (raycaster == null)    
            raycaster = GetComponent<GraphicRaycaster>();
       
        if (mainMenuManager == null)
            mainMenuManager = FindAnyObjectByType<MainMenuManager>();

        if (descriptionPanel == null)
            descriptionPanel = UIBinder.FindObject(transform, "DescriptionPanel");

        if (settingPanel == null)
            settingPanel = UIBinder.Find<SettingPanel>(transform, "SettingPanel");

        if (settingPanel == null)
            settingPanel = GetComponentInChildren<SettingPanel>(true);

        UIBinder.BindButtons(transform, new Dictionary<string, UnityAction>
        {
            { "StartButton", OnClickStart },
            { "DescriptionButton", OnClickDescription },
            { "SettingButton", OnClickSetting },
            { "CloseDescriptionButton", OnClickCloseDescription },
            { "ExitButton", OnClickExit }
        });
    }

    public void SetUIInteractable(bool state)
    {
        isProcessing = !state;
        if (raycaster != null) 
            raycaster.enabled = state;
    }

    public void OnClickStart()
    {
        if (isProcessing)
            return;

        PlayButtonSound();

        if (mainMenuManager != null)
        {
            SetUIInteractable(false);
            mainMenuManager.StartGameSequence();
        }
    }

    public void OnClickDescription()
    {
        if (isProcessing)
            return;

        PlayButtonSound();

        if (descriptionPanel != null) 
            descriptionPanel.SetActive(true);
    }

    public void OnClickSetting()
    {
        if (isProcessing)
            return;

        PlayButtonSound();

        if (settingPanel != null)
            settingPanel.Open();
    }

    public void OnClickCloseDescription()
    {
        if (isProcessing)
            return;

        PlayButtonSound();

        if (descriptionPanel != null) 
            descriptionPanel.SetActive(false);
    }

    public void OnClickExit()
    {
        if (isProcessing)
            return;

        PlayButtonSound();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}