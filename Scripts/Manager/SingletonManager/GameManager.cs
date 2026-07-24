using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MapSpawner))]
[RequireComponent(typeof(EndingDirector))]
public class GameManager : Singleton<GameManager>
{
    [Header("Floor Settings")]
    [SerializeField] private int startFloor = 10;
    [SerializeField] private int targetFloor = 0;

    [Header("Scene Settings")]
    [SerializeField] private string storySceneName = "StoryMode";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Map Settings")]
    [SerializeField] private Transform mapSpawnPoint;

    [Header("Player Settings")]
    [SerializeField] private Vector3 playerSpawnPosition;
    [SerializeField] private Quaternion playerSpawnRotation;

    public GameObject player { get; private set; }
    public bool showFloorNumber { get; set; } = true;

    public bool isEnded => endingDirector.IsEnded;
    public int CurrentFloor => progress.CurrentFloor;

    public static event Action<int> OnFloorFirstVisited;
    public static event Action OnLoopReset;

    private FloorProgress progress;
    private MapSpawner mapSpawner;
    private EndingDirector endingDirector;

    protected override void Awake()
    {
        base.Awake();
        if (Instance == this)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

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
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        base.OnDestroy();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == mainMenuSceneName)
        {
            progress.Reset();
            endingDirector.ResetState();
            return;
        }

        GameObject foundPlayer = GameObject.FindWithTag("Player");
        if (foundPlayer != null)
        {
            player = foundPlayer;
        }

        GameObject foundSpawnPoint = GameObject.Find("MapSpawnPoint");
        if (foundSpawnPoint != null)
        {
            mapSpawnPoint = foundSpawnPoint.transform;
        }

        if (scene.name == storySceneName)
        {
            showFloorNumber = true;
            if (player != null)
            {
                playerSpawnPosition = player.transform.position;
                playerSpawnRotation = player.transform.rotation;
            }
            StartLoop();
        }
    }

    public void StartLoop()
    {
        if (player != null)
        {
            var movement = player.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.canMove = true;
            }
        }

        if (FadeManager.HasInstance)
        {
            FadeManager.Instance.FadeOut(2.0f);
        }

        // 1. ClearPreviousFloorState() - 반드시 맵 생성 '앞'에서 실행되어야 함
        ClearPreviousFloorState();

        // 2. plan 추출 및 mapSpawner.Spawn() 호출
        bool isEndingScene = SceneManager.GetActiveScene().name == endingDirector.BadEndingSceneName;
        FloorRule.MapPlan plan = FloorRule.ResolveMapPlan(progress.CurrentFloor, startFloor, targetFloor, isEndingScene);
        mapSpawner.Spawn(plan, mapSpawnPoint);

        // 3. 디스플레이 갱신
        mapSpawner.UpdateFloorDisplay(progress.CurrentFloor, showFloorNumber);

        // 4. 플레이어 위치 초기화
        ResetPlayerPosition();

        // 5. 이벤트 전파
        RaiseFloorEvents();
    }

    private void ClearPreviousFloorState()
    {
        /*
         * [게임플레이 버그 재발 방지용 핵심 주석]
         * ClearPreviousFloorState()는 플레이어의 FootstepController 비정상 상태(예: 이중 발자국 효과음 등)를 초기화합니다.
         * 이 작업은 반드시 mapSpawner.Spawn()보다 '앞'에서 실행되어야 합니다.
         * 만약 Spawn()이 먼저 실행되어 새 맵의 이상현상(Double Sound 등)이 플레이어의 FootstepController에 적용된 후
         * ClearPreviousFloorState()가 실행되면, 방금 새로 적용된 이상현상 상태가 즉시 지워져서 이상현상이 정상적으로 작동하지 않게 되는 심각한 버그가 발생합니다.
         * NPC는 매 층마다 맵 프리팹과 함께 새로 파괴 및 생성되므로 초기화 대상이 아니며, 플레이어에 대해서만 초기화 작업을 수행합니다.
         */
        if (player == null)
            return;

        if (player.TryGetComponent(out FootstepController footstep))
        {
            footstep.SetAbnormalStatus(false, false);
        }
    }

    private void ResetPlayerPosition()
    {
        if (player == null)
            return;

        StartCoroutine(ResetPlayerPositionRoutine());
    }

    private IEnumerator ResetPlayerPositionRoutine()
    {
        ElevatorController.IsTeleporting = true;

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.SetPositionAndRotation(playerSpawnPosition, playerSpawnRotation);
        if (cc != null) cc.enabled = true;

        yield return new WaitForFixedUpdate();
        yield return null;

        ElevatorController.IsTeleporting = false;

        ElevatorController[] elevators =
            FindObjectsByType<ElevatorController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var elevator in elevators)
            elevator.InitializeFirstTriggerState(player.transform.position);
    }

    private void RaiseFloorEvents()
    {
        if (progress.ConsumeReturningFlag())
        {
            OnLoopReset?.Invoke();
        }
        else
        {
            if (progress.TryMarkVisited())
            {
                OnFloorFirstVisited?.Invoke(progress.CurrentFloor);
            }
        }
    }

    public void CheckAnswer(TriggerType choice)
    {
        if (progress.IsCleared)
            return;

        progress.Submit(choice, mapSpawner.HasAbnormal);
        StartLoop();
    }
}
