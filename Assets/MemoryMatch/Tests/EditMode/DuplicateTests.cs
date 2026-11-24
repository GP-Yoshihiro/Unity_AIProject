using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class DuplicateTests
    {
        private static MemoryLogic NewGame() => FlipTests.NewGame();
        private static void Space(MemoryLogic g, int i) => FlipTests.Space(g, i);

        [Test]
        public void UT04_SameCardCannotBeTheSecondCard()
        {
            var g = NewGame();
            Space(g, 0);
            g.PressSpace();
            Assert.That(new[] { g.Matched, g.Misses, g.Score }, Is.EqualTo(new[] { 0, 0, 0 }));
            Assert.That(g.FaceAt(0), Is.EqualTo(CardFace.Up));
            Space(g, 1);
            Assert.That(g.Matched, Is.EqualTo(1));
        }

        [Test]
        public void UT04_MatchedCardCannotBeChosenAgain()
        {
            var g = NewGame();
            Space(g, 0);
            Space(g, 1);
            Space(g, 0);
            Assert.That(g.FaceAt(0), Is.EqualTo(CardFace.Matched));
            Space(g, 1);
            Assert.That(new[] { g.Matched, g.Misses, g.Score }, Is.EqualTo(new[] { 1, 0, 100 }));
        }

        [Test]
        public void UT04_MatchedCardCannotBeTheSecondCard()
        {
            var g = NewGame();
            Space(g, 0);
            Space(g, 1);
            Space(g, 2);
            Space(g, 0);
            Assert.That(g.Misses, Is.EqualTo(0));
            Assert.That(g.FaceAt(0), Is.EqualTo(CardFace.Matched));
        }
    }
}
