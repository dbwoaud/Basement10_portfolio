using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EndingCreditUIManager : BaseUIManager<EndingCreditUIManager>
{
    [Header("UI 요소")]
    [SerializeField] private GameObject blackBackgroundPanel;
    [SerializeField] private Text roleText;
    [SerializeField] private Text nameText;
    [SerializeField] private Button skipButton;

    [Header("텍스트 설정")]
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

    [Header("화면 연출 설정")]
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float displayDuration = 4.0f;
    [SerializeField] private float lineGapDuration = 0.5f;
    private WaitForSeconds displayWait;
    private WaitForSeconds lineGapWait;

    private Coroutine creditRoutine;
    private bool isSkipped;


    protected override void AutoBindUI() // UI 자동화 함수
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

    protected override void InitializeUI() // UI 초기화 함수
    {
        SetTextAlpha(0f);
        displayWait = new WaitForSeconds(displayDuration);
        lineGapWait = new WaitForSeconds(lineGapDuration);
    }

    private void Start()
    {
        if (!CheckKeys())
            return;

        creditRoutine = StartCoroutine(PlayCreditSequenceRoutine());
    }

    private bool CheckKeys() // 키의 유효성을 확인하는 함수
    {
        if (roleKeys == null || roleKeys.Length == 0)
            return false;

        if (nameKeys == null || nameKeys.Length != roleKeys.Length)
            return false;

        return true;
    }

    private IEnumerator PlayCreditSequenceRoutine() // 엔딩 크레딧 연출을 수행하는 함수
    {
        yield return Loc.EnsureReady();

        if (FadeManager.HasInstance)
        {
            FadeManager.Instance.SetAllBackground(false);
            FadeManager.Instance.SetBlackBackGround(true);
            FadeManager.Instance.FadeOut(5.0f);
            yield return new WaitUntil(() => !FadeManager.Instance.isFading);
        }

        if (blackBackgroundPanel != null)
            blackBackgroundPanel.SetActive(true);

        yield return lineGapWait;

        for (int i = 0; i < roleKeys.Length; i++)
        {
            if (isSkipped)
                yield break;

            if (roleText != null)
                roleText.text = Loc.Story(roleKeys[i]);

            if (nameText != null)
                nameText.text = Loc.Story(nameKeys[i]);

            yield return StartCoroutine(FadeTextAlpha(0f, 1f, fadeDuration));
            yield return displayWait;
            yield return StartCoroutine(FadeTextAlpha(1f, 0f, fadeDuration));
            yield return lineGapWait;
        }

        if (!isSkipped)
            FinishCredits();
    }

    private IEnumerator FadeTextAlpha(float startAlpha, float targetAlpha, float duration) // 텍스트 페이드 연출을 수행하는 함수
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

    private void SetTextAlpha(float alpha) // 엔딩 크레딧 내 모든 텍스트의 투명도를 설정하는 함수
    {
        ApplyAlpha(roleText, alpha);
        ApplyAlpha(nameText, alpha);
    }

    private static void ApplyAlpha(Text text, float alpha) // 텍스트에 투명도를 설정하는 함수
    {
        if (text == null)
            return;

        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }

    public void OnClickSkipButton() // 스킵 버튼 클릭 시 실행되는 함수
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

    private void FinishCredits() // 크레딧 연출을 종료하는 함수
    {
        EndingCreditManager manager = FindAnyObjectByType<EndingCreditManager>();
        if (manager != null)
            manager.GoToMainMenu();
    }
}