using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrueEndingManager : MonoBehaviour
{
    [Header("씬 전환 설정")]
    [SerializeField] private string nextSceneName = "EndingCredit";
    [SerializeField] private float endingWaitTime = 5.0f;

    private void Start()
    {
        StartCoroutine(TrueEndingCoroutine());
    }

    private IEnumerator TrueEndingCoroutine() // 진엔딩을 재생하는 코루틴 
    {
        yield return StartCoroutine(TrueEndingSequenceCoroutine());
        SetMonologueSequence();
        yield return StartCoroutine(PlayMonologueCoroutine());
        yield return StartCoroutine(TransitionToNextSceneCoroutine());
    }

    private IEnumerator TrueEndingSequenceCoroutine() // 진엔딩 시퀀스를 재생하는 코루틴
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayBGM(SoundManager.Instance.EyeOpeningBGM);

        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.SetWhiteBackGround(true);

            FadeManager.Instance.FlashOut(endingWaitTime);
            yield return new WaitUntil(() => !FadeManager.Instance.isFading);

            yield return new WaitForSeconds(endingWaitTime);

            FadeManager.Instance.FlashIn(endingWaitTime);
            yield return new WaitUntil(() => !FadeManager.Instance.isFading);
        }
    }

    private void SetMonologueSequence() // 독백 출력을 위한 설정을 진행하는 함수
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopAllSound();
            SoundManager.Instance.PlayBGM(SoundManager.Instance.TrueEndingBGM, 0.8f);
        }

        if (FadeManager.Instance != null)
            FadeManager.Instance.SetWhiteBackGround(false);
    }

    private IEnumerator PlayMonologueCoroutine() // 독백을 재생하는 코루틴
    {
        if (TrueEndingUIManager.Instance != null)
            yield return StartCoroutine(TrueEndingUIManager.Instance.PlayMonologueSequence());
    }

    private IEnumerator TransitionToNextSceneCoroutine() // 다음 씬으로 이동을 준비하는 코루틴
    {
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.SetAllBackground(false);
            FadeManager.Instance.SetBlackBackGround(true);
            FadeManager.Instance.FadeIn(5.0f);
            yield return new WaitUntil(() => !FadeManager.Instance.isFading);
        }

        SceneManager.LoadScene(nextSceneName);
    }
}