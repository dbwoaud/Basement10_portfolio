using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FadeManager : Singleton<FadeManager>
{
    [Header("페이드 이미지")]
    [SerializeField] private Image black;
    [SerializeField] private Image white;

    public bool isFading { get; private set; } = false;
    private Coroutine currentFadeCoroutine;


    override protected void Awake() 
    {
        base.Awake();
        if (Instance != this)
            return;

        AutoBindImages();
        SetAllBackground(false);
    }

    private void AutoBindImages() // 이미지를 자동 할당하는 함수
    {
        if (black == null)
            black = UIBinder.Find<Image>(transform, "BlackBackground");

        if (white == null)
            white = UIBinder.Find<Image>(transform, "WhiteBackground");
    }

    public void SetAllBackground(bool state) // 모든 배경화면을 활성화/비활성화하는 함수
    {
        SetBlackBackGround(state);
        SetWhiteBackGround(state);
    }

    public void SetWhiteBackGround(bool state) // 흰 배경화면을 활성화/비활성화하는 함수
    {
        if (white != null)
            white.gameObject.SetActive(state);
    }

    public void SetBlackBackGround(bool state) // 검은 배경화면을 활성화/비활성화하는 함수
    {
        if (black != null)
            black.gameObject.SetActive(state);
    }

    public void FadeOut(float duration = 2.0f) => StartFade(black, 1f, 0f, duration); 
    public void FadeIn(float duration = 2.0f) => StartFade(black, 0f, 1f, duration);
    public void FlashOut(float duration = 2.0f) => StartFade(white, 1f, 0f, duration);
    public void FlashIn(float duration = 2.0f) => StartFade(white, 0f, 1f, duration);

    private void StartFade(Image targetImage, float startAlpha, float targetAlpha, float duration) // 페이드 효과를 시작하는 함수
    {
        if (currentFadeCoroutine != null)
            StopCoroutine(currentFadeCoroutine);

        currentFadeCoroutine = StartCoroutine(StartFadeCoroutine(targetImage, startAlpha, targetAlpha, duration));
    }

    private IEnumerator StartFadeCoroutine(Image targetImage, float startAlpha, float targetAlpha, float duration) // 페이드 효과를 시작하는 코루틴
    {
        if (targetImage == null)
        {
            isFading = false;
            yield break;
        }

        isFading = true;
        targetImage.gameObject.SetActive(true);

        Color color = targetImage.color;
        color.a = startAlpha;
        targetImage.color = color;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            targetImage.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        targetImage.color = color;

        if (Mathf.Approximately(targetAlpha, 0f))
            targetImage.gameObject.SetActive(false);

        isFading = false;
        currentFadeCoroutine = null;
    }
}