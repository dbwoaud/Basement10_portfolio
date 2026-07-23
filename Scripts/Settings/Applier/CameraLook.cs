using UnityEngine;

public class CameraLook : SettingApplierBase
{
    [Header("참조")]
    [SerializeField] private Transform playerBody;

    [Header("회전 제한")]
    [SerializeField] private float minPitch = -90f;
    [SerializeField] private float maxPitch = 90f;

    [Header("가속도 기준")]
    [SerializeField] private float accelBaseSpeed = 0.1f;
    [SerializeField] private float accelMaxBonus = 2f;

    private float sensitivity = 2f;
    private float accelAmount = 0f;
    private float pitch;

    public bool IsLookEnabled { get; set; } = true;


    private void Awake()
    {
        if (playerBody == null)
            playerBody = transform.root;

        pitch = NormalizeAngle(transform.localEulerAngles.x);
    }

    protected override void Apply(GameSetting setting) // 게임 설정을 적용하는 함수
    {
        sensitivity = setting.mouseSensitivity;
        accelAmount = setting.cameraAccel;
    }

    private void Update()
    {
        if (!IsLookEnabled || Time.timeScale <= 0f) 
            return;

        Vector2 raw = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        if (raw.sqrMagnitude < 0.0000001f) 
            return;

        Vector2 delta = raw * sensitivity * CalculateAccelMultiplier(raw);
        pitch = Mathf.Clamp(pitch - delta.y, minPitch, maxPitch);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        if (playerBody != null)
            playerBody.Rotate(Vector3.up * delta.x);
    }

    private static float NormalizeAngle(float angle) // 각도를 -180 ~ 180 로 정규화하는 함수
    {
        angle %= 360f;
        return angle > 180f ? angle - 360f : angle;
    }

    private float CalculateAccelMultiplier(Vector2 raw) // 가속도를 계산하는 함수
    {
        if (accelAmount <= 0f) 
            return 1f;

        float dt = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
        float speed = raw.magnitude / dt;
        float bonus = Mathf.Clamp(speed / accelBaseSpeed - 1f, 0f, accelMaxBonus);

        return 1f + accelAmount * bonus;
    }

    public void ResetPitch() // 피치를 초기화하는 함수
    {
        pitch = 0f;
        transform.localRotation = Quaternion.identity;
    }
}