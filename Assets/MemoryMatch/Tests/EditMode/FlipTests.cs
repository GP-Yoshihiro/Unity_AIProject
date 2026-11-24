using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class FlipTests
    {
        // 配置: 0 と 1、2 と 3 … が同じ絵柄
        internal static MemoryLogic NewGame() => new MemoryLogic(new[] { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5 });

        internal static void Space(MemoryLogic g, int index)
        {
            g.MoveCursor(-9, -9);
            for (int i = 0; i < index % 4; i++) g.MoveCursor(1, 0);
            for (int i = 0; i < index / 4; i++) g.MoveCursor(0, 1);
            g.PressSpace();
        }

        [Test]
        public void UT02_CursorStartsTopLeftMovesAndStopsAtEdges()
        {
            var g = NewGame();
            Assert.That(g.Cursor, Is.EqualTo(0));
            g.MoveCursor(-1, 0);
            g.MoveCursor(0, -1);
            Assert.That(g.Cursor, Is.EqualTo(0));
            g.MoveCursor(1, 0);
            g.MoveCursor(0, 1);
            Assert.That(g.Cursor, Is.EqualTo(5));
            for (int i = 0; i < 9; i++) { g.MoveCursor(1, 0); g.MoveCursor(0, 1); }
            Assert.That(g.Cursor, Is.EqualTo(11));
        }

        [Test]
        public void UT02_SpaceTurnsOnlyTheCardUnderTheCursor()
        {
            var g = NewGame();
            Space(g, 5);
            for (int i = 0; i < 12; i++) Assert.That(g.FaceAt(i) != CardFace.Down, Is.EqualTo(i == 5));
        }

        [Test]
        public void UT02_ThirdCardCannotBeTurned()
        {
            var g = NewGame();
            Space(g, 0);
            Space(g, 2);
            Assert.That(g.FaceAt(0), Is.EqualTo(CardFace.Up));
            Assert.That(g.FaceAt(2), Is.EqualTo(CardFace.Up));
            Space(g, 4);
            Assert.That(g.FaceAt(4), Is.EqualTo(CardFace.Down));
        }
    }
}
