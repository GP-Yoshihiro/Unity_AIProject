using NUnit.Framework;
using CryptEscape.Core;

namespace CryptEscape.Tests
{
    public class LivesAndRespawnTests
    {
        [Test]
        public void NoSave_RespawnsAtStart()
        {
            var run = new RunState(3);
            var lives = new LivesAndRespawn(run);

            Assert.AreEqual(DeathOutcome.Respawn, lives.OnPlayerDied());
            Assert.AreEqual(2, run.Lives);
            Assert.AreEqual(RunState.StartPointId, run.RespawnPointId);
        }

        [Test]
        public void Save_UpdatesRespawnPoint()
        {
            var run = new RunState(3);
            var lives = new LivesAndRespawn(run);

            lives.Save(1);
            lives.OnPlayerDied();

            Assert.AreEqual(1, run.RespawnPointId);
        }

        [Test]
        public void ThirdDeath_IsGameOver()
        {
            var run = new RunState(3);
            var lives = new LivesAndRespawn(run);

            Assert.AreEqual(DeathOutcome.Respawn, lives.OnPlayerDied());
            Assert.AreEqual(DeathOutcome.Respawn, lives.OnPlayerDied());
            Assert.AreEqual(DeathOutcome.GameOver, lives.OnPlayerDied());
            Assert.AreEqual(0, run.Lives);
        }

        [Test]
        public void Respawn_KeepsCoinsDefeatedEnemiesAndUsedPotions()
        {
            var run = new RunState(3);
            var lives = new LivesAndRespawn(run);
            lives.Save(0);
            run.TryCollectCoin(0);
            run.TryCollectCoin(1);
            run.MarkEnemyDefeated(4);
            run.MarkPotionUsed(2);
            run.TryCollectCoin(2); // 保存より後に取った金貨も保持する

            lives.OnPlayerDied();

            Assert.AreEqual(3, run.CoinCount);
            Assert.IsTrue(run.IsEnemyDefeated(4));
            Assert.IsTrue(run.IsPotionUsed(2));
            Assert.AreEqual(0, run.RespawnPointId);
        }
    }
}
