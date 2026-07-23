using UnityEngine;

public class HeadBob : SettingApplierBase
{
    [Header("참조")]
    [SerializeField] private CharacterController controller;

    [Header("반동 파라미터")]
    [SerializeField] private float frequency = 8f;
    [SerializeField] private float amplitudeY = 0.05f;
    [SerializeField] private float amplitudeX = 0.03f;
    [SerializeField] private float returnSpeed = 6f;

    private Vector3 defaultLocalPos;
    private float timer;
    private float shakeScale = 1f;

    private void Awake()
    {
        defaultLocalPos = transform.localPosition;

        if (controller == null)
            controller = GetComponentInParent<CharacterController>();
    }

    protected override void Apply(GameSetting settings)
    {
        shakeScale = settings.cameraShake;
        if (shakeScale <= 0f)
        {
            timer = 0f;
            transform.localPosition = defaultLocalPos;
        }
    }

    private void LateUpdate()
    {
        if (controller == null) 
            return;

        if (shakeScale <= 0f)
        {
            transform.localPosition = defaultLocalPos;
            return;
        }

        Vector3 flatVelocity = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
        bool isMoving = flatVelocity.sqrMagnitude > 0.01f && controller.isGrounded;

        if (isMoving)
        {
            timer += Time.deltaTime * frequency;

            Vector3 offset = new Vector3(
                Mathf.Cos(timer * 0.5f) * amplitudeX,
                Mathf.Sin(timer) * amplitudeY,
                0f) * shakeScale;

            transform.localPosition = defaultLocalPos + offset;
        }
        else
        {
            timer = 0f;
            transform.localPosition = Vector3.Lerp(
                transform.localPosition, defaultLocalPos, Time.deltaTime * returnSpeed);
        }
    }
}