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

    [Header("����")]
    [SerializeField] private TypewriterText monologueTypewriter;

    [Tooltip("�ε��� 0�� ���� ��(10��), ���� �� ���� ��������. Story ���̺��� Ű.")]
    [SerializeField]
    private string[] monologueKeys =
    {
        "story.monologue.floor10", "story.monologue.floor9", "story.monologue.floor8",
        "story.monologue.floor7",  "story.monologue.floor6", "story.monologue.floor5",
        "story.monologue.floor4",  "story.monologue.floor3", "story.monologue.floor2",
        "story.monologue.floor1",  "story.monologue.floor0",
    };

    [Tooltip("�������� ���� ���� �ǵ��ƿ��� �� �������� �ϳ��� ����Ѵ�.")]
    [SerializeField]
    private string[] loopResetKeys =
    {
        "story.loopReset.0", "story.loopReset.1", "story.loopReset.2",
    };

    [Header("�� ��ȯ")]
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

        // ���� �г��� ���� ������ ������ �Է��� �Һ��Ѵ�.
        if (settingPanel != null && settingPanel.HandleCancelInput())
            return;

        menuActivated = !menuActivated;
        ToggleMenu(menuActivated);
    }

    // ���� ���� ��������������������������������������������������������������������������������������������

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

    // ���� �޴� ��������������������������������������������������������������������������������������������

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

        if (SoundManager.HasInstance)
        {
            if (isVisible)
                SoundManager.Instance.PauseGameplay();
            else
                SoundManager.Instance.ResumeGameplay();
        }

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

        if (menuUI != null)
            menuUI.SetActive(false);

        settingPanel.Open();
    }

    private void OnSettingClosed()
    {
        if (menuUI != null)
            menuUI.SetActive(menuActivated);
    }

    public void OnClickGoToTitle()
    {
        PlayButtonSound();

        if (SoundManager.HasInstance)
            SoundManager.Instance.StopAllSound();

        // ���� �ѱ�� ���� �ݵ�� �ǵ����� ���� ���� ���� ä�� �������� �ʴ´�.
        Time.timeScale = 1f;
        

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