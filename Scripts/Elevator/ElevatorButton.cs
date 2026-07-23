using UnityEngine;
using System;

public class ElevatorButton : MonoBehaviour
{
    [Header("엘레베이터 설정")]
    [SerializeField] private ElevatorController elevatorController;

    [Header("플레이어 감지")]
    private bool isPlayerInTrigger = false;

    public static event Action<bool> OnPlayerNearButton;

    private void Awake()
    {
        AutoBindUI();
    }

    void Update()
    {
        if (isPlayerInTrigger && Input.GetKeyDown(KeyCode.F))
        {
            if (elevatorController != null)
            {
                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlayButtonSound();

                elevatorController.StartCoroutine(elevatorController.SetDoors(true));
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
            OnPlayerNearButton?.Invoke(true);   
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
            OnPlayerNearButton?.Invoke(false);
        }
    }
    private void AutoBindUI() // UI 자동화 함수
    {
        if (elevatorController == null)
            elevatorController = GetComponentInParent<ElevatorController>();
    }
}
