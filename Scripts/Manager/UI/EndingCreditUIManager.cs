using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndingCreditUIManager : MonoBehaviour
{
    public static EndingCreditUIManager instance;

    [Header("UI 설정")]
    [SerializeField] private GameObject blackBackgroundPanel;
    [SerializeField] private Text roleText;
    [SerializeField] private Text nameText;
    [SerializeField] private Button skipButton;

    [Header("텍스트 설정")]
    [SerializeField] private List<string> roleTextList;
    [SerializeField, TextArea()] private List<string> nameTextList;

    [Header("연출 설정")]
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float displayDuration = 4.0f;
    [SerializeField] private Coroutine creditCoroutine;
    [SerializeField] private bool isSkipped = false;

    private void Awake()
    {
        if(instance == null)
            instance = this;

        else
        {
            Destroy(gameObject);
            return;
        }

        AutoBindUI();
    }
    private void Start()
    {   
        SetTextAlpha(0f);
        if (roleTextList.Count > 0 && nameTextList.Count > 0)
            creditCoroutine = StartCoroutine(CreditSequenceCoroutine());
    }

    private void AutoBindUI() // UI 자동화 함수
    {
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        foreach (Transform t in allChildren)
        {
            if (t.name == "BlackBackgroundPanel")
            {
                blackBackgroundPanel = t.gameObject;
                break;
            }
        }

        Text[] texts = GetComponentsInChildren<Text>(true);
        foreach (Text t in texts)
        {
            if (t.gameObject.name == "RoleText") 
                roleText = t;
            else if (t.gameObject.name == "NameText") 
                nameText = t;
        }

        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button b in buttons)
        {
            if (b.gameObject.name == "SkipButton")
            {
                skipButton = b;
                skipButton.interactable = true;
                skipButton.onClick.RemoveAllListeners();
                skipButton.onClick.AddListener(OnClickSkipButton);
            }
        }
    }

    private IEnumerator CreditSequenceCoroutine() // 크레딧 연출을 수행하는 코루틴
    {
        if(FadeManager.instance != null)
        {
            FadeManager.instance.SetAllBackground(false);
            FadeManager.instance.SetBlackBackGround(true);
            FadeManager.instance.FadeOut(5.0f);
            yield return new WaitUntil(() => !FadeManager.instance.isFading);
        }

        if (blackBackgroundPanel != null)
            blackBackgroundPanel.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        int count = Mathf.Min(roleTextList.Count, nameTextList.Count);

        for (int i = 0; i < count; i++)
        {
            if (isSkipped) 
                yield break;

            roleText.text = roleTextList[i];
            nameText.text = nameTextList[i];
            yield return StartCoroutine(FadeTextAlpha(0f, 1f, fadeDuration));
            yield return new WaitForSeconds(displayDuration);
            yield return StartCoroutine(FadeTextAlpha(1f, 0f, fadeDuration));
            yield return new WaitForSeconds(0.5f);
        }

        if (!isSkipped) 
            FinishCredits();
    }

    private IEnumerator FadeTextAlpha(float startAlpha, float targetAlpha, float duration) // 글자 투명도를 조절하는 코루틴
    {
        float elapsed = 0f;
        Color roleColor = roleText.color;
        Color nameColor = nameText.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            roleColor.a = alpha;
            nameColor.a = alpha;
            roleText.color = roleColor;
            nameText.color = nameColor;
            yield return null;
        }

        roleColor.a = targetAlpha;
        nameColor.a = targetAlpha;
        roleText.color = roleColor;
        nameText.color = nameColor;
    }

    private void SetTextAlpha(float alpha) // 텍스트 알파 값을 설정하는 함수
    {
        if (roleText != null) 
        { 
            Color c = roleText.color; 
            c.a = alpha; 
            roleText.color = c; 
        }

        if (nameText != null) 
        { 
            Color c = nameText.color; 
            c.a = alpha; 
            nameText.color = c; 
        }
    }

    public void OnClickSkipButton() // 스킵 버튼 클릭 시 실행되는 함수
    {
        if (isSkipped) 
            return;
        
        isSkipped = true;
        if (SoundManager.instance != null)
        {
            SoundManager.instance.StopAllSound();
            SoundManager.instance.PlayButtonSound();
        }

        skipButton.interactable = false;

        if (creditCoroutine != null) 
            StopCoroutine(creditCoroutine);

        FinishCredits();
    }

    private void FinishCredits() // 크레딧 연출을 끝내는 함수
    {
        EndingCreditManager manager = FindAnyObjectByType<EndingCreditManager>();
        if (manager != null) 
            manager.GoToMainMenu();
    }
}
