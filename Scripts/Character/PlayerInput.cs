using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerInput : MonoBehaviour
{
    [Header("플레이어 이동 및 시야 설정")]
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private PlayerMovement playerMovement;

    private void Awake()
    {
        InitializeComponents();
    }

    void Update()
    {
        HandleMovementInput();
        HandleLookInput();
    }

    void InitializeComponents() // 컴포넌트 초기화 함수
    {
        if(playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void HandleMovementInput() // 이동 관련 입력을 다루는 함수
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        Vector3 moveInput = new Vector3(moveX, 0f, moveZ);
        playerMovement.Move(moveInput, isRunning);
    }

    private void HandleLookInput() // 시야 관련 입력을 다루는 함수
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        playerMovement.Look(mouseX, mouseY);
    }
}
