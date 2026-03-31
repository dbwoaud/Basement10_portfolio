using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingCreditManager : MonoBehaviour
{
    [Header("상태 관리")]
    [SerializeField] private bool isTransitioning = false;

    [Header("씬 전환 설정")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private float transferDuration = 3.0f;

    private void Start()
    {
        if (SoundManager.instance != null && SoundManager.instance.EndingCreditBGM != null)
            SoundManager.instance.PlayBGM(SoundManager.instance.EndingCreditBGM, 0.8f);
    }

    public void GoToMainMenu() // 메인메뉴로 이동하는 함수
    {
        if (isTransitioning) 
            return;

        isTransitioning = true;
        StartCoroutine(ReturnToMainMenuCoroutine());
    }

    private IEnumerator ReturnToMainMenuCoroutine() // 메인메뉴로 이동하는 코루틴
    {
        if (FadeManager.instance != null)
        {
            FadeManager.instance.SetBlackBackGround(true);
            FadeManager.instance.FadeIn(5.0f);
            yield return new WaitUntil(() => !FadeManager.instance.isFading);
        }

        yield return new WaitForSeconds(transferDuration);
        SceneManager.LoadScene(mainMenuScene);
    }
}