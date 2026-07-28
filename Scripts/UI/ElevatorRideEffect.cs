using UnityEngine;

public class ElevatorRideEffect : MonoBehaviour
{
    [Header("진동 설정")]
    [SerializeField] private float shakeAmount = 0.02f;
    [SerializeField] private float shakeSpeed = 20f;

    [Header("위치 설정")]
    private Vector3 initialPosition;

    [Header("상태 변수")]
    private bool isMoving = true;


    private void Awake()
    {
        initialPosition = transform.localPosition;
    }

    private void Start()
    {
        if (SoundManager.HasInstance)
            SoundManager.Instance.PlayElevatorMovingSound();
    }

    void Update()
    {
        if (!isMoving)
            return;

        float x = (Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) - 0.5f) * shakeAmount;
        float y = (Mathf.PerlinNoise(0f,Time.time * shakeSpeed) - 0.5f) * shakeAmount * 2f;
        transform.localPosition = initialPosition + new Vector3(x, y, 0f);
    }

    public void StopElevator() // 엘리베이터 이동 연출을 중지하는 함수
    {
        isMoving = false;
        transform.localPosition = initialPosition;
        if (SoundManager.HasInstance)
        {
            SoundManager.Instance.StopAmbience();
            SoundManager.Instance.PlayElevatorFinishSound();
        }
    }
}