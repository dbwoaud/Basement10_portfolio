using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [Header("게임 설정")]
    [SerializeField] private int startFloor = 10;
    [SerializeField] private int currentFloor = 10;
    [SerializeField] private int targetFloor = 0;
    [SerializeField] private bool[] visitedFloors = new bool[11];
    [SerializeField] private bool isReturningFromFailure = false;
    public bool showFloorNumber = true;

    [Header("맵 생성 설정")]
    [SerializeField] private GameObject map;
    [SerializeField] private GameObject finalMap;
    [SerializeField] private Transform mapSpawnPoint;

    [Header("플레이어 설정")]
    public GameObject player;
    [SerializeField] private Vector3 playerSpawnPosition;
    [SerializeField] private Quaternion playerSpawnRotation;

    [Header("이상 현상 설정")]
    [SerializeField] private GameObject currentMapInstance;
    [SerializeField] private AbnormalData currentAbnormalData;

    [Header("씬 이름 설정")]
    [SerializeField] private string currentSceneName = "StoryMode";
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string badEndingSceneName = "BadEnding";
    [SerializeField] private string trueEndingSceneName = "TrueEnding";

    public static event Action<int> OnFloorFirstVisited;
    public static event Action OnLoopReset;

    public bool isEnded { get; private set; } = false;

    override protected void Awake()
    {
        base.Awake();
        if(Instance == this)
            SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnEnable()
    {
        EndingTrigger.OnEndingTriggered += ProcessEnding;
        ElevatorController.OnElevatorAnswerSelected += CheckAnswer;
    }

    private void OnDisable()
    {
        EndingTrigger.OnEndingTriggered -= ProcessEnding;
        ElevatorController.OnElevatorAnswerSelected -= CheckAnswer;
    }

    private void OnDestroy() 
    { 
        if(Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded; 
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) // 씬 로딩 시 실행할 함수
    {
        if (scene.name == mainMenuSceneName)
        {
            isEnded = false;
            currentFloor = startFloor;
            isReturningFromFailure = false;

            for (int i = 0; i < visitedFloors.Length; i++)
                visitedFloors[i] = false;

            return;
        }

        GameObject foundPlayer = GameObject.FindWithTag("Player");
        if (foundPlayer != null) 
            player = foundPlayer;

        GameObject foundSpawnPoint = GameObject.Find("MapSpawnPoint");
        if (foundSpawnPoint != null) 
            mapSpawnPoint = foundSpawnPoint.transform;

        if (scene.name == currentSceneName)
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

    public void StartLoop() // 게임 로직을 실행하는 함수
    {
        if (player != null)
        {
            var movement = player.GetComponent<PlayerMovement>();
            if (movement != null)
                movement.canMove = true;
        }

        if (FadeManager.Instance != null)
            FadeManager.Instance.FadeOut(2.0f);

        GenerateMap();
        UpdateFloorDisplay();
        ResetPlayerPosition();
        ResetAbnormal();
        HandleFloorEvents();
    }

    private void GenerateMap() // 맵을 생성하는 함수
    {
        if (currentMapInstance != null) 
            Destroy(currentMapInstance);

        if(SceneManager.GetActiveScene().name == badEndingSceneName)
        {
            currentMapInstance = Instantiate(map, mapSpawnPoint.position, mapSpawnPoint.rotation);
            currentAbnormalData = null;
            return;
        }

        if (currentFloor == startFloor)
        {
            currentMapInstance = Instantiate(map, mapSpawnPoint.position, mapSpawnPoint.rotation);
            currentAbnormalData = null;
            Debug.Log("10층에는 이상현상이 없습니다.");
        }

        else if (currentFloor > targetFloor)
        {
            currentMapInstance = Instantiate(map, mapSpawnPoint.position, mapSpawnPoint.rotation);
            if (SpawnAbnormalManager.Instance != null)
            {
                SpawnAbnormalManager.Instance.mapRoot = currentMapInstance;
                currentAbnormalData = SpawnAbnormalManager.Instance.SelectAbnormal();
                if (currentAbnormalData != null)
                    Debug.Log("이상현상 번호: " + currentAbnormalData.abnormalName + "\n 이상현상 설명: " + currentAbnormalData.abnormalDescription);
                else
                    Debug.Log("현재 층에는 이상현상이 없습니다.");
                
            }
        }

        else
        {
            Vector3 finalMapPos = mapSpawnPoint.position - new Vector3(0f, 0f, 5f);
            currentMapInstance = Instantiate(finalMap, finalMapPos, mapSpawnPoint.rotation);
            currentAbnormalData = null;
        }
    }

    private void UpdateFloorDisplay() // 맵에 현재 층을 출력하는 함수
    {
        if (currentMapInstance == null) 
            return;

        FloorNumberDisplay display = currentMapInstance.GetComponentInChildren<FloorNumberDisplay>();

        if (display != null)
        {
            if(showFloorNumber)
                display.SetFloorNumber(currentFloor);
            else
                display.ResetFloorNumber();
        }       
    }

    private void ResetPlayerPosition()
    {
        if (player == null) 
            return;

        CharacterController cc = player.GetComponent<CharacterController>();

        if (cc != null) 
            cc.enabled = false;

        player.transform.SetPositionAndRotation(playerSpawnPosition, playerSpawnRotation);

        if (cc != null) 
            cc.enabled = true;

    }

    private void ResetAbnormal() // 이상현상을 초기화하는 함수
    {
        FootstepController playerFootstep = player.GetComponent<FootstepController>();
        if (playerFootstep != null)
            playerFootstep.SetAbnormalStatus(false, false);  

        NPCMovement npc = FindAnyObjectByType<NPCMovement>();
        if (npc != null)
        {
            FootstepController npcFootstep = npc.GetComponent<FootstepController>();
            if (npcFootstep != null)
                npcFootstep.SetAbnormalStatus(false, false);
        }
    }

    private void HandleFloorEvents() // 층 진입 결과에 따른 상태 이벤트를 처리하는 함수
    {
        if (isReturningFromFailure)
        {
            OnLoopReset?.Invoke();
            isReturningFromFailure = false;
        }

        else if (currentFloor >= 0 && currentFloor <= startFloor)
        {
            if (!visitedFloors[currentFloor])
            {
                visitedFloors[currentFloor] = true;
                OnFloorFirstVisited?.Invoke(currentFloor);
            }
        }
    }

    public void CheckAnswer(TriggerType choice) // 플레이어의 선택에 따른 게임 로직을 실행하는 함수
    {
        if (currentFloor == targetFloor) 
            return;

        bool isAbnormal = (currentAbnormalData != null);
        bool isCorrect = (choice == TriggerType.Exit && !isAbnormal) ||
                         (choice == TriggerType.Return && isAbnormal);

        if (isCorrect)
        {
            currentFloor--;
            isReturningFromFailure = false;
        }
        else
        {
            currentFloor = startFloor;
            isReturningFromFailure = true;
        }
        StartLoop();
    }

    public void ProcessEnding(EndType type) // 엔딩을 진행하는 함수
    {
        if (isEnded) 
            return;

        isEnded = true;
        StartCoroutine(EndingSequenceCoroutine(type));
    }

    IEnumerator EndingSequenceCoroutine(EndType type) // 엔딩을 진행하는 코루틴
    {
        if (SoundManager.Instance != null) 
            SoundManager.Instance.StopAllSound();

        if (FadeManager.Instance != null)
        {
            if (type == EndType.Bad)
                FadeManager.Instance.FadeIn();
            
            else if (type == EndType.True)
                FadeManager.Instance.FlashIn(2.0f);

            yield return new WaitUntil(() => !FadeManager.Instance.isFading);
        }

        string sceneName = (type == EndType.Bad) ? badEndingSceneName : trueEndingSceneName;
        SceneManager.LoadScene(sceneName);
    }
}