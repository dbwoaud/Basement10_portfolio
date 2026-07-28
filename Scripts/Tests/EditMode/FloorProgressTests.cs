using System;
using NUnit.Framework;

public class FloorProgressTests
{
    [Test]
    public void 생성_직후_현재_층은_시작_층이고_실패_회귀_플래그는_거짓이다()
    {
        FloorProgress progress = new FloorProgress(10, 0);

        Assert.AreEqual(10, progress.CurrentFloor, "생성 직후 현재 층은 설정한 시작 층(10층)이어야 합니다.");
        Assert.IsFalse(progress.IsReturningFromFailure, "생성 직후 실패 회귀 플래그(IsReturningFromFailure)는 false여야 합니다.");
        Assert.IsFalse(progress.IsCleared, "생성 직후 클리어 상태(IsCleared)는 false여야 합니다.");
    }

    [Test]
    public void 시작_층이_목표_층보다_작으면_ArgumentException이_발생한다()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            new FloorProgress(0, 10);
        }, "시작 층이 목표 층보다 작으면 생성 단계에서 ArgumentException이 일어나야 합니다.");
    }

    [Test]
    public void 정답을_10번_연속_제출하면_목표_층에_도달하고_클리어_상태가_된다()
    {
        FloorProgress progress = new FloorProgress(10, 0);

        // 10층에서 0층까지 가려면 10번 맞추어야 함 (각 제출 시 이상현상 없는 곳에서 Exit 정답 제출)
        for (int i = 0; i < 10; i++)
        {
            Assert.IsFalse(progress.IsCleared, $"진행 중인 {10 - i}층에서는 클리어 상태가 아니어야 합니다.");
            bool submitted = progress.Submit(TriggerType.Exit, false); // 정답
            Assert.IsTrue(submitted, $"{10 - i}층에서 정답 제출이 정상 처리되어야 합니다.");
        }

        Assert.AreEqual(0, progress.CurrentFloor, "10번 정답을 맞춘 후 현재 층은 목표 층(0층)이어야 합니다.");
        Assert.IsTrue(progress.IsCleared, "목표 층에 도달했으므로 IsCleared는 true여야 합니다.");
    }

    [Test]
    public void 진행_중_오답을_제출하면_시작_층으로_돌아가고_실패_회귀_플래그가_참이_된다()
    {
        FloorProgress progress = new FloorProgress(10, 0);

        // 9층까지 정답으로 가기
        progress.Submit(TriggerType.Exit, false); // 10 -> 9

        // 9층에서 오답 제출 (이상현상 없는데 Return 선택)
        bool submitted = progress.Submit(TriggerType.Return, false); // 오답
        Assert.IsFalse(submitted, "오답 제출 시 Submit 결과는 false여야 합니다.");

        Assert.AreEqual(10, progress.CurrentFloor, "오답을 내면 즉시 시작 층(10층)으로 강제 리셋되어야 합니다.");
        Assert.IsTrue(progress.IsReturningFromFailure, "오답으로 복귀했으므로 IsReturningFromFailure 플래그가 true여야 합니다.");
    }

    [Test]
    public void 실패_회귀_플래그_소비_메서드는_첫_호출에_참_두_번째_호출에_거짓을_반환한다()
    {
        FloorProgress progress = new FloorProgress(10, 0);

        // 오답을 내어 IsReturningFromFailure를 true로 만듬
        progress.Submit(TriggerType.Return, false);
        Assert.IsTrue(progress.IsReturningFromFailure, "오답 후 IsReturningFromFailure는 true인 상태입니다.");

        // ConsumeReturningFlag() 첫 호출
        bool firstCall = progress.ConsumeReturningFlag();
        Assert.IsTrue(firstCall, "실패 후 첫 ConsumeReturningFlag() 호출은 true를 반환해야 합니다.");
        Assert.IsFalse(progress.IsReturningFromFailure, "ConsumeReturningFlag() 호출 이후 IsReturningFromFailure는 false로 재설정되어야 합니다.");

        // ConsumeReturningFlag() 두 번째 호출
        bool secondCall = progress.ConsumeReturningFlag();
        Assert.IsFalse(secondCall, "이미 실패 플래그가 소비되었으므로 두 번째 ConsumeReturningFlag() 호출은 false여야 합니다.");
    }

    [Test]
    public void 방문_표시는_각_층마다_최초_한_번만_성공하고_이미_방문했거나_범위를_벗어나면_실패한다()
    {
        FloorProgress progress = new FloorProgress(10, 0);

        // 10층 최초 방문
        bool firstMark = progress.TryMarkVisited();
        Assert.IsTrue(firstMark, "10층 최초 방문 마크는 성공(true)해야 합니다.");

        // 10층 중복 방문 시도
        bool secondMark = progress.TryMarkVisited();
        Assert.IsFalse(secondMark, "이미 방문한 10층에 대해 TryMarkVisited()를 다시 호출하면 실패(false)해야 합니다.");

        // 9층으로 이동 후 방문
        progress.Submit(TriggerType.Exit, false); // 10 -> 9
        bool targetMark = progress.TryMarkVisited();
        Assert.IsTrue(targetMark, "새롭게 도달한 9층의 최초 방문 마크는 성공(true)해야 합니다.");
    }

    [Test]
    public void 오답으로_시작_층에_돌아왔을_때_방문_마크_시도는_실패해야_한다()
    {
        FloorProgress progress = new FloorProgress(10, 0);

        // 10층 최초 방문 마크 (독백 출력 등)
        progress.TryMarkVisited();

        // 9층으로 이동
        progress.Submit(TriggerType.Exit, false);

        // 9층에서 오답을 선택하여 10층으로 리셋
        progress.Submit(TriggerType.Return, false);
        Assert.AreEqual(10, progress.CurrentFloor, "오답 제출 후 10층으로 돌아와야 합니다.");

        // 이미 10층은 이전에 방문했으므로, 리셋 후의 방문 마크 시도는 false여야 함 (독백 재출력 방지용)
        bool markAfterFailure = progress.TryMarkVisited();
        Assert.IsFalse(markAfterFailure, "오답으로 인해 시작 층에 되돌아왔을 때는 이미 방문했던 기록이 유지되어 TryMarkVisited()가 false여야 합니다.");
    }

    [Test]
    public void 리셋_메서드_호출_시_층과_실패_회귀_플래그_및_방문_기록이_모두_초기화된다()
    {
        FloorProgress progress = new FloorProgress(10, 0);

        // 방문 흔적 남기기, 오답 발생시키기 등 복잡한 상태 만들기
        progress.TryMarkVisited(); // 10층 방문
        progress.Submit(TriggerType.Exit, false); // 9층 이동
        progress.TryMarkVisited(); // 9층 방문
        progress.Submit(TriggerType.Return, false); // 오답 제출로 10층 복귀 (IsReturningFromFailure = true)

        Assert.IsTrue(progress.IsReturningFromFailure, "리셋 전에는 실패 회귀 플래그가 true입니다.");
        Assert.IsTrue(progress.HasVisited(10), "리셋 전에는 10층 방문 기록이 있어야 합니다.");
        Assert.IsTrue(progress.HasVisited(9), "리셋 전에는 9층 방문 기록이 있어야 합니다.");

        // 리셋 실행
        progress.Reset();

        // 초기화 검증
        Assert.AreEqual(10, progress.CurrentFloor, "리셋 후 현재 층은 시작 층(10층)이어야 합니다.");
        Assert.IsFalse(progress.IsReturningFromFailure, "리셋 후 실패 회귀 플래그는 false여야 합니다.");
        Assert.IsFalse(progress.HasVisited(10), "리셋 후에는 10층 방문 기록이 리셋되어 없어야 합니다.");
        Assert.IsFalse(progress.HasVisited(9), "리셋 후에는 9층 방문 기록이 리셋되어 없어야 합니다.");

        // 리셋 후에 다시 10층에 방문 마크 시도 시 성공해야 함
        bool markAfterReset = progress.TryMarkVisited();
        Assert.IsTrue(markAfterReset, "리셋 후에는 10층 방문 기록이 사라져 TryMarkVisited()가 성공해야 합니다.");
    }
}