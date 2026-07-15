using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FadeManager : Singleton<FadeManager>
{
    [SerializeField] private Image black;
    [SerializeField] private Image white;
    public bool isFading { get; private set; } = false;

    private Coroutine currentFadeCoroutine;

    private WaitForSeconds waitTime = new WaitForSeconds(0.05f);

    override protected void Awake() 
    {
        base.Awake();
        AutoBindImages();
        SetAllBackground(false);
    }

    private void AutoBindImages() // UI 자동화 함수
    {
        Transform canvasTrans = transform.Find("Canvas");
        if (canvasTrans == null) 
            return;

        if (black == null)
        {
            Transform b = canvasTrans.Find("BlackBackground");
            if (b != null) 
                black = b.GetComponent<Image>();
        }
        if (white == null)
        {
            Transform w = canvasTrans.Find("WhiteBackground");
            if (w != null) 
                white = w.GetComponent<Image>();
        }
    }

    public void SetAllBackground(bool state) // 모든 배경화면을 설정하는 함수
    {
        SetBlackBackGround(state);
        SetWhiteBackGround(state);
    }

    public void SetWhiteBackGround(bool state) // 하얀 배경화면을 설정하는 함수
    {
        if (white != null)
            white.gameObject.SetActive(state);
    }

    public void SetBlackBackGround(bool state) // 검은 배경화면을 설정하는 함수
    {
        if (black != null)
            black.gameObject.SetActive(state);
    }

    public void FadeOut(float duration = 2.0f) => StartFade(black, 1f, 0f, duration); 
    public void FadeIn(float duration = 2.0f) => StartFade(black, 0f, 1f, duration);
    public void FlashOut(float duration = 2.0f) => StartFade(white, 1f, 0f, duration);
    public void FlashIn(float duration = 2.0f) => StartFade(white, 0f, 1f, duration);

    private void StartFade(Image targetImage, float startAlpha, float targetAlpha, float duration) // 페이드 효과를 수행하는 함수
    {
        if (currentFadeCoroutine != null)
            StopCoroutine(currentFadeCoroutine);

        currentFadeCoroutine = StartCoroutine(FadeCoroutine(targetImage, startAlpha, targetAlpha, duration));
    }

    private IEnumerator FadeCoroutine(Image targetImage, float startAlpha, float targetAlpha, float duration) // 페이드 효과 코루틴
    {
        if (targetImage == null)
            yield break;

        isFading = true;
        targetImage.gameObject.SetActive(true);

        Color color = targetImage.color;
        color.a = startAlpha;
        targetImage.color = color;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            targetImage.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        targetImage.color = color;

        if (targetAlpha == 0f)
            targetImage.gameObject.SetActive(false);

        isFading = false;
        currentFadeCoroutine = null;
    }
}