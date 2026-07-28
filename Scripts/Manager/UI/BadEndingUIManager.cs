public class BadEndingUIManager : BaseEndingUIManager<BadEndingUIManager>
{
    protected override string EndingPanelName => "BadEndingPanel";


    protected override void OnMonologueFinished() // 독백 연출 완료 시 실행되는 함수
    {
        if (SoundManager.HasInstance)
            SoundManager.Instance.StopAllSound();

        if (!FadeManager.HasInstance)
            return;

        FadeManager.Instance.SetBlackBackGround(true);
        FadeManager.Instance.FadeIn(3.0f);
    }
}