using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(FootstepController))]
public class NPCMovement : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private int currentWaypoint = 0;
    [SerializeField] private FootstepController footstepController;
    [SerializeField] private float walkSpeed = 4.0f;
    [SerializeField] private NavMeshAgent navMeshAgent;

    [Header("애니메이션 설정")]
    [SerializeField] private Animator animator;
    public bool opening = false;

    [Header("회전 연출 설정")]
    [SerializeField] private float lookRotationSpeed = 2.0f;


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

    private void SetDestinationToCurrentWaypoint() // NPC의 NavMeshAgent 목적지를 설정하는 함수
    {
        navMeshAgent.updatePosition = true;
        navMeshAgent.updateRotation = true;
        navMeshAgent.SetDestination(waypoints[currentWaypoint].position);
    }

    private void CheckWayPointArrival() // NPC가 체크포인트에 도달했는지 확인하고 관련 동작을 수행하는 함수
    {
        if (navMeshAgent.pathPending)
            return;

        if (navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance + 0.05f)
            return;

        if (currentWaypoint >= waypoints.Length - 1)
        {
            opening = true;
            navMeshAgent.isStopped = true;
            UpdateAnimator();
            return;
        }

        currentWaypoint++;
        SetDestinationToCurrentWaypoint();
    }

    private void HandleFootsteps() // NPC의 발자국 소리를 설정하는 함수
    {
        bool isMoving = navMeshAgent.velocity.magnitude > 0.1f && !navMeshAgent.isStopped;
        footstepController.CalculateAndPlayFootstep(isMoving);
    }

    private void UpdateAnimator() // 애니메이션을 업데이트하는 함수
    {
        if (animator != null)
            animator.SetBool(AnimatorParams.Opening, opening);
    }

    public void LookAtTarget(Vector3 targetPos) // NPC가 특정 위치를 바라보게 만드는 함수
    {
        StartCoroutine(LookAtCoroutine(targetPos));
    }

    private IEnumerator LookAtCoroutine(Vector3 targetPos) // NPC가 특정 위치를 바라보게 만드는 코루틴
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        dir.y = 0f;

        if (dir == Vector3.zero)
            yield break;

        Quaternion targetRotation = Quaternion.LookRotation(dir);
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lookRotationSpeed);
            yield return null; 
        }

        transform.rotation = targetRotation;
    }
}