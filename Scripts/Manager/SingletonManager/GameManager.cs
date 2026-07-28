using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MapSpawner))]
[RequireComponent(typeof(EndingDirector))]
public class GameManager : Singleton<GameManager>
{
    [Header("층 설정")]
    [SerializeField] private int startFloor = 10;
    [SerializeField] private int targetFloor = 0;

    [Header("씬 설정")]
    [SerializeField] private string storySceneName = "StoryMode";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("맵 설정")]
    [SerializeField] private Transform mapSpawnPoint;

    [Header("플레이어 설정")]
    [SerializeField] private Vector3 playerSpawnPosition;
    [SerializeField] private Quaternion playerSpawnRotation;

    public bool showFloorNumber { get; set; } = true;
    public int CurrentFloor => progress.CurrentFloor;

    public bool isEnded => endingDirector.IsEnded;

    public GameObject player { get; private set; }

    public static event Action<int> OnFloorFirstVisited;
    public static event Action OnLoopReset;

    private FloorProgress progress;
    private MapSpawner mapSpawner;
    private EndingDirector endingDirector;


    protected override void Awake()
    {
        base.Awake();
        if (Instance == this)
            SceneManager.sceneLoaded += OnSceneLoaded;
        
        progress = new FloorProgress(startFloor, targetFloor);
        mapSpawner = GetComponent<MapSpawner>();
        endingDirector = GetComponent<EndingDirector>();
    }

    private void OnEnable()
    {
        ElevatorController.OnElevatorAnswerSelected += CheckAnswer;
    }

    private void OnDisable()
    {
        ElevatorController.OnElevatorAnswerSelected -= CheckAnswer;
    }

    protected override void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
        
        base.OnDestroy();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) // 씬 이동 시 실행되는 함수
    {
        if (scene.name == mainMenuSceneName)
        {
            HandleMainMenuScene();
            return;
        }

        AssignPlayer();
        AssignSpawnPoint();

        if (scene.name == storySceneName)
            HandleStoryScene();
    }

    private void HandleMainMenuScene() // 메인메뉴 씬 이동을 처리하는 함수
    {
        progress.Reset();
        endingDirector.ResetState();
    }

    private void AssignPlayer() // 맵 내의 플레이어 찾고 할당하는 함수
    {
        GameObject foundPlayer = GameObject.FindWithTag("Player");
        if (foundPlayer != null)
            player = foundPlayer;
    }

    private void AssignSpawnPoint() // 맵 내의 스폰포인트를 찾고 할당하는 함수
    {
        GameObject foundSpawnPoint = GameObject.Find("MapSpawnPoint");
        if (foundSpawnPoint != null)
            mapSpawnPoint = foundSpawnPoint.transform;
    }

    private void HandleStoryScene() // 스토리 씬 이동을 처리하는 함수
    {
        showFloorNumber = true;
        if (player != null)
        {
            playerSpawnPosition = player.transform.position;
            playerSpawnRotation = player.transform.rotation;
        }
        StartLoop();
    }

    public void StartLoop() // 게임의 주요 로직을 실행하는 함수
    {
        if (player != null)
        {
            var movement = player.GetComponent<PlayerMovement>();
            if (movement != null)
                movement.canMove = true;
        }

        if (FadeManager.HasInstance)
            FadeManager.Instance.FadeOut(2.0f);
       
        ClearPreviousFloorState();
        bool isEndingScene = SceneManager.GetActiveScene().name == endingDirector.BadEndingSceneName;
        FloorRule.MapInfo nextMap = FloorRule.ChoiceMap(progress.CurrentFloor, startFloor, targetFloor, isEndingScene);
        mapSpawner.Spawn(nextMap, mapSpawnPoint);
        mapSpawner.UpdateFloorDisplay(progress.CurrentFloor, showFloorNumber);
        ResetPlayerPosition();
        RaiseFloorEvents();
    }

    private void ClearPreviousFloorState() // 이전 층 상태를 초기화하는 함수
    {
        if (player == null)
            return;

        if (player.TryGetComponent(out FootstepController footstep))
            footstep.SetAbnormalStatus(false, false);
    }

    private void ResetPlayerPosition() // 플레이어 위치를 초기화하는 함수
    {
        if (player == null)
            return;

        StartCoroutine(ResetPlayerPositionRoutine());
    }

    private IEnumerator ResetPlayerPositionRoutine() // 플레이어 위치를 초기화하는 코루틴
    {
        ElevatorController.IsTeleporting = true;
        CharacterController cc = player.GetComponent<CharacterController>();
        
        if (cc != null) 
            cc.enabled = false;
        player.transform.SetPositionAndRotation(playerSpawnPosition, playerSpawnRotation);
        if (cc != null) 
            cc.enabled = true;

        yield return new WaitForFixedUpdate();
        yield return null;

        ElevatorController.IsTeleporting = false;

        ElevatorController[] elevators = FindObjectsByType<ElevatorController>();
        foreach (var elevator in elevators)
            elevator.InitializeFirstTriggerState(player.transform.position);
    }

    private void RaiseFloorEvents() // 현재 층 관련 이벤트를 발생시키는 함수
    {
        if (progress.ConsumeReturningFlag())
            OnLoopReset?.Invoke();
        else
        {
            if (progress.TryMarkVisited())
                OnFloorFirstVisited?.Invoke(progress.CurrentFloor);
        }
    }

    public void CheckAnswer(TriggerType choice) // 엘레베이터 선택에 따른 플레이어 정답을 확인하는 함수
    {
        if (progress.IsCleared)
            return;

        progress.Submit(choice, mapSpawner.HasAbnormal);
        StartLoop();
    }
}