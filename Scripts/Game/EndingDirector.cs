using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingDirector : MonoBehaviour
{
    [Header("엔딩 설정 정보")]
    [SerializeField] private string badEndingSceneName = "BadEnding"; // 배드엔딩 씬 이름
    [SerializeField] private string trueEndingSceneName = "TrueEnding"; // 진엔딩 씬 이름
    [SerializeField] private float badEndingFadeDuration = 2.0f; // 페이드 지속시간
    [SerializeField] private float trueEndingFlashDuration = 2.0f; // 플래시 지속시간

    public bool IsEnded { get; private set; } // 엔딩 도달 여부
    public string BadEndingSceneName => badEndingSceneName;


    private void OnEnable()
    {
        EndingTrigger.OnEndingTriggered += Play;
    }

    private void OnDisable()
    {
        EndingTrigger.OnEndingTriggered -= Play;
    }

    public void ResetState() // 엔딩 도달 여부를 초기화하는 함수
    {
        IsEnded = false;
    }

    public void Play(EndType type) // 엔딩 시퀀스를 재생하는 함수
    {
        if (IsEnded)
            return;

        IsEnded = true;
        StartCoroutine(EndingSequenceCoroutine(type));
    }

    private IEnumerator EndingSequenceCoroutine(EndType type) // 엔딩 시퀀스를 재생하는 코루틴
    {
        if (SoundManager.HasInstance)
            SoundManager.Instance.StopAllSound();

        if (FadeManager.HasInstance)
        {
            if (type == EndType.Bad)
                FadeManager.Instance.FadeIn(badEndingFadeDuration);
            else if (type == EndType.True)
                FadeManager.Instance.FlashIn(trueEndingFlashDuration);

            yield return null;
            yield return new WaitUntil(() => !FadeManager.Instance.isFading);
        }

        string sceneName = (type == EndType.Bad) ? badEndingSceneName : trueEndingSceneName;
        SceneManager.LoadScene(sceneName);
    }
}