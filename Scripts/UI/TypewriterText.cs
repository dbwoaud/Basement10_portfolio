using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TypewriterText : MonoBehaviour
{
    [SerializeField] private Text target;

    [Header("텍스트 연출 속도")]
    [SerializeField] private float charInterval = 0.05f;
    [SerializeField] private float newlineInterval = 0.2f;

    [Header("텍스트 연출 유지 시간")]
    [SerializeField] private float holdDuration = 2.0f;

    private Coroutine routine;

    private readonly StringBuilder builder = new StringBuilder(256);
    public bool IsTyping => routine != null;


    private void Awake()
    {
        if (target == null)
            target = GetComponent<Text>();
    }

    public Coroutine Play(string content, Action onComplete = null) // 텍스트 연출을 수행하는 코루틴
    {
        gameObject.SetActive(true);
        Stop();
        routine = StartCoroutine(PlayRoutine(content, clearAfterHold: true, onComplete));
        return routine;
    }

    public Coroutine PlayAndKeep(string content, Action onComplete = null) // 지워지지 않는 텍스트 연출을 수행하는 코루틴
    {
        gameObject.SetActive(true);
        Stop();
        routine = StartCoroutine(PlayRoutine(content, clearAfterHold: false, onComplete));
        return routine;
    }

    public void Stop() // 현재 코루틴을 중지시키는 함수
    {
        if (routine == null)
            return;

        StopCoroutine(routine);
        routine = null;
    }

    public void Clear() // 현재 텍스트를 초기화하는 함수
    {
        Stop();

        if (target != null)
            target.text = string.Empty;
    }

    public void SkipToEnd(string content) // 남은 텍스트를 즉시 출력하는 함수
    {
        Stop();

        if (target != null)
            target.text = content;
    }

    private IEnumerator PlayRoutine(string content, bool clearAfterHold, Action onComplete) // 텍스트 연출을 수행하는 코루틴
    {
        if (target == null || string.IsNullOrEmpty(content))
        {
            routine = null;
            onComplete?.Invoke();
            yield break;
        }

        target.gameObject.SetActive(true);
        target.text = string.Empty;

        foreach (char letter in content)
        {
            builder.Append(letter);
            target.text = builder.ToString();
            yield return new WaitForSecondsRealtime(letter == '\n' ? newlineInterval : charInterval);
        }

        if (holdDuration > 0f)
            yield return new WaitForSecondsRealtime(holdDuration);

        if (clearAfterHold)
        {
            builder.Clear();
            target.text = string.Empty;
            target.gameObject.SetActive(false);
        }

        routine = null;
        onComplete?.Invoke();
    }
}