using System;
using System.Collections;
using UnityEngine;

public enum TriggerType { Exit, Return }

public class ElevatorController : MonoBehaviour
{
    public static bool IsTeleporting = false;

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
    [SerializeField] private Collider innerTriggerCollider;
    public bool isOpen {  get; private set; }

    [Header("연출 설정")]
    [SerializeField] private Transform standPoint;
    [SerializeField] private float standPointMoveDuration = 3.0f;

    private CameraLook cameraLook;

    public static event Action<TriggerType> OnElevatorAnswerSelected;

    private void Awake()
    {
        AutoBindUI();
    }

    private void Start()
    {
        InitializePlayerRef();      
    }

    private void InitializePlayerRef()
    {
        if (playerMovement == null)
        {
            playerMovement = FindAnyObjectByType<PlayerMovement>();
            if (playerMovement != null)
            {
                playerTransform = playerMovement.transform;
                if (Vector3.Distance(transform.position, playerTransform.position) < detectionDistance)
                    ignoreFirstTrigger = true;
            }
        }
    }

    public void InitializeFirstTriggerState(Vector3 playerPosition)
    {
        InitializePlayerRef();
        ignoreFirstTrigger = innerTriggerCollider != null
            && innerTriggerCollider.bounds.Contains(playerPosition);
    }

    void Update()
    {
        if (playerTransform == null || isSequenceRunning) 
            return;

        HandleProximityLogic();
    }

    private void OnDisable()
    {
        if (cameraLook != null)
            cameraLook.IsLookEnabled = true;
    }

    void AutoBindUI() // UI 자동화 함수
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (transform.parent != null)
            parentAnimator = transform.parent.GetComponent<Animator>();

        if (innerTriggerCollider == null)
        {
            ElevatorTrigger trigger = GetComponentInChildren<ElevatorTrigger>();
            if (trigger != null)
                innerTriggerCollider = trigger.GetComponent<Collider>();
        }

        if (standPoint == null)
            standPoint = transform.Find("StandPoint");
    }

    public void PlayerEnteredInnerTrigger() // 플레이어가 엘리베이터 안쪽 영역에 닿을 때 실행되는 함수
    {
        InitializePlayerRef();

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
        if (cameraLook != null)
        {
            cameraLook.ResyncFromTransforms();
            cameraLook.IsLookEnabled = true;
        }

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

        if (cameraLook == null)
            cameraLook = playerTransform.GetComponentInChildren<CameraLook>();

        if (cameraLook != null)
            cameraLook.IsLookEnabled = false;

        Transform camTransform = (cameraLook != null) ? cameraLook.transform : Camera.main?.transform;

        Vector3 startPos = playerTransform.position;
        Quaternion startRot = playerTransform.rotation;
        Quaternion startCamRot = (camTransform != null) ? camTransform.localRotation : Quaternion.identity;

        float elapsed = 0f;

        while (elapsed < standPointMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / standPointMoveDuration);

            playerTransform.position = Vector3.Lerp(startPos, standPoint.position, t);
            playerTransform.rotation = Quaternion.Slerp(startRot, standPoint.rotation, t);

            if (camTransform != null)
                camTransform.localRotation = Quaternion.Slerp(startCamRot, Quaternion.identity, t);

            yield return null;
        }

        playerTransform.SetPositionAndRotation(standPoint.position, standPoint.rotation);

        if (camTransform != null)
            camTransform.localRotation = Quaternion.identity;

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
