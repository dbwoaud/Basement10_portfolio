using System;
using UnityEngine;

public enum EndType { Bad, True }; // 엔딩 종류

public class EndingTrigger: MonoBehaviour
{
    [Header("엔딩 설정")]
    [SerializeField] private EndType endType;
    private bool isTriggered = false;

    public static event Action<EndType> OnEndingTriggered;


    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered)
            return;

        if(other.CompareTag("Player"))
        {
            isTriggered = true;
            OnEndingTriggered?.Invoke(endType);
        }
    }
}