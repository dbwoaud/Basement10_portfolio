using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("게임 로직 요소")]
    [SerializeField] private ElevatorRideEffect rideEffect;
    [SerializeField] private string nextSceneName = "StoryMode";

    private void Awake()
    {
        if (rideEffect == null)
            rideEffect = FindAnyObjectByType<ElevatorRideEffect>();
    }

    private void Start()
    {
        if (FadeManager.instance != null)
            FadeManager.instance.SetAllBackground(false);
    }

    public void StartGameSequence() // 게임 시작 시 연출을 수행하는 함수
    {
        StartCoroutine(StartGameSequenceCoroutine());
    }

    public IEnumerator StartGameSequenceCoroutine() // 게임 시작 시 연출을 수행하는 코루틴 
    {
        yield return new WaitForSeconds(1.5f);

        if(rideEffect != null)
            rideEffect.StopElevator();

        yield return new WaitForSeconds(1.0f);

        if (FadeManager.instance != null)
        {
            FadeManager.instance.FadeIn(1.5f);
            yield return new WaitUntil(() => !FadeManager.instance.isFading);
        }

        SceneManager.LoadScene(nextSceneName);
    }
}