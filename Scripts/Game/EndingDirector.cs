using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingDirector : MonoBehaviour
{
    [SerializeField] private string badEndingSceneName = "BadEnding";
    [SerializeField] private string trueEndingSceneName = "TrueEnding";
    [SerializeField] private float badEndingFadeDuration = 2.0f;
    [SerializeField] private float trueEndingFlashDuration = 2.0f;

    public bool IsEnded { get; private set; }
    public string BadEndingSceneName => badEndingSceneName;

    private void OnEnable()
    {
        EndingTrigger.OnEndingTriggered += Play;
    }

    private void OnDisable()
    {
        EndingTrigger.OnEndingTriggered -= Play;
    }

    public void ResetState()
    {
        IsEnded = false;
    }

    public void Play(EndType type)
    {
        if (IsEnded)
            return;

        IsEnded = true;
        StartCoroutine(EndingSequenceCoroutine(type));
    }

    private IEnumerator EndingSequenceCoroutine(EndType type)
    {
        if (SoundManager.HasInstance)
        {
            SoundManager.Instance.StopAllSound();
        }

        if (FadeManager.HasInstance)
        {
            if (type == EndType.Bad)
            {
                FadeManager.Instance.FadeIn(badEndingFadeDuration);
            }
            else if (type == EndType.True)
            {
                FadeManager.Instance.FlashIn(trueEndingFlashDuration);
            }

            // Wait a frame to allow the FadeManager's coroutine to start and set isFading to true
            yield return null;

            yield return new WaitUntil(() => !FadeManager.Instance.isFading);
        }

        string sceneName = (type == EndType.Bad) ? badEndingSceneName : trueEndingSceneName;
        SceneManager.LoadScene(sceneName);
    }
}
