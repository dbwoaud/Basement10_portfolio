using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class EndingCreditUIManager : BaseUIManager<EndingCreditUIManager>
{
    [Header("UI")]
    [SerializeField] private GameObject blackBackgroundPanel;
    [SerializeField] private Text roleText;
    [SerializeField] private Text nameText;
    [SerializeField] private Button skipButton;

    [Header("크레딧 내용")]
    [Tooltip("역할 이름의 Story 테이블 키. 예: credit.role.programming")]
    [SerializeField] private string[] roleKeys;

    [Tooltip("표시할 이름. 고유명사이므로 번역하지 않는다.")]
    [SerializeField, TextArea] private string[] names;

    [Header("연출")]
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float displayDuration = 4.0f;

    private Coroutine creditRoutine;
    private bool isSkipped;

    protected override void AutoBindUI()
    {
        if (blackBackgroundPanel == null)
            blackBackgroundPanel = UIBinder.FindObject(transform, "BlackBackgroundPanel");

        if (roleText == null)
            roleText = UIBinder.Find<Text>(transform, "RoleText");

        if (nameText == null)
            nameText = UIBinder.Find<Text>(transform, "NameText");

        if (skipButton == null)
            skipButton = UIBinder.Find<Button>(transform, "SkipButton");

        UIBinder.BindButtons(transform, new Dictionary<string, UnityAction>
        {
            { "SkipButton", OnClickSkipButton },
        });

        if (skipButton != null)
            skipButton.interactable = true;
    }

    protected override void InitializeUI()
    {
        SetTextAlpha(0f);
    }

    private void Start()
    {
        if (roleKeys != null && roleKeys.Length > 0 && names != null && names.Length > 0)
            creditRoutine = StartCoroutine(CreditSequenceRoutine());
    }

    private IEnumerator CreditSequenceRoutine()
    {
        if (FadeManager.HasInstance)
        {
            FadeManager.instance.SetAllBackground(false);
            FadeManager.instance.SetBlackBackGround(true);
            FadeManager.instance.FadeOut(5.0f);
            yield return new WaitUntil(() => !FadeManager.instance.isFading);
        }

        if (blackBackgroundPanel != null)
            blackBackgroundPanel.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        int count = Mathf.Min(roleKeys.Length, names.Length);

        for (int i = 0; i < count; i++)
        {
            if (isSkipped)
                yield break;

            if (roleText != null)
                roleText.text = Loc.Story(roleKeys[i]);

            if (nameText != null)
                nameText.text = names[i];

            yield return StartCoroutine(FadeTextAlpha(0f, 1f, fadeDuration));
            yield return new WaitForSeconds(displayDuration);
            yield return StartCoroutine(FadeTextAlpha(1f, 0f, fadeDuration));
            yield return new WaitForSeconds(0.5f);
        }

        if (!isSkipped)
            FinishCredits();
    }

    private IEnumerator FadeTextAlpha(float startAlpha, float targetAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetTextAlpha(Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration));
            yield return null;
        }

        SetTextAlpha(targetAlpha);
    }

    private void SetTextAlpha(float alpha)
    {
        ApplyAlpha(roleText, alpha);
        ApplyAlpha(nameText, alpha);
    }

    private static void ApplyAlpha(Text text, float alpha)
    {
        if (text == null)
            return;

        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }

    public void OnClickSkipButton()
    {
        if (isSkipped)
            return;

        isSkipped = true;

        if (SoundManager.HasInstance)
        {
            SoundManager.instance.StopAllSound();
            SoundManager.instance.PlayButtonSound();
        }

        if (skipButton != null)
            skipButton.interactable = false;

        if (creditRoutine != null)
            StopCoroutine(creditRoutine);

        FinishCredits();
    }

    private void FinishCredits()
    {
        EndingCreditManager manager = FindAnyObjectByType<EndingCreditManager>();

        if (manager != null)
            manager.GoToMainMenu();
    }
}