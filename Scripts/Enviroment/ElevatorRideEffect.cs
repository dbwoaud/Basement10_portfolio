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
    
    void Start()
    {
        initialPosition = transform.localPosition;
        if(SoundManager.instance != null)
            SoundManager.instance.PlayElevatorMovingSound();
    }

    void Update()
    {
        if (!isMoving)
            return;

        float x = (Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) - 0.5f) * shakeAmount;
        float y = (Mathf.PerlinNoise(0f,Time.time * shakeSpeed) - 0.5f) * shakeAmount * 2f;
        transform.localPosition = initialPosition + new Vector3(x, y, 0f);
    }

    public void StopElevator() // 엘리베이터를 이동을 멈추는 함수
    {
        isMoving = false;
        transform.localPosition = initialPosition;
        if (SoundManager.instance != null)
        {
            SoundManager.instance.StopBGM();
            SoundManager.instance.PlayElevatorFinishSound();
        }
    }
}
