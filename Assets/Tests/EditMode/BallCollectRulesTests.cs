using NUnit.Framework;

namespace BallCollect.Tests
{
    public class BallCollectRulesTests
    {
        [Test]
        public void SameItemTwice_CountsOnce()
        {
            var rules = new BallCollectRules(5);

            Assert.IsTrue(rules.TryCollect(0));
            Assert.IsFalse(rules.TryCollect(0));
            Assert.AreEqual(1, rules.CollectedCount);
        }

        [Test]
        public void CollectAllFive_Clears()
        {
            var rules = new BallCollectRules(5);

            for (int i = 0; i < 4; i++) rules.TryCollect(i);
            Assert.IsFalse(rules.IsCleared, "4個ではクリアしない");

            rules.TryCollect(4);
            Assert.IsTrue(rules.IsCleared);
        }

        [Test]
        public void Reset_ReturnsToZeroAndNotCleared()
        {
            var rules = new BallCollectRules(5);
            for (int i = 0; i < 5; i++) rules.TryCollect(i);
            rules.Tick(31f);

            rules.Reset();

            Assert.AreEqual(0, rules.CollectedCount);
            Assert.IsFalse(rules.IsCleared);
            Assert.IsFalse(rules.IsTimeUp);
            Assert.AreEqual(30f, rules.RemainingTime);
        }

        [Test]
        public void TimeRunsOut_TimeUpAndNoMoreCollect()
        {
            var rules = new BallCollectRules(5);
            rules.TryCollect(0);

            rules.Tick(30f);

            Assert.IsTrue(rules.IsTimeUp);
            Assert.AreEqual(0f, rules.RemainingTime);
            Assert.IsFalse(rules.TryCollect(1), "TIME UP 後は回収しない");
        }

        [Test]
        public void FifthCollectAndTimeOutInSameUpdate_ClearWins()
        {
            var rules = new BallCollectRules(5, 30f);
            for (int i = 0; i < 4; i++) rules.TryCollect(i);
            rules.Tick(29.99f);

            // 同じ更新内: 回収 → 時間経過 の順に処理する
            rules.TryCollect(4);
            rules.Tick(0.05f);

            Assert.IsTrue(rules.IsCleared);
            Assert.IsFalse(rules.IsTimeUp, "CLEAR後に時間切れへ変わらない");
            Assert.Greater(rules.RemainingTime, 0f, "CLEARで時計が止まる");
        }
    }
}
