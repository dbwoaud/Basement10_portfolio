using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
    public static MainMenuUIManager instance;

    [Header("UI 연결")]
    [SerializeField] private GameObject descriptionPanel;

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
    }

    private void AutoBindUI() // UI 자동화 함수
    {
        if (raycaster == null)    
            raycaster = GetComponent<GraphicRaycaster>();
       
        if (mainMenuManager == null)
            mainMenuManager = FindFirstObjectByType<MainMenuManager>();

        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            if (child.name == "DescriptionPanel")
            {
                descriptionPanel = child.gameObject;
                break;
            }
        }

        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            switch (button.gameObject.name)
            {
                case "StartButton": button.onClick.AddListener(OnClickStart); break;
                case "DescriptionButton": button.onClick.AddListener(OnClickDescription); break;
                case "XButton": button.onClick.AddListener(OnClickCloseDescription); break;
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

        if (SoundManager.instance != null)
            SoundManager.instance.PlayButtonSound();

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
            
        if (SoundManager.instance != null)
            SoundManager.instance.PlayButtonSound();

        if (descriptionPanel != null) 
            descriptionPanel.SetActive(true);
    }

    public void OnClickCloseDescription() // 설명 패널의 닫기 버튼 클릭 시 실행되는 함수
    {
        if (isProcessing)
            return;
        
        if (SoundManager.instance != null)
            SoundManager.instance.PlayButtonSound();

        if (descriptionPanel != null) 
            descriptionPanel.SetActive(false);
    }

    public void OnClickExit() // 나가기 버튼 클릭 시 실행되는 함수
    {
        if (isProcessing)
            return;

        if (SoundManager.instance != null)
            SoundManager.instance.PlayButtonSound();

        Application.Quit();
    }
}