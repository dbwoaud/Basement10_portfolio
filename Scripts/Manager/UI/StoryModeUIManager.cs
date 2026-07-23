using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StoryModeUIManager : MonoBehaviour
{
    public static StoryModeUIManager instance;

    [Header("UI 설정")]
    [SerializeField] private Text elevatorText;
    [SerializeField] private GameObject menuUI;
    [SerializeField] private bool menuActivated;

    [Header("독백 시스템")]
    [SerializeField] private Text monologueText;
    [SerializeField, TextArea(3, 10)] private List<string> monologueList;
    [SerializeField, TextArea(3, 10)] private List<string> loopResetList;
    private Coroutine currentMonologueCoroutine;

    [Header("플레이어 설정")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("씬 전환 설정")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";


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
        InitializeUI();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerMovement = FindAnyObjectByType<PlayerMovement>();
    }

    private void OnEnable()
    {
        ElevatorButton.OnPlayerNearButton += ToggleInteractionText;
        GameManager.OnFloorFirstVisited += HandleFloorFirstVisited;
        GameManager.OnLoopReset += HandleLoopReset;
    }

    private void OnDisable()
    {
        ElevatorButton.OnPlayerNearButton -= ToggleInteractionText;
        GameManager.OnFloorFirstVisited -= HandleFloorFirstVisited;
        GameManager.OnLoopReset -= HandleLoopReset;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            menuActivated = !menuActivated;
            ToggleMenu(menuActivated);
        }
    }

    private void AutoBindUI() // UI 자동화 함수
    {
        if (menuUI == null)
            menuUI = transform.Find("MenuUI")?.gameObject;

        if (elevatorText == null)
            elevatorText = transform.Find("ElevatorButtonText")?.GetComponent<Text>();

        if (monologueText == null)
            monologueText = transform.Find("MonologueText")?.GetComponent<Text>();

        if (menuUI != null)
        {
            Button[] buttons = menuUI.GetComponentsInChildren<Button>(true);
            foreach (Button button in buttons)
            {
                switch (button.gameObject.name)
                {
                    case "ContinueButton": button.onClick.AddListener(OnClickContinue); break;
                    case "GoToTitleButton": button.onClick.AddListener(OnClickGoToTitle); break;
                    case "ExitButton": button.onClick.AddListener(OnClickExit); break;
                }
            }
        }
    }

    private void InitializeUI() // UI 초기화 함수
    {
        if (menuUI != null)
            menuUI.SetActive(false);
        if (elevatorText != null)
            elevatorText.gameObject.SetActive(false);
        if (monologueText != null)
            monologueText.gameObject.SetActive(false);
    }

    private void HandleFloorFirstVisited(int floor) // 최초 층을 처리하는 함수
    {
        int index = 10 - floor;
        if (index >= 0 && index < monologueList.Count)
        {
            if(SoundManager.instance != null)
                ShowMonologue(monologueList[index]);
        }
    }

    private void HandleLoopReset() // 10층 회귀 시 독백을 출력하는 함수
    {
        if (loopResetList.Count > 0)
        {
            int randomIndex = Random.Range(0, loopResetList.Count);
            ShowMonologue(loopResetList[randomIndex]);
        }
    }

    private void ShowMonologue(string content) // 독백을 출력하는 함수
    {
        if (currentMonologueCoroutine != null) 
            StopCoroutine(currentMonologueCoroutine);

        currentMonologueCoroutine = StartCoroutine(TypeMonologue(content));
    }

    private IEnumerator TypeMonologue(string content) // 독백을 출력하는 코루틴
    {
        monologueText.text = "";
        monologueText.gameObject.SetActive(true);
        foreach (char letter in content.ToCharArray())
        {
            monologueText.text += letter;
            yield return new WaitForSecondsRealtime(letter == '\n' ? 0.2f : 0.05f);
        }
        yield return new WaitForSecondsRealtime(2.0f);
        monologueText.gameObject.SetActive(false);
        currentMonologueCoroutine = null;
    }

    private void ToggleInteractionText(bool isVisible) // 엘리베이터 텍스트를 토글하는 함수
    { 
        if (elevatorText != null) 
            elevatorText.gameObject.SetActive(isVisible); 
    }

    private void ToggleMenu(bool isVisible) // 메뉴UI를 토글하는 함수
    {
        if (menuUI == null)
            return;

        menuUI.SetActive(isVisible);
        Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isVisible;

        if(playerMovement != null)
            playerMovement.canMove = !isVisible;

        AudioListener.pause = isVisible;
        Time.timeScale = isVisible ? 0f : 1f;    
    }

    public void OnClickContinue() // 계속하기 버튼 클릭 시 실행되는 함수
    {
        PlayButtonSound();
        menuActivated = false;
        ToggleMenu(menuActivated);
    }

    public void OnClickGoToTitle() // 타이틀 버튼 클릭 시 실행되는 함수
    {
        if (SoundManager.instance != null)
            SoundManager.instance.StopAllSound();

        PlayButtonSound();
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnClickExit() // 나가기 버튼 클릭 시 실행되는 함수
    {
        PlayButtonSound();
        Application.Quit(); 
    }

    private void PlayButtonSound() // 버튼 소리를 재생하는 함수
    {
        if (SoundManager.instance != null)
            SoundManager.instance.PlayButtonSound();
    }
}