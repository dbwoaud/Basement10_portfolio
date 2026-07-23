using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class BadEndingManager : MonoBehaviour
{
    [Header("씬 전환 설정")]
    [SerializeField] private string nextSceneName = "EndingCredit";
    [SerializeField] private float transferTime = 5.0f;

    private void Start()
    {
        StartCoroutine(BadEndingCoroutine());
    }

    private IEnumerator BadEndingCoroutine() // 배드 엔딩을 재생하는 코루틴
    {
        yield return StartCoroutine(BadEndingSequenceCoroutine());
        yield return StartCoroutine(PlayFadeAndAudioCoroutine());
        yield return StartCoroutine(PlayMonologueCoroutine());
        yield return StartCoroutine(TransitionToNextSceneCoroutine());
    }

    IEnumerator BadEndingSequenceCoroutine() // 배드 엔딩 시퀀스를 재생하는 코루틴
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.showFloorNumber = false;
            GameManager.Instance.StartLoop();
        }

        yield return new WaitForEndOfFrame();

        PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
        NPCMovement npc = FindAnyObjectByType<NPCMovement>();

        if (player != null) 
            player.canMove = false;

        if (npc != null)
        {
            Transform anchor = npc.transform.Find("CameraAnchor");
            if (anchor != null)
            {
                Camera.main.transform.SetParent(anchor);
                Camera.main.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }

            yield return new WaitUntil(() => npc.opening);

            if(GameManager.Instance != null && GameManager.Instance.player)
                npc.LookAtTarget(GameManager.Instance.player.transform.position);
        }

        yield return new WaitForSeconds(transferTime);
    }

    private IEnumerator PlayFadeAndAudioCoroutine() // 페이드 효과와 오디오를 설정하는 코루틴
    {
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.FadeIn(3.0f);
            yield return new WaitUntil(() => !FadeManager.Instance.isFading);
            FadeManager.Instance.SetBlackBackGround(false);
        }

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayBGM(SoundManager.Instance.BadEndingBGM, 0.8f);
    }

    private IEnumerator PlayMonologueCoroutine() // 독백을 재생하는 코루틴
    {
        if (BadEndingUIManager.instance != null)
            yield return StartCoroutine(BadEndingUIManager.instance.PlayMonologueSequence());
    }

    private IEnumerator TransitionToNextSceneCoroutine() // 다음 씬으로 이동을 준비하는 코루틴
    {
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.SetAllBackground(false);
            FadeManager.Instance.SetWhiteBackGround(true);
            FadeManager.Instance.FlashIn(transferTime);
            yield return new WaitUntil(() => !FadeManager.Instance.isFading);
        }
        SceneManager.LoadScene(nextSceneName);
    }
}