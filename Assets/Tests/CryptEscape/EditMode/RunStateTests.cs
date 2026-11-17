using NUnit.Framework;
using CryptEscape.Core;

namespace CryptEscape.Tests
{
    public class RunStateTests
    {
        [Test]
        public void SameCoinTwice_CountsOnce()
        {
            var run = new RunState(startLives: 3);

            Assert.IsTrue(run.TryCollectCoin(2));
            Assert.IsFalse(run.TryCollectCoin(2));
            Assert.AreEqual(1, run.CoinCount);
        }

        [Test]
        public void FifthCoin_OpensBossDoor()
        {
            var run = new RunState(3);
            for (int i = 0; i < 4; i++) run.TryCollectCoin(i);
            Assert.IsFalse(run.BossDoorOpen, "4枚では開かない");

            run.TryCollectCoin(4);
            Assert.IsTrue(run.BossDoorOpen);
        }

        [Test]
        public void Exit_OpensOnlyAfterBossDefeated()
        {
            var run = new RunState(3);
            Assert.IsFalse(run.ExitOpen);

            run.DefeatBoss();
            Assert.IsTrue(run.BossDefeated);
            Assert.IsTrue(run.ExitOpen);
        }

        [Test]
        public void FullReset_ClearsEverything()
        {
            var run = new RunState(3);
            for (int i = 0; i < 5; i++) run.TryCollectCoin(i);
            run.DefeatBoss();
            run.MarkEnemyDefeated(3);
            run.MarkPotionUsed(1);
            run.Lives = 1;
            run.RespawnPointId = 2;

            run.FullReset();

            Assert.AreEqual(0, run.CoinCount);
            Assert.IsFalse(run.BossDoorOpen);
            Assert.IsFalse(run.BossDefeated);
            Assert.IsFalse(run.ExitOpen);
            Assert.IsFalse(run.IsEnemyDefeated(3));
            Assert.IsFalse(run.IsPotionUsed(1));
            Assert.AreEqual(3, run.Lives);
            Assert.AreEqual(RunState.StartPointId, run.RespawnPointId);
        }
    }
}
