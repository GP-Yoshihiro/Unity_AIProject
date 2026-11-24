using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class JudgeTests
    {
        private static MemoryLogic NewGame() => FlipTests.NewGame();
        private static void Space(MemoryLogic g, int i) => FlipTests.Space(g, i);

        [Test]
        public void UT03_SameSymbolMatchesAtOnceAndAdds100()
        {
            var g = NewGame();
            Space(g, 0);
            Space(g, 1);
            Assert.That(g.FaceAt(0), Is.EqualTo(CardFace.Matched));
            Assert.That(g.FaceAt(1), Is.EqualTo(CardFace.Matched));
            Assert.That(new[] { g.Matched, g.Misses, g.Score }, Is.EqualTo(new[] { 1, 0, 100 }));
        }

        [Test]
        public void UT03_DifferentSymbolsMissAtOnceAndSubtract10()
        {
            var g = NewGame();
            Space(g, 0);
            Space(g, 1);
            Space(g, 2);
            Space(g, 4);
            Assert.That(new[] { g.Matched, g.Misses, g.Score }, Is.EqualTo(new[] { 1, 1, 90 }));
            Assert.That(g.FaceAt(2), Is.EqualTo(CardFace.Up));
            Assert.That(g.FaceAt(4), Is.EqualTo(CardFace.Up));
        }

        [Test]
        public void UT03_ScoreDoesNotGoBelowZero()
        {
            var g = NewGame();
            Space(g, 0);
            Space(g, 2);
            Assert.That(g.Misses, Is.EqualTo(1));
            Assert.That(g.Score, Is.EqualTo(0));
        }

        [Test]
        public void UT03_NextSpaceOnlyTurnsTheMissedCardsBack()
        {
            var g = NewGame();
            Space(g, 0);
            Space(g, 2);
            Space(g, 4);
            for (int i = 0; i < 12; i++) Assert.That(g.FaceAt(i), Is.EqualTo(CardFace.Down));
            g.PressSpace();
            Assert.That(g.FaceAt(4), Is.EqualTo(CardFace.Up));
        }

        [Test]
        public void UT03_CursorCanMoveWhileMissIsShown()
        {
            var g = NewGame();
            Space(g, 0);
            Space(g, 2);
            g.MoveCursor(1, 0);
            Assert.That(g.Cursor, Is.EqualTo(3));
        }
    }
}
