using UnityEngine;
using UnityEngine.UI;

public class BadEndingUIManager : BaseEndingUIManager<BadEndingUIManager>
{
    protected override void AutoBindUI()
    {
        if (endingPanel == null)
        {
            Transform panel = transform.Find("BadEndingPanel");
            if (panel != null) 
                endingPanel = panel.gameObject;
        }

        if (endingTextUI == null && endingPanel != null)
            endingTextUI = endingPanel.GetComponentInChildren<Text>();
    }

    protected override void OnMonologueFinished()
    {
        if (SoundManager.instance != null)
            SoundManager.instance.StopAllSound();

        if(FadeManager.instance != null)
        {
            FadeManager.instance.SetBlackBackGround(true);
            FadeManager.instance.FadeIn(3.0f);
        }
    }
}