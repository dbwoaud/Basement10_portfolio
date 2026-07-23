using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
    public static MainMenuUIManager instance;

    [Header("UI 연결")]
    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private SettingPanel settingPanel;

    [Header("로직 매니저 연결")]
    [SerializeField] private MainMenuManager mainMenuManager;

    private GraphicRaycaster raycaster;
    private bool isProcessing = false;

    private void Awake()
    {
        if (instance == null) 
            instance = this;
        else 
        { 
            Destroy(gameObject); 
            return; 
        }
 
        AutoBindUI();
    }

    private void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (descriptionPanel != null) 
            descriptionPanel.SetActive(false);

        if (settingPanel != null)
            settingPanel.Close();
    }

    private void AutoBindUI() // UI 자동화 함수
    {
        if (raycaster == null)    
            raycaster = GetComponent<GraphicRaycaster>();
       
        if (mainMenuManager == null)
            mainMenuManager = FindAnyObjectByType<MainMenuManager>();

        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            if (descriptionPanel == null && child.name == "DescriptionPanel")
                descriptionPanel = child.gameObject;
            else if (settingPanel == null && child.name == "SettingPanel")
                settingPanel = child.GetComponent<SettingPanel>();
        }

        if (settingPanel == null)
            settingPanel = GetComponentInChildren<SettingPanel>(true);

        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            switch (button.gameObject.name)
            {
                case "StartButton": button.onClick.AddListener(OnClickStart); break;
                case "DescriptionButton": button.onClick.AddListener(OnClickDescription); break;
                case "SettingButton": button.onClick.AddListener(OnClickSetting); break;
                case "CloseDescriptionButton": button.onClick.AddListener(OnClickCloseDescription); break;
                case "ExitButton": button.onClick.AddListener(OnClickExit); break;
            }
        }
    }

    public void SetUIInteractable(bool state)
    {
        isProcessing = !state;
        if (raycaster != null) 
            raycaster.enabled = state;
    }

    public void OnClickStart() // 시작 버튼 클릭 시 실행되는 함수
    {
        if (isProcessing)
            return;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayButtonSound();

        if (mainMenuManager != null)
        {
            SetUIInteractable(false);
            mainMenuManager.StartGameSequence();
        }
    }

    public void OnClickDescription() // 설명 버튼 클릭 시 실행되는 함수
    {
        if (isProcessing)
            return;
            
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayButtonSound();

        if (descriptionPanel != null) 
            descriptionPanel.SetActive(true);
    }

    public void OnClickSetting() // 설정 버튼 클릭 시 실행되는 함수
    {
        if (isProcessing)
            return;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayButtonSound();

        if (settingPanel != null)
            settingPanel.Open();
    }

    public void OnClickCloseDescription() // 설명 패널의 닫기 버튼 클릭 시 실행되는 함수
    {
        if (isProcessing)
            return;
        
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayButtonSound();

        if (descriptionPanel != null) 
            descriptionPanel.SetActive(false);
    }

    public void OnClickExit() // 나가기 버튼 클릭 시 실행되는 함수
    {
        if (isProcessing)
            return;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayButtonSound();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}