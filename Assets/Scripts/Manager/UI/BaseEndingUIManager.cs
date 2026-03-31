using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseEndingUIManager<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T instance;

    [Header("엔딩 공통 UI")]
    protected GameObject endingPanel;
    protected Text endingTextUI;
    [SerializeField, TextArea(3, 10)] protected List<string> monologueList;
    [SerializeField] private float monologueDisplayDuration = 4.0f;

    protected virtual void Awake()
    {
        if (instance == null) 
            instance = this as T;
        else 
        { 
            Destroy(gameObject); 
            return; 
        }

        AutoBindUI();
    }

    protected virtual void Start()
    {
        if (endingPanel != null) 
            endingPanel.SetActive(false);
        if (endingTextUI != null) 
            endingTextUI.text = "";
    }

    protected abstract void AutoBindUI(); // UI 자동화 함수

    public IEnumerator PlayMonologueSequence() // 독백을 출력하는 코루틴
    {
        if (endingPanel != null)
        {
            endingPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (endingTextUI == null) 
            yield break;

        foreach (string line in monologueList)
        {
            yield return StartCoroutine(TypeTextCoroutine(line));
            yield return new WaitForSecondsRealtime(monologueDisplayDuration);
            endingTextUI.text = "";
            yield return new WaitForSecondsRealtime(1.0f);
        }

        OnMonologueFinished();
    }

    private IEnumerator TypeTextCoroutine(string content, float speed = 0.05f) // 텍스트를 타이핑하는 코루틴
    {
        endingTextUI.text = "";
        foreach (char letter in content.ToCharArray())
        {
            endingTextUI.text += letter;
            yield return new WaitForSecondsRealtime(speed);
        }
    }

    protected abstract void OnMonologueFinished(); // 독백을 종료시키는 함수
}