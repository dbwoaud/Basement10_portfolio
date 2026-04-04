using UnityEngine;

public class ElevatorTrigger : MonoBehaviour
{
    [SerializeField] private ElevatorController elevatorController;

    private void Awake()
    {
        AutoBindUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && elevatorController != null)
            elevatorController.PlayerEnteredInnerTrigger();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && elevatorController != null)
            elevatorController.PlayerExitedInnerTrigger();
        
    }

    private void AutoBindUI() // UI 자동화 함수
    {
        if (elevatorController == null)
            elevatorController = GetComponentInParent<ElevatorController>();
    }
}
