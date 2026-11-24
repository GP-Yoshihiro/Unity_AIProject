using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class VersusTests
    {
        internal static MemoryLogic NewGame(int players) => new MemoryLogic(new[] { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5 }, players);
        private static void Space(MemoryLogic g, int i) => FlipTests.Space(g, i);

        [Test]
        public void UT10_MatchedPairIsTakenByCurrentPlayerWhoContinues()
        {
            var g = NewGame(2);
            Space(g, 0);
            Space(g, 1);
            Assert.That(g.PairsOf(0), Is.EqualTo(1));
            Assert.That(g.PairsOf(1), Is.EqualTo(0));
            Assert.That(g.OwnerAt(0), Is.EqualTo(0));
            Assert.That(g.OwnerAt(1), Is.EqualTo(0));
            Assert.That(g.OwnerAt(2), Is.EqualTo(-1));
            Assert.That(g.CurrentPlayer, Is.EqualTo(0));
        }

        [Test]
        public void UT10_TurnPassesWhenMissIsTurnedBack()
        {
            var g = NewGame(2);
            Space(g, 0);
            Space(g, 2);
            Assert.That(g.CurrentPlayer, Is.EqualTo(0));
            g.PressSpace();
            Assert.That(g.CurrentPlayer, Is.EqualTo(1));
            Space(g, 0);
            Space(g, 1);
            Assert.That(g.OwnerAt(0), Is.EqualTo(1));
            Assert.That(g.PairsOf(1), Is.EqualTo(1));
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void UT10_TurnsGoAroundAllPlayers(int players)
        {
            var g = NewGame(players);
            for (int n = 0; n <= players; n++)
            {
                Assert.That(g.PlayerCount, Is.EqualTo(players));
                Assert.That(g.CurrentPlayer, Is.EqualTo(n % players));
                Space(g, 0);
                Space(g, 2);
                g.PressSpace();
            }
        }

        [Test]
        public void UT10_RestartResetsOwnersPairsAndTurn()
        {
            var g = NewGame(2);
            Space(g, 0);
            Space(g, 2);
            g.PressSpace();
            Space(g, 0);
            Space(g, 1);
            Assert.That(g.PairsOf(1), Is.EqualTo(1));
            g.Restart(new[] { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5 });
            Assert.That(g.CurrentPlayer, Is.EqualTo(0));
            Assert.That(g.PairsOf(1), Is.EqualTo(0));
            Assert.That(g.OwnerAt(0), Is.EqualTo(-1));
        }
    }
}
