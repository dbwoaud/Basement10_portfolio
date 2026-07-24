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
        if (ElevatorController.IsTeleporting)
            return;

        if (other.CompareTag("Player") && elevatorController != null)
            elevatorController.PlayerEnteredInnerTrigger();
    }

    private void OnTriggerExit(Collider other)
    {
        if (ElevatorController.IsTeleporting)
            return;

        if (other.CompareTag("Player") && elevatorController != null)
            elevatorController.PlayerExitedInnerTrigger();
        
    }

    private void AutoBindUI() // UI �ڵ�ȭ �Լ�
    {
        if (elevatorController == null)
            elevatorController = GetComponentInParent<ElevatorController>();
    }
}
