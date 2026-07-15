using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(FootstepController))]
public class NPCMovement : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float walkSpeed = 4.0f;
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private FootstepController footstepController;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private int currentWaypoint = 0;

    [Header("애니메이션 설정")]
    [SerializeField] private Animator animator;
    public bool opening = false;

    [Header("이상현상 설정")]
    [SerializeField] private bool isMuted = false;
    [SerializeField] private bool isDoubleSound = false;

    private void Awake()
    {
        InitializeComponents();
    }

    void Start()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        SetDestinationToCurrentWaypoint();
        UpdateAnimator();
    }

    void Update()
    {
        if (opening || waypoints == null || waypoints.Length == 0)
            return;

        CheckWayPointArrival();
        HandleFootsteps();
    }

    private void InitializeComponents() // 컴포넌트 초기화 함수
    {
        if(animator == null)
            animator = GetComponent<Animator>();

        if(navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            if(navMeshAgent != null)
                navMeshAgent.speed = walkSpeed;
        }

        if(footstepController == null)
            footstepController = GetComponent<FootstepController>();
    }

    private void CheckWayPointArrival() // NPC가 체크포인트에 도달했는지 확인하는 함수
    {
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + 0.05f)
        {
            if (currentWaypoint >= waypoints.Length - 1)
            {
                opening = true;
                navMeshAgent.isStopped = true;
                UpdateAnimator();
            }
            else
            {
                currentWaypoint++;
                SetDestinationToCurrentWaypoint();
            }
        }
    }

    private void SetDestinationToCurrentWaypoint() // NPC의 목적지를 설정하는 함수
    {
        navMeshAgent.updatePosition = true;
        navMeshAgent.updateRotation = true;
        navMeshAgent.SetDestination(waypoints[currentWaypoint].position);
    }

    private void HandleFootsteps() // NPC의 발자국 소리를 설정하는 함수
    {
        if (isMuted)
        {
            footstepController.CalculateAndPlayFootstep(false, 1f);
            return;
        }

        bool isMoving = navMeshAgent.velocity.magnitude > 0.1f && !navMeshAgent.isStopped;
        float currentSpeedRatio = isDoubleSound ? 2f : 1f;
        footstepController.CalculateAndPlayFootstep(isMoving, currentSpeedRatio);
    }

    public void SetAbnormalStatus(bool mute, bool doubleSound) // NPC의 이상 현상을 설정하는 함수
    {
        this.isMuted = mute;
        this.isDoubleSound = doubleSound;
    }

    private void UpdateAnimator() // 다음 애니메이션을 재생하도록 변수를 설정하는 함수
    {
        if (animator != null)
            animator.SetBool("Opening", opening);
    }

    public void LookAtTarget(Vector3 targetPos) // NPC가 특정 위치를 바라보게 만드는 함수
    {
        StartCoroutine(LookAtCoroutine(targetPos));
    }

    private IEnumerator LookAtCoroutine(Vector3 targetPos) // NPC가 특정 위치를 바라보게 만드는 코루틴
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        dir.y = 0f;
        if (dir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            float rotationSpeed = 2.0f;
            while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                yield return null; 
            }

            transform.rotation = targetRotation;
        }
    }

    public void UpdateAnimator(Animator newAnimator)
    {
        this.animator = newAnimator;
    }
}