using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseEndingUIManager<T> : BaseUIManager<T> where T : BaseEndingUIManager<T>
{
    [Header("엔딩 공통 UI")]
    [SerializeField] protected GameObject endingPanel;
    [SerializeField] protected TypewriterText typewriter;

    [Tooltip("출력할 독백의 Story 테이블 키를 순서대로 넣는다.")]
    [SerializeField] protected string[] monologueKeys;

    [SerializeField] private float lineGapDuration = 1.0f;

    protected abstract string EndingPanelName { get; }

    protected override void AutoBindUI()
    {
        if (endingPanel == null)
            endingPanel = UIBinder.FindObject(transform, EndingPanelName);

        if (typewriter != null || endingPanel == null)
            return;

        Text text = endingPanel.GetComponentInChildren<Text>(true);

        if (text == null)
            return;

        typewriter = text.GetComponent<TypewriterText>();

        if (typewriter == null)
            typewriter = text.gameObject.AddComponent<TypewriterText>();
    }

    protected override void InitializeUI()
    {
        if (endingPanel != null)
            endingPanel.SetActive(false);

        if (typewriter != null)
            typewriter.Clear();
    }

    public IEnumerator PlayMonologueSequence()
    {
        if (endingPanel != null)
        {
            endingPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (typewriter == null || monologueKeys == null)
            yield break;

        foreach (string key in monologueKeys)
        {
            if (string.IsNullOrEmpty(key))
                continue;

            yield return typewriter.Play(Loc.Story(key));
            yield return new WaitForSecondsRealtime(lineGapDuration);
        }

        OnMonologueFinished();
    }

    protected abstract void OnMonologueFinished();
}