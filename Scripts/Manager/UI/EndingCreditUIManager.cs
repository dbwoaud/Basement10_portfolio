using System;
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

    [Header("ũ���� ����")]
    [SerializeField]
    private string[] roleKeys =
    {
        "credit.role.0", "credit.role.1", "credit.role.2", "credit.role.3", "credit.role.4",
        "credit.role.5", "credit.role.6", "credit.role.7", "credit.role.8",
    };

    [SerializeField]
    private string[] nameKeys =
    {
        "credit.name.0", "credit.name.1", "credit.name.2", "credit.name.3", "credit.name.4",
        "credit.name.5", "credit.name.6", "credit.name.7", "credit.name.8",
    };

    [Header("����")]
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
        if (!ValidateKeys())
            return;

        creditRoutine = StartCoroutine(CreditSequenceRoutine());
    }

    private bool ValidateKeys()
    {
        if (roleKeys == null || roleKeys.Length == 0)
        {
            Debug.LogError("[ũ����] roleKeys�� ��� �ֽ��ϴ�.", this);
            return false;
        }

        if (nameKeys == null || nameKeys.Length != roleKeys.Length)
        {
            Debug.LogError(
                $"[ũ����] roleKeys({roleKeys.Length})�� nameKeys({nameKeys?.Length ?? 0})�� " +
                "������ �ٸ��ϴ�.", this);
            return false;
        }

        return true;
    }

    private IEnumerator CreditSequenceRoutine()
    {
        if (FadeManager.HasInstance)
        {
            FadeManager.Instance.SetAllBackground(false);
            FadeManager.Instance.SetBlackBackGround(true);
            FadeManager.Instance.FadeOut(5.0f);
            yield return new WaitUntil(() => !FadeManager.Instance.isFading);
        }

        if (blackBackgroundPanel != null)
            blackBackgroundPanel.SetActive(true);

        yield return new WaitForSeconds(0.5f);


        for (int i = 0; i < roleKeys.Length; i++)
        {
            if (isSkipped)
                yield break;

            if (roleText != null)
                roleText.text = Loc.Story(roleKeys[i]);

            if (nameText != null)
                nameText.text = Loc.Story(nameKeys[i]);

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
            SoundManager.Instance.StopAllSound();

        PlayButtonSound();

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