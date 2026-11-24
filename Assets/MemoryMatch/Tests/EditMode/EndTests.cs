using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class EndTests
    {
        private static MemoryLogic NewGame() => FlipTests.NewGame();
        private static void Space(MemoryLogic g, int i) => FlipTests.Space(g, i);

        internal static void ClearAll(MemoryLogic g)
        {
            for (int i = 0; i < 12; i++) Space(g, i);
        }

        internal static void MissTimes(MemoryLogic g, int times)
        {
            for (int n = 0; n < times; n++)
            {
                if (n > 0) g.PressSpace();
                Space(g, 0);
                Space(g, 2);
            }
        }

        [Test]
        public void UT05_PhaseFollowsTheTurn()
        {
            var g = NewGame();
            Assert.That(g.Phase, Is.EqualTo(GamePhase.PickFirst));
            Space(g, 0);
            Assert.That(g.Phase, Is.EqualTo(GamePhase.PickSecond));
            Space(g, 2);
            Assert.That(g.Phase, Is.EqualTo(GamePhase.ShowingMiss));
            g.PressSpace();
            Assert.That(g.Phase, Is.EqualTo(GamePhase.PickFirst));
        }

        [Test]
        public void UT05_AllPairsMatchedClearsAndIgnoresInput()
        {
            var g = NewGame();
            ClearAll(g);
            Assert.That(g.Matched, Is.EqualTo(6));
            Assert.That(g.Phase, Is.EqualTo(GamePhase.Cleared));
            g.MoveCursor(-1, 0);
            g.PressSpace();
            Assert.That(g.Cursor, Is.EqualTo(11));
            Assert.That(g.Phase, Is.EqualTo(GamePhase.Cleared));
        }

        [Test]
        public void UT05_FourMissesDoNotEndTheGame()
        {
            var g = NewGame();
            MissTimes(g, 4);
            Assert.That(g.Phase, Is.EqualTo(GamePhase.ShowingMiss));
        }

        [Test]
        public void UT05_FifthMissEndsTheGameKeepsCardsUpAndIgnoresInput()
        {
            var g = NewGame();
            MissTimes(g, 5);
            Assert.That(g.Misses, Is.EqualTo(5));
            Assert.That(g.Phase, Is.EqualTo(GamePhase.GameOver));
            int cursor = g.Cursor;
            g.PressSpace();
            g.MoveCursor(1, 1);
            Assert.That(g.Cursor, Is.EqualTo(cursor));
            Assert.That(g.FaceAt(0), Is.EqualTo(CardFace.Up));
            Assert.That(g.FaceAt(2), Is.EqualTo(CardFace.Up));
        }
    }
}
