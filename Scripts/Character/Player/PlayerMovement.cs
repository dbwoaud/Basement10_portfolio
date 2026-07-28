using UnityEngine;

[RequireComponent (typeof(CharacterController))]
[RequireComponent(typeof(FootstepController))]

public class PlayerMovement : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private FootstepController footstepController;
    private Vector3 verticalVelocity;
    public bool canMove = true;


    private void Awake()
    {
        InitializeComponents();
    }

    void Update()
    {
        if (!canMove)
        {
            verticalVelocity.y = -2f;
            return;
        }
        ApplyGravity();
    }

    private void InitializeComponents() // 컴포넌트 초기화 함수
    {
        if(characterController == null)
            characterController = GetComponent<CharacterController>();

        if(footstepController == null)
            footstepController = GetComponent<FootstepController>();
    }

    public void Move(Vector3 moveInput, bool isRunning) // 플레이어의 이동을 설정하는 함수
    {
        if (!canMove || characterController == null)
            return;
        
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        Vector3 move = transform.TransformDirection(moveInput) * currentSpeed;
        Vector3 finalVelocity = move + verticalVelocity;
        characterController.Move(finalVelocity * Time.deltaTime);

        bool isMoving = characterController.isGrounded && moveInput.magnitude > 0f;
        float speedRatio = currentSpeed / walkSpeed;
        footstepController.CalculateAndPlayFootstep(isMoving, speedRatio);
    }

    private void ApplyGravity() // 플레이어에게 중력을 적용하는 함수
    {
        if(characterController.isGrounded && verticalVelocity.y < 0f)
            verticalVelocity.y = -2f;

        verticalVelocity.y += gravity * Time.deltaTime;
    }
}