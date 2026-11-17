using NUnit.Framework;
using CryptEscape.Core;

namespace CryptEscape.Tests
{
    public class BossBrainTests
    {
        static BossBrain NewBrain() => new BossBrain(new BossTimings
        {
            maxHp = 20,
            idle = 1f,
            chargeTell = 0.8f,
            chargeMaxTime = 0.9f,
            chargeRecover = 1.5f,
            aoeTell = 1f,
            aoeRecover = 1.2f,
        });

        [Test]
        public void Dormant_UntilActivated_AndTakesNoDamage()
        {
            var boss = NewBrain();
            Assert.AreEqual(BossState.Dormant, boss.State);
            Assert.IsFalse(boss.TakeDamage(1));
            Assert.AreEqual(20, boss.Hp);

            boss.Tick(5f);
            Assert.AreEqual(BossState.Dormant, boss.State);
        }

        [Test]
        public void Cycle_ChargeThenAoe_InOrder()
        {
            var boss = NewBrain();
            boss.Activate();
            Assert.AreEqual(BossState.Idle, boss.State);

            Assert.AreEqual(BossEvent.ChargeTellStarted, boss.Tick(1.01f));
            Assert.AreEqual(BossState.ChargeTell, boss.State);
            Assert.AreEqual(BossEvent.ChargeStarted, boss.Tick(0.81f));
            Assert.AreEqual(BossState.Charge, boss.State);
            boss.Tick(0.91f); // 最大時間で突進終了
            Assert.AreEqual(BossState.Recover, boss.State);
            Assert.AreEqual(BossEvent.AoeTellStarted, boss.Tick(1.51f));
            Assert.AreEqual(BossState.AoeTell, boss.State);
            Assert.AreEqual(BossEvent.AoeHit, boss.Tick(1.01f));
            Assert.AreEqual(BossState.Recover, boss.State);
            Assert.AreEqual(BossEvent.ChargeTellStarted, boss.Tick(1.21f));
            Assert.AreEqual(BossState.ChargeTell, boss.State);
        }

        [Test]
        public void EndCharge_EarlyOnWallHit()
        {
            var boss = NewBrain();
            boss.Activate();
            boss.Tick(1.01f);
            boss.Tick(0.81f);

            boss.EndCharge();
            Assert.AreEqual(BossState.Recover, boss.State);
        }

        [Test]
        public void Telegraphs_AreAtLeast06Seconds()
        {
            var boss = NewBrain();
            boss.Activate();
            boss.Tick(1.01f);
            Assert.AreEqual(BossState.ChargeTell, boss.State);
            boss.Tick(0.6f);
            Assert.AreEqual(BossState.ChargeTell, boss.State, "0.6秒ではまだ突進しない");
        }

        [Test]
        public void ZeroHp_Defeated_AndStaysDefeated()
        {
            var boss = NewBrain();
            boss.Activate();

            Assert.IsFalse(boss.TakeDamage(19));
            Assert.IsTrue(boss.TakeDamage(1));
            Assert.AreEqual(BossState.Defeated, boss.State);
            Assert.IsFalse(boss.TakeDamage(1), "撃破後は二重に撃破しない");
            Assert.AreEqual(BossEvent.None, boss.Tick(10f));
        }

        [Test]
        public void Reset_FullHpAndDormant()
        {
            var boss = NewBrain();
            boss.Activate();
            boss.TakeDamage(10);

            boss.Reset();

            Assert.AreEqual(20, boss.Hp);
            Assert.AreEqual(BossState.Dormant, boss.State);
        }
    }
}
