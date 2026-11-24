using System.Linq;
using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class DifficultyTests
    {
        private static BoardSpec SpecOf(string name) => name == "Easy" ? BoardSpec.Easy : name == "Normal" ? BoardSpec.Normal : BoardSpec.Hard;

        private static MemoryLogic NewGame(BoardSpec spec) => new MemoryLogic(spec.CreateCards(), 1, spec);

        [TestCase("Easy", 4, 3, 5)]
        [TestCase("Normal", 4, 4, 10)]
        [TestCase("Hard", 6, 4, 20)]
        public void UT13_SizeLayoutAndMissLimit(string name, int columns, int rows, int missLimit)
        {
            var spec = SpecOf(name);
            Assert.That(new[] { spec.Columns, spec.Rows, spec.MissLimit }, Is.EqualTo(new[] { columns, rows, missLimit }));
            var deck = new DeckShuffler(1).Next(spec);
            Assert.That(deck.Length, Is.EqualTo(columns * rows));
            Assert.That(deck.GroupBy(x => x).All(g => g.Count() == 2), Is.True);
        }

        [TestCase("Normal")]
        [TestCase("Hard")]
        public void UT13_AllPairsClearAndCursorReachesTheLastCard(string name)
        {
            var spec = SpecOf(name);
            var g = NewGame(spec);
            for (int i = 0; i < spec.CardCount; i++) Select(g, i);
            Assert.That(g.Phase, Is.EqualTo(GamePhase.Cleared));
            Assert.That(g.Matched, Is.EqualTo(spec.CardCount / 2));
            Assert.That(g.Cursor, Is.EqualTo(spec.CardCount - 1));
        }

        [TestCase("Normal")]
        [TestCase("Hard")]
        public void UT13_GameOverExactlyAtTheLimit(string name)
        {
            var spec = SpecOf(name);
            var g = NewGame(spec);
            for (int n = 1; n <= spec.MissLimit; n++)
            {
                if (n > 1) g.PressSpace();
                Select(g, 0);
                Select(g, 2);
                Assert.That(g.Phase == GamePhase.GameOver, Is.EqualTo(n == spec.MissLimit));
            }
        }

        internal static void Select(MemoryLogic g, int index)
        {
            g.MoveCursor(-99, -99);
            g.MoveCursor(index % g.Spec.Columns, index / g.Spec.Columns);
            g.PressSpace();
        }
    }
}
