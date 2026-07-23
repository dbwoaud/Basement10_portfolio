using UnityEngine;

public class CameraLook : SettingApplierBase
{
    [Header("참조")]
    [SerializeField] private Transform playerBody;

    [Header("회전 제한")]
    [SerializeField] private float minPitch = -90f;
    [SerializeField] private float maxPitch = 90f;

    [Header("가속도 기준")]
    [Tooltip("이 속도를 넘어서면 가속 배율이 붙기 시작한다.")]
    [SerializeField] private float accelBaseSpeed = 0.1f;
    [Tooltip("가속으로 늘어날 수 있는 최대 추가 배율.")]
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

    protected override void Apply(GameSetting settings)
    {
        sensitivity = settings.mouseSensitivity;
        accelAmount = settings.cameraAccel;
    }

    private void Update()
    {
        if (!IsLookEnabled) 
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

    private float CalculateAccelMultiplier(Vector2 raw)
    {
        if (accelAmount <= 0f) 
            return 1f;

        float dt = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
        float speed = raw.magnitude / dt;
        float bonus = Mathf.Clamp(speed / accelBaseSpeed - 1f, 0f, accelMaxBonus);

        return 1f + accelAmount * bonus;
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        return angle > 180f ? angle - 360f : angle;
    }
}