using System;
using System.Collections;
using UnityEngine;

public enum TriggerType { Exit, Return }

public class ElevatorController : MonoBehaviour
{
    [Header("설정")]
    public TriggerType type;
    [SerializeField] private float detectionDistance = 3f; 
    [SerializeField] private float transferDelay = 1f;
    [SerializeField] private float doorAnimDuration = 3f;

    [Header("애니메이션 설정")]
    [SerializeField] private Animator animator;
    [SerializeField] private Animator parentAnimator;

    [Header("상태 관리 변수")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private bool isAnimating = false;
    [SerializeField] private bool isSequenceRunning = false;
    [SerializeField] private bool ignoreFirstTrigger = false;
    public bool isOpen {  get; private set; }

    [Header("연출 설정")]
    [SerializeField] private Transform standPoint;

    public static event Action<TriggerType> OnElevatorAnswerSelected;

    private void Awake()
    {
        AutoBindUI();
    }

    private void Start()
    {
        playerMovement = FindAnyObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerTransform = playerMovement.transform;
            if (Vector3.Distance(transform.position, playerTransform.position) < detectionDistance)
                ignoreFirstTrigger = true;
        }         
    }

    void Update()
    {
        if (playerTransform == null || isSequenceRunning) 
            return;

        HandleProximityLogic();
    }

    void AutoBindUI() // UI 자동화 함수
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (transform.parent != null)
            parentAnimator = transform.parent.GetComponent<Animator>();

        if (standPoint == null)
            standPoint = transform.Find("StandPoint");
    }

    public void PlayerEnteredInnerTrigger() // 플레이어가 엘리베이터 안쪽 영역에 닿을 때 실행되는 함수
    {
        if (ignoreFirstTrigger)
            return;

        if (!isSequenceRunning)
            StartCoroutine(ElevatorSequenceCoroutine());
    }

    public void PlayerExitedInnerTrigger() // 플레이어가 엘리베이터 안쪽 영역에 나갈 때 실행되는 함수
    {
        ignoreFirstTrigger = false;
    }

    private void HandleProximityLogic() // 엘리베이터 로직을 수행하는 함수
    {
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool isNear = distance < detectionDistance;

        if (isNear && !isOpen && !isAnimating && ignoreFirstTrigger)
            StartCoroutine(SetDoors(true));

        else if (!isNear && isOpen && !isAnimating)
            StartCoroutine(SetDoors(false));
    }

    private IEnumerator ElevatorSequenceCoroutine() // 엘리베이터 시퀀스를 재생하는 코루틴
    {
        isSequenceRunning = true;
        if (playerMovement) 
            playerMovement.canMove = false;

        yield return new WaitUntil(() => !isAnimating);

        Coroutine moveCoroutine = StartCoroutine(MovePlayerToStandPoint());
        Coroutine doorCoroutine = StartCoroutine(SetDoors(false));
        yield return moveCoroutine;
        yield return doorCoroutine;
   

        if (SoundManager.Instance != null)
            SoundManager.Instance.StopAllSound();

        FootstepController[] allFootsteps = FindObjectsByType<FootstepController>();
        foreach (var fc in allFootsteps)
            fc.StopFootsteps();

        if (FadeManager.Instance != null)
        { 
            FadeManager.Instance.FadeIn();
            yield return new WaitUntil(() => !FadeManager.Instance.isFading);
        }

        yield return new WaitForSeconds(transferDelay);
        OnElevatorAnswerSelected?.Invoke(type);
        isSequenceRunning = false;
    }

    public IEnumerator SetDoors(bool shouldOpen) // 엘리베이터 문을 설정하는 함수
    {
        if (isOpen == shouldOpen || isAnimating) 
            yield break;

        isOpen = shouldOpen;
        isAnimating = true;

        PlayDoorSound();
        UpdateAnimators(shouldOpen);

        yield return new WaitForSeconds(doorAnimDuration);
        isAnimating = false;
    }

    private IEnumerator MovePlayerToStandPoint() // 플레이어를 엘리베이터 중앙에 위치시키고, 정면을 바라보게 하는 함수
    {
        if (playerTransform == null || standPoint == null)
            yield break;

        CharacterController cc = playerTransform.GetComponent<CharacterController>();
        if (cc != null) 
            cc.enabled = false;

        Vector3 startPos = playerTransform.position;
        Quaternion startRot = playerTransform.rotation;

        Camera mainCam = Camera.main;
        if (mainCam != null)
            mainCam.transform.localRotation = Quaternion.Lerp(mainCam.transform.localRotation, Quaternion.identity, 1f);
        
        float elapsed = 0f;
        float duration = 3.0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            playerTransform.position = Vector3.Lerp(startPos, standPoint.position, t);
            playerTransform.rotation = Quaternion.Lerp(startRot, standPoint.rotation, t);
            yield return null;
        }

        playerTransform.position = standPoint.position;
        playerTransform.rotation = standPoint.rotation;

        if (cc != null) 
            cc.enabled = true;
    }

    private void UpdateAnimators(bool state) // 엘리베이터 애니메이션을 업데이트하는 함수
    {
        if (parentAnimator != null) 
            parentAnimator.SetBool("mainDoorOpen", state);

        if (animator != null) 
            animator.SetBool("elevatorDoorOpen", state);
    }

    private void PlayDoorSound() // 엘리베이터 문 효과음을 재생하는 함수
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayElevatorDoorSound();
    }
}
