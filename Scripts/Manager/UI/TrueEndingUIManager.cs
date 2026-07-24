public class TrueEndingUIManager : BaseEndingUIManager<TrueEndingUIManager>
{
    protected override string EndingPanelName => "TrueEndingPanel";

    protected override void OnMonologueFinished()
    {
        if (SoundManager.HasInstance)
            SoundManager.Instance.StopAllSound();

        if (!FadeManager.HasInstance)
            return;

        FadeManager.Instance.SetBlackBackGround(true);
        FadeManager.Instance.FadeIn(3.0f);
    }
}