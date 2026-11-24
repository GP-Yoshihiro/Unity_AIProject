using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class ComboTests
    {
        private static void Space(MemoryLogic g, int i) => FlipTests.Space(g, i);

        [Test]
        public void UT08_ConsecutiveMatchesMultiplyUpToFive()
        {
            var g = FlipTests.NewGame();
            int[] expected = { 100, 300, 600, 1000, 1500, 2000 };
            for (int p = 0; p < 6; p++)
            {
                Space(g, p * 2);
                Space(g, p * 2 + 1);
                Assert.That(g.Score, Is.EqualTo(expected[p]));
                Assert.That(g.Combo, Is.EqualTo(p + 1));
            }
        }

        [Test]
        public void UT08_MissResetsComboAndPenaltyIsNotMultiplied()
        {
            var g = FlipTests.NewGame();
            for (int i = 0; i < 4; i++) Space(g, i);
            Space(g, 4);
            Space(g, 6);
            Assert.That(g.Score, Is.EqualTo(290));
            Assert.That(g.Combo, Is.EqualTo(0));
            g.PressSpace();
            Space(g, 4);
            Space(g, 5);
            Assert.That(g.Score, Is.EqualTo(390));
        }

        [Test]
        public void UT08_RestartResetsCombo()
        {
            var g = FlipTests.NewGame();
            Space(g, 0);
            Space(g, 1);
            Assert.That(g.Combo, Is.EqualTo(1));
            g.Restart(new[] { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5 });
            Assert.That(g.Combo, Is.EqualTo(0));
        }
    }
}
