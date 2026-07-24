using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 텍스트를 한 글자씩 출력하는 연출.
///
/// 기존에는 BaseEndingUIManager.TypeTextCoroutine과
/// StoryModeUIManager.TypeMonologue가 거의 같은 코드로 존재했다.
/// 후자만 개행 문자에서 더 오래 쉬는 차이가 있었는데, 이 차이는 파라미터로 흡수했다.
///
/// 일시정지(timeScale = 0) 중에도 독백이 진행되어야 하므로
/// WaitForSecondsRealtime을 쓴다(기존 두 구현 모두 동일).
/// </summary>
[RequireComponent(typeof(Text))]
public class TypewriterText : MonoBehaviour
{
    [SerializeField] private Text target;

    [Header("속도")]
    [SerializeField] private float charInterval = 0.05f;
    [Tooltip("개행 문자에서 추가로 쉬는 시간.")]
    [SerializeField] private float newlineInterval = 0.2f;

    [Header("유지 시간")]
    [Tooltip("출력이 끝난 뒤 텍스트를 남겨 두는 시간.")]
    [SerializeField] private float holdDuration = 2.0f;

    private Coroutine routine;

    public bool IsTyping => routine != null;

    private void Awake()
    {
        if (target == null)
            target = GetComponent<Text>();
    }

    /// <summary>한 줄을 출력하고 holdDuration 만큼 유지한 뒤 지운다.</summary>
    public Coroutine Play(string content, Action onComplete = null)
    {
        gameObject.SetActive(true);
        Stop();
        routine = StartCoroutine(PlayRoutine(content, clearAfterHold: true, onComplete));
        return routine;
    }

    /// <summary>출력만 하고 지우지 않는다. 엔딩 독백처럼 호출부가 흐름을 제어할 때 쓴다.</summary>
    public Coroutine PlayAndKeep(string content, Action onComplete = null)
    {
        gameObject.SetActive(true);
        Stop();
        routine = StartCoroutine(PlayRoutine(content, clearAfterHold: false, onComplete));
        return routine;
    }

    public void Stop()
    {
        if (routine == null)
            return;

        StopCoroutine(routine);
        routine = null;
    }

    public void Clear()
    {
        Stop();

        if (target != null)
            target.text = string.Empty;
    }

    /// <summary>남은 글자를 즉시 모두 표시한다.</summary>
    public void SkipToEnd(string content)
    {
        Stop();

        if (target != null)
            target.text = content;
    }

    private IEnumerator PlayRoutine(string content, bool clearAfterHold, Action onComplete)
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
            target.text += letter;
            yield return new WaitForSecondsRealtime(letter == '\n' ? newlineInterval : charInterval);
        }

        if (holdDuration > 0f)
            yield return new WaitForSecondsRealtime(holdDuration);

        if (clearAfterHold)
        {
            target.text = string.Empty;
            target.gameObject.SetActive(false);
        }

        routine = null;
        onComplete?.Invoke();
    }
}