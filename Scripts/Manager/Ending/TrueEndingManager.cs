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
        if (SoundManager.instance != null)
            SoundManager.instance.PlayBGM(SoundManager.instance.EyeOpeningBGM);

        if (FadeManager.instance != null)
        {
            FadeManager.instance.SetWhiteBackGround(true);

            FadeManager.instance.FlashOut(endingWaitTime);
            yield return new WaitUntil(() => !FadeManager.instance.isFading);

            yield return new WaitForSeconds(endingWaitTime);

            FadeManager.instance.FlashIn(endingWaitTime);
            yield return new WaitUntil(() => !FadeManager.instance.isFading);
        }
    }

    private void SetMonologueSequence() // 독백 출력을 위한 설정을 진행하는 함수
    {
        if (SoundManager.instance != null)
        {
            SoundManager.instance.StopAllSound();
            SoundManager.instance.PlayBGM(SoundManager.instance.TrueEndingBGM, 0.8f);
        }

        if (FadeManager.instance != null)
            FadeManager.instance.SetWhiteBackGround(false);
    }

    private IEnumerator PlayMonologueCoroutine() // 독백을 재생하는 코루틴
    {
        if (TrueEndingUIManager.instance != null)
            yield return StartCoroutine(TrueEndingUIManager.instance.PlayMonologueSequence());
    }

    private IEnumerator TransitionToNextSceneCoroutine() // 다음 씬으로 이동을 준비하는 코루틴
    {
        if (FadeManager.instance != null)
        {
            FadeManager.instance.SetAllBackground(false);
            FadeManager.instance.SetBlackBackGround(true);
            FadeManager.instance.FadeIn(5.0f);
            yield return new WaitUntil(() => !FadeManager.instance.isFading);
        }

        SceneManager.LoadScene(nextSceneName);
    }
}