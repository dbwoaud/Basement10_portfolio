using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerInput : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private PlayerMovement playerMovement;


    private void Awake()
    {
        InitializeComponents();
    }

    void Update()
    {
        if (Time.timeScale <= 0f)
            return;

        HandleMovementInput();
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
}