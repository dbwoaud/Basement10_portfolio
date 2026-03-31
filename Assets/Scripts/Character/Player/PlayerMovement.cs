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
    [SerializeField] private Vector3 verticalVelocity;
    public bool canMove = true;

    [Header("시야 설정")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float verticalClamp = 80f;
    [SerializeField] private float verticalLookRotation;

    private void Awake()
    {
        InitializeComponents();
    }

    void Update()
    {
        ApplyGravity();
    }

    private void InitializeComponents() // 컴포넌트 초기화 함수
    {
        if(characterController == null)
            characterController = GetComponent<CharacterController>();

        if(footstepController == null)
            footstepController = GetComponent<FootstepController>();

        if (cameraPivot == null && Camera.main != null)
            cameraPivot = Camera.main.transform;
    }

    public void Move(Vector3 moveInput, bool isRunning) // 플레이어 이동을 다루는 함수
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

    public void Look(float mouseX, float mouseY) // 플레이어 시야를 다루는 함수
    {
        if (!canMove || cameraPivot == null)
            return;

        transform.Rotate(Vector3.up * mouseX);
        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -verticalClamp, verticalClamp);
        cameraPivot.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
    }

    private void ApplyGravity() // 플레이어에게 중력을 적용하는 함수
    {
        if(characterController.isGrounded && verticalVelocity.y < 0f)
            verticalVelocity.y = -2f;

        verticalVelocity.y += gravity * Time.deltaTime;
    }
}
