using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StoryModeUIManager : BaseUIManager<StoryModeUIManager>
{
    [Header("UI")]
    [SerializeField] private Text elevatorText;
    [SerializeField] private GameObject menuUI;
    [SerializeField] private SettingPanel settingPanel;

    [Header("독백")]
    [SerializeField] private TypewriterText monologueTypewriter;

    [Tooltip("인덱스 0이 시작 층(10층), 이후 한 층씩 내려간다. Story 테이블의 키.")]
    [SerializeField]
    private string[] monologueKeys =
    {
        "story.monologue.floor10", "story.monologue.floor9", "story.monologue.floor8",
        "story.monologue.floor7",  "story.monologue.floor6", "story.monologue.floor5",
        "story.monologue.floor4",  "story.monologue.floor3", "story.monologue.floor2",
        "story.monologue.floor1",  "story.monologue.floor0",
    };

    [Tooltip("오답으로 시작 층에 되돌아왔을 때 무작위로 하나를 출력한다.")]
    [SerializeField]
    private string[] loopResetKeys =
    {
        "story.loopReset.0", "story.loopReset.1", "story.loopReset.2",
    };

    [Header("씬 전환")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private PlayerMovement playerMovement;
    private bool menuActivated;

    protected override void AutoBindUI()
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

    protected override void InitializeUI()
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

        // 설정 패널이 열려 있으면 그쪽이 입력을 소비한다.
        if (settingPanel != null && settingPanel.HandleCancelInput())
            return;

        menuActivated = !menuActivated;
        ToggleMenu(menuActivated);
    }

    // ── 독백 ──────────────────────────────────────────────

    private void HandleFloorFirstVisited(int floor)
    {
        int index = 10 - floor;

        if (index < 0 || index >= monologueKeys.Length)
            return;

        ShowMonologue(monologueKeys[index]);
    }

    private void HandleLoopReset()
    {
        if (loopResetKeys == null || loopResetKeys.Length == 0)
            return;

        ShowMonologue(loopResetKeys[Random.Range(0, loopResetKeys.Length)]);
    }

    private void ShowMonologue(string key)
    {
        if (monologueTypewriter == null)
            return;

        monologueTypewriter.Play(Loc.Story(key));
    }

    // ── 메뉴 ──────────────────────────────────────────────

    private void ToggleInteractionText(bool isVisible)
    {
        if (elevatorText != null)
            elevatorText.gameObject.SetActive(isVisible);
    }

    private void ToggleMenu(bool isVisible)
    {
        if (menuUI == null)
            return;

        menuUI.SetActive(isVisible);

        Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isVisible;

        if (playerMovement != null)
            playerMovement.canMove = !isVisible;

        AudioListener.pause = isVisible;
        Time.timeScale = isVisible ? 0f : 1f;
    }

    public void OnClickContinue()
    {
        PlayButtonSound();
        menuActivated = false;
        ToggleMenu(menuActivated);
    }

    public void OnClickSetting()
    {
        PlayButtonSound();

        if (settingPanel == null)
            return;

        // 볼륨 조절을 귀로 확인할 수 있어야 하므로 일시적으로 오디오를 되살린다.
        AudioListener.pause = false;
        settingPanel.Open();
    }

    private void OnSettingClosed()
    {
        AudioListener.pause = menuActivated;

        if (menuUI != null)
            menuUI.SetActive(menuActivated);
    }

    public void OnClickGoToTitle()
    {
        PlayButtonSound();

        if (SoundManager.HasInstance)
            SoundManager.instance.StopAllSound();

        // 씬을 넘기기 전에 반드시 되돌려야 다음 씬이 멈춘 채로 시작하지 않는다.
        Time.timeScale = 1f;
        AudioListener.pause = false;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnClickExit()
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