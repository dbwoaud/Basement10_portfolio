using NUnit.Framework;

public class FloorRuleTests
{
    [Test]
    public void 이상현상이_없을_때_나가면_정답이다()
    {
        bool result = FloorRule.IsCorrect(TriggerType.Exit, false);
        Assert.IsTrue(result, "이상현상이 없을 때 Exit(나가기)을 선택하면 정답이어야 합니다.");
    }

    [Test]
    public void 이상현상이_없을_때_되돌아가면_오답이다()
    {
        bool result = FloorRule.IsCorrect(TriggerType.Return, false);
        Assert.IsFalse(result, "이상현상이 없을 때 Return(되돌아가기)을 선택하면 오답이어야 합니다.");
    }

    [Test]
    public void 이상현상이_있을_때_되돌아가면_정답이다()
    {
        bool result = FloorRule.IsCorrect(TriggerType.Return, true);
        Assert.IsTrue(result, "이상현상이 있을 때 Return(되돌아가기)을 선택하면 정답이어야 합니다.");
    }

    [Test]
    public void 이상현상이_있을_때_나가면_오답이다()
    {
        bool result = FloorRule.IsCorrect(TriggerType.Exit, true);
        Assert.IsFalse(result, "이상현상이 있을 때 Exit(나가기)을 선택하면 오답이어야 합니다.");
    }

    [Test]
    public void 정답일_때_한_층_내려간다()
    {
        // 10 -> 9
        int nextFloor1 = FloorRule.DecideNextMap(10, 10, true);
        Assert.AreEqual(9, nextFloor1, "시작 층(10층)에서 정답을 맞추면 9층으로 내려가야 합니다.");

        // 1 -> 0
        int nextFloor2 = FloorRule.DecideNextMap(1, 10, true);
        Assert.AreEqual(0, nextFloor2, "1층에서 정답을 맞추면 목표 층(0층)으로 내려가야 합니다.");
    }

    [Test]
    public void 오답일_때_시작_층으로_복귀한다()
    {
        // 3 -> 10
        int nextFloor = FloorRule.DecideNextMap(3, 10, false);
        Assert.AreEqual(10, nextFloor, "중간 층(3층)에서 오답을 선택하면 시작 층(10층)으로 리셋되어야 합니다.");
    }

    [Test]
    public void 시작_층에서는_일반_맵이고_이상현상이_없다()
    {
        var plan = FloorRule.ChoiceMap(10, 10, 0, false);
        Assert.IsFalse(plan.UseFinalMap, "시작 층(10층)에서는 최종 맵이 아닌 일반 맵을 사용해야 합니다.");
        Assert.IsFalse(plan.AllowAbnormal, "시작 층(10층)에서는 첫 진입이므로 이상현상이 발생하지 않아야 합니다.");
    }

    [Test]
    public void 중간_층에서는_일반_맵이고_이상현상이_추첨된다()
    {
        var plan = FloorRule.ChoiceMap(5, 10, 0, false);
        Assert.IsFalse(plan.UseFinalMap, "중간 층(5층)에서는 아직 최종 맵을 사용하지 않고 일반 맵을 사용해야 합니다.");
        Assert.IsTrue(plan.AllowAbnormal, "중간 층(5층)에서는 이상현상이 출현 가능해야(AllowAbnormal = true) 합니다.");
    }

    [Test]
    public void 목표_층에서는_최종_맵이고_이상현상이_없다()
    {
        var plan = FloorRule.ChoiceMap(0, 10, 0, false);
        Assert.IsTrue(plan.UseFinalMap, "목표 층(0층)에서는 최종 맵(UseFinalMap = true)을 사용해야 합니다.");
        Assert.IsFalse(plan.AllowAbnormal, "목표 층(0층)에서는 이상현상이 발생하지 않아야 합니다.");
    }

    [Test]
    public void 엔딩_씬이면_층과_무관하게_일반_맵이고_이상현상이_없다()
    {
        // 엔딩 씬인 경우 (isEndingScene = true)
        var planEndingAtStart = FloorRule.ChoiceMap(10, 10, 0, true);
        Assert.IsFalse(planEndingAtStart.UseFinalMap, "엔딩 씬에서는 층과 상관없이 최종 맵이 아닌 일반 맵을 사용해야 합니다.");
        Assert.IsFalse(planEndingAtStart.AllowAbnormal, "엔딩 씬에서는 층과 상관없이 이상현상이 발생하지 않아야 합니다.");

        var planEndingAtMid = FloorRule.ChoiceMap(5, 10, 0, true);
        Assert.IsFalse(planEndingAtMid.UseFinalMap, "엔딩 씬에서는 중간 층이라도 최종 맵이 아닌 일반 맵을 사용해야 합니다.");
        Assert.IsFalse(planEndingAtMid.AllowAbnormal, "엔딩 씬에서는 중간 층이라도 이상현상이 발생하지 않아야 합니다.");

        var planEndingAtTarget = FloorRule.ChoiceMap(0, 10, 0, true);
        Assert.IsFalse(planEndingAtTarget.UseFinalMap, "엔딩 씬에서는 목표 층에 도달했더라도 최종 맵이 아닌 일반 맵을 사용해야 합니다.");
        Assert.IsFalse(planEndingAtTarget.AllowAbnormal, "엔딩 씬에서는 목표 층에 도달했더라도 이상현상이 발생하지 않아야 합니다.");
    }
}