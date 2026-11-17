using NUnit.Framework;
using CryptEscape.Core;

namespace CryptEscape.Tests
{
    public class PlayerHealthRulesTests
    {
        [Test]
        public void Damage_ReducesHearts_ThenInvincible()
        {
            var hp = new PlayerHealthRules(maxHearts: 5);

            Assert.IsTrue(hp.TryDamage(1, invincibleSeconds: 1f));
            Assert.AreEqual(4, hp.Hearts);
            Assert.IsFalse(hp.TryDamage(1, 1f), "無敵中は減らない（同時被弾も1回分）");
            Assert.AreEqual(4, hp.Hearts);

            hp.Tick(1.01f);
            Assert.IsTrue(hp.TryDamage(2, 1f));
            Assert.AreEqual(2, hp.Hearts);
        }

        [Test]
        public void ZeroHearts_IsDead_AndNotBelowZero()
        {
            var hp = new PlayerHealthRules(5);
            hp.TryDamage(7, 1f);

            Assert.AreEqual(0, hp.Hearts);
            Assert.IsTrue(hp.IsDead);
        }

        [Test]
        public void Heal_CapsAtMax_AndRefusesWhenFull()
        {
            var hp = new PlayerHealthRules(5);
            Assert.IsFalse(hp.TryHeal(2), "満タンなら回復しない");

            hp.TryDamage(1, 0f);
            Assert.IsTrue(hp.TryHeal(2));
            Assert.AreEqual(5, hp.Hearts);
        }

        [Test]
        public void DodgeInvincibility_BlocksDamage()
        {
            var hp = new PlayerHealthRules(5);
            hp.GrantInvincibility(0.3f);

            Assert.IsFalse(hp.TryDamage(1, 1f));
            hp.Tick(0.31f);
            Assert.IsTrue(hp.TryDamage(1, 1f));
        }

        [Test]
        public void Reset_RestoresFullAndClearsInvincibility()
        {
            var hp = new PlayerHealthRules(5);
            hp.TryDamage(5, 1f);

            hp.Reset();

            Assert.AreEqual(5, hp.Hearts);
            Assert.IsFalse(hp.IsInvincible);
        }
    }
}
