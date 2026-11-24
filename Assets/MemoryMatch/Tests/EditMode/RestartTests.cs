using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class RestartTests
    {
        private static readonly int[] Reversed = { 5, 5, 4, 4, 3, 3, 2, 2, 1, 1, 0, 0 };

        private static void AssertInitial(MemoryLogic g)
        {
            for (int i = 0; i < 12; i++) Assert.That(g.FaceAt(i), Is.EqualTo(CardFace.Down));
            Assert.That(new[] { g.Matched, g.Misses, g.Score, g.Cursor }, Is.EqualTo(new[] { 0, 0, 0, 0 }));
            Assert.That(g.Phase, Is.EqualTo(GamePhase.PickFirst));
        }

        [Test]
        public void UT06_RestartMidTurnAndAfterMiss()
        {
            var g = FlipTests.NewGame();
            FlipTests.Space(g, 0);
            FlipTests.Space(g, 1);
            FlipTests.Space(g, 2);
            g.Restart(Reversed);
            AssertInitial(g);
            EndTests.MissTimes(g, 1);
            g.Restart(Reversed);
            AssertInitial(g);
        }

        [Test]
        public void UT06_RestartAfterClearAndGameOverIsPlayable()
        {
            var g = FlipTests.NewGame();
            EndTests.ClearAll(g);
            g.Restart(Reversed);
            AssertInitial(g);
            EndTests.MissTimes(g, 5);
            g.Restart(Reversed);
            AssertInitial(g);
            FlipTests.Space(g, 3);
            Assert.That(g.FaceAt(3), Is.EqualTo(CardFace.Up));
        }

        [Test]
        public void UT06_RestartUsesTheNewDeck()
        {
            var g = FlipTests.NewGame();
            g.Restart(new[] { 0, 1, 0, 1, 2, 3, 2, 3, 4, 5, 4, 5 });
            FlipTests.Space(g, 0);
            FlipTests.Space(g, 2);
            Assert.That(g.Matched, Is.EqualTo(1));
        }
    }
}
