using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StoryModeUIManager : BaseUIManager<StoryModeUIManager>
{
    [Header("UI 요소")]
    [SerializeField] private Text elevatorText;
    [SerializeField] private GameObject menuUI;
    [SerializeField] private SettingPanel settingPanel;

    [Header("텍스트 설정")]
    [SerializeField] private TypewriterText monologueTypewriter;
    [SerializeField]
    private string[] monologueKeys =
    {
        "story.monologue.floor10", "story.monologue.floor9", "story.monologue.floor8",
        "story.monologue.floor7",  "story.monologue.floor6", "story.monologue.floor5",
        "story.monologue.floor4",  "story.monologue.floor3", "story.monologue.floor2",
        "story.monologue.floor1",  "story.monologue.floor0",
    };
    [SerializeField]
    private string[] loopResetKeys =
    {
        "story.loopReset.0", "story.loopReset.1", "story.loopReset.2",
    };

    [Header("씬 설정")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private PlayerMovement playerMovement;
    private bool menuActivated;


    protected override void AutoBindUI() // UI 자동화 함수
    {
        if (menuUI == null)
            menuUI = UIBinder.FindObject(transform, "MenuUI");

        if (elevatorText == null)
            elevatorText = UIBinder.Find<Text>(transform, "ElevatorButtonText");

        if (monologueTypewriter == null)
        {
            Transform monologue = UIBinder.FindTransform(transform, "MonologueText");

            if (monologue != null)
            {
                monologueTypewriter = monologue.GetComponent<TypewriterText>();

                if (monologueTypewriter == null)
                    monologueTypewriter = monologue.gameObject.AddComponent<TypewriterText>();
            }
        }

        if (settingPanel == null)
            settingPanel = GetComponentInChildren<SettingPanel>(true);

        UIBinder.BindButtons(transform, new Dictionary<string, UnityAction>
        {
            { "ContinueButton",  OnClickContinue  },
            { "SettingButton",   OnClickSetting   },
            { "GoToTitleButton", OnClickGoToTitle },
            { "ExitButton",      OnClickExit      },
        });

        if (settingPanel != null)
            settingPanel.Closed += OnSettingClosed;
    }

    protected override void InitializeUI() // UI 초기화 함수
    {
        if (menuUI != null)
            menuUI.SetActive(false);

        if (elevatorText != null)
            elevatorText.gameObject.SetActive(false);

        if (monologueTypewriter != null)
            monologueTypewriter.Clear();

        if (settingPanel != null)
            settingPanel.Close();
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

    protected override void OnDestroy()
    {
        if (settingPanel != null)
            settingPanel.Closed -= OnSettingClosed;

        base.OnDestroy();
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        if (settingPanel != null && settingPanel.HandleCancelInput())
            return;

        menuActivated = !menuActivated;
        ToggleMenu(menuActivated);
    }

    private void HandleFloorFirstVisited(int floor) // 최초 층 방문을 처리하는 함수
    {
        int index = 10 - floor;

        if (index < 0 || index >= monologueKeys.Length)
            return;

        ShowMonologue(monologueKeys[index]);
    }

    private void HandleLoopReset() // 오답 선택 후 10층 이동 시 실행되는 함수
    {
        if (loopResetKeys == null || loopResetKeys.Length == 0)
            return;

        ShowMonologue(loopResetKeys[Random.Range(0, loopResetKeys.Length)]);
    }

    private void ShowMonologue(string key) // 독백창을 출력하는 함수
    {
        if (monologueTypewriter == null)
            return;

        monologueTypewriter.Play(Loc.Story(key));
    }

    private void ToggleInteractionText(bool isVisible) // 상호작용 텍스트를 토글하는 함수
    {
        if (elevatorText != null)
            elevatorText.gameObject.SetActive(isVisible);
    }

    private void ToggleMenu(bool isVisible) // 메뉴창을 토글하는 함수
    {
        if (menuUI == null)
            return;

        menuUI.SetActive(isVisible);
        Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isVisible;

        if (playerMovement != null)
            playerMovement.canMove = !isVisible;

        if (SoundManager.HasInstance)
        {
            if (isVisible)
                SoundManager.Instance.PauseGameplay();
            else
                SoundManager.Instance.ResumeGameplay();
        }

        Time.timeScale = isVisible ? 0f : 1f;
    }

    public void OnClickContinue() // 계속하기 버튼 클릭 시 실행되는 함수
    {
        PlayButtonSound();
        menuActivated = false;
        ToggleMenu(menuActivated);
    }

    public void OnClickSetting() // 설정 버튼 클릭 시 실행되는 함수
    {
        PlayButtonSound();

        if (settingPanel == null)
            return;

        if (menuUI != null)
            menuUI.SetActive(false);

        settingPanel.Open();
    }

    private void OnSettingClosed() // 세팅 닫기 버튼 클릭 시 실행되는 함수
    {
        if (menuUI != null)
            menuUI.SetActive(menuActivated);
    }

    public void OnClickGoToTitle() // 메인메뉴 이동 버튼 클릭 시 실행되는 함수
    {
        PlayButtonSound();

        if (SoundManager.HasInstance)
            SoundManager.Instance.StopAllSound();

        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnClickExit() // 나가기 버튼 클릭 시 실행되는 함수
    {
        PlayButtonSound();
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}