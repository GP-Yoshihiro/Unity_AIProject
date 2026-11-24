using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class EffectLevelTests
    {
        [TestCase(0, 0)]
        [TestCase(1, 1)]
        [TestCase(4, 4)]
        [TestCase(5, 5)]
        [TestCase(6, 5)]
        [TestCase(12, 5)]
        public void UT15_LevelFollowsComboUpToFive(int combo, int level)
        {
            Assert.That(EffectLevel.For(combo).Level, Is.EqualTo(level));
        }

        [Test]
        public void UT15_EveryStrengthIncreasesWithEachLevel()
        {
            for (int combo = 1; combo < 5; combo++)
            {
                var a = EffectLevel.For(combo);
                var b = EffectLevel.For(combo + 1);
                Assert.That(b.Particles, Is.GreaterThan(a.Particles));
                Assert.That(b.TextSize, Is.GreaterThan(a.TextSize));
                Assert.That(b.Shake, Is.GreaterThan(a.Shake));
                Assert.That(b.Flash, Is.GreaterThan(a.Flash));
            }
        }

        [Test]
        public void UT15_NoComboHasNoParticlesShakeOrFlash()
        {
            var e = EffectLevel.For(0);
            Assert.That(e.Particles, Is.EqualTo(0));
            Assert.That(e.Shake, Is.EqualTo(0f));
            Assert.That(e.Flash, Is.EqualTo(0f));
        }

        [Test]
        public void UT15_FlashStaysWeakAtTheMaximum()
        {
            var e = EffectLevel.For(99);
            Assert.That(e.Flash, Is.GreaterThan(0f).And.LessThanOrEqualTo(0.35f));
        }
    }
}
