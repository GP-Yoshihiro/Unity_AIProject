using System.Linq;
using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class TrumpTests
    {
        private static readonly BoardSpec Trump = BoardSpec.Trump;

        [Test]
        public void UT14_DeckIsFiftyTwoDistinctCardsWithFourOfEachRank()
        {
            var deck = new DeckShuffler(1).Next(Trump);
            Assert.That(deck.Length, Is.EqualTo(52));
            Assert.That(deck.Distinct().Count(), Is.EqualTo(52));
            for (int rank = 0; rank < 13; rank++) Assert.That(deck.Count(c => Trump.KeyOf(c) == rank), Is.EqualTo(4));
            Assert.That(Trump.Columns * Trump.Rows, Is.EqualTo(52));
        }

        [Test]
        public void UT14_SameRankOfDifferentSuitsMatches()
        {
            var g = new MemoryLogic(Trump.CreateCards(), 2, Trump); // 整列: 位置 = カード番号
            DifficultyTests.Select(g, 0);
            DifficultyTests.Select(g, 39);
            Assert.That(g.PairsOf(0), Is.EqualTo(1));
        }

        [Test]
        public void UT14_SameSuitOfDifferentRanksDoesNotMatch()
        {
            var g = new MemoryLogic(Trump.CreateCards(), 2, Trump);
            DifficultyTests.Select(g, 0);
            DifficultyTests.Select(g, 1);
            Assert.That(g.PairsOf(0), Is.EqualTo(0));
            Assert.That(g.Phase, Is.EqualTo(GamePhase.ShowingMiss));
        }

        [Test]
        public void UT14_EndsWhenTwentySixPairsAreTaken()
        {
            var g = new MemoryLogic(Trump.CreateCards(), 2, Trump);
            for (int rank = 0; rank < 13; rank++)
            {
                for (int suit = 0; suit < 4; suit++)
                {
                    Assert.That(g.Phase, Is.Not.EqualTo(GamePhase.Cleared));
                    DifficultyTests.Select(g, suit * 13 + rank);
                }
            }
            Assert.That(g.Phase, Is.EqualTo(GamePhase.Cleared));
            Assert.That(g.PairsOf(0), Is.EqualTo(26));
        }

        [Test]
        public void UT14_VisibleKeyHidesFaceDownCards()
        {
            var g = new MemoryLogic(Trump.CreateCards(), 2, Trump);
            Assert.That(g.VisibleKeyAt(14), Is.EqualTo(-1));
            DifficultyTests.Select(g, 14);
            Assert.That(g.VisibleKeyAt(14), Is.EqualTo(1));
            Assert.That(g.VisibleSymbolAt(14), Is.EqualTo(14));
        }
    }
}
