using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class WinnerTests
    {
        private static void Take(MemoryLogic g, int pair)
        {
            FlipTests.Space(g, pair * 2);
            FlipTests.Space(g, pair * 2 + 1);
        }

        private static void Miss(MemoryLogic g)
        {
            FlipTests.Space(g, 8);
            FlipTests.Space(g, 10);
            g.PressSpace();
        }

        [Test]
        public void UT11_MostPairsWins()
        {
            var g = VersusTests.NewGame(2);
            for (int p = 0; p < 2; p++) Take(g, p);
            Miss(g);
            for (int p = 2; p < 6; p++) Take(g, p);
            Assert.That(g.Phase, Is.EqualTo(GamePhase.Cleared));
            Assert.That(g.Winners, Is.EqualTo(new[] { 1 }));
        }

        [Test]
        public void UT11_EqualPairsIsADraw()
        {
            var g = VersusTests.NewGame(2);
            for (int p = 0; p < 3; p++) Take(g, p);
            Miss(g);
            for (int p = 3; p < 6; p++) Take(g, p);
            Assert.That(g.Winners, Is.EqualTo(new[] { 0, 1 }));
        }

        [Test]
        public void UT11_NoWinnersBeforeTheEndOrInSolo()
        {
            Assert.That(VersusTests.NewGame(2).Winners, Is.Empty);
            var solo = FlipTests.NewGame();
            EndTests.ClearAll(solo);
            Assert.That(solo.Winners, Is.Empty);
        }

        [Test]
        public void UT11_VersusHasNoMissLimitScoreOrCombo()
        {
            var g = VersusTests.NewGame(2);
            for (int n = 0; n < 10; n++) Miss(g);
            Assert.That(g.Phase, Is.EqualTo(GamePhase.PickFirst));
            Take(g, 0);
            Assert.That(new[] { g.Score, g.Combo, g.Misses }, Is.EqualTo(new[] { 0, 0, 0 }));
        }
    }
}
