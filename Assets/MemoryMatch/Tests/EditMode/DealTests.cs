using System.Linq;
using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class DealTests
    {
        [Test]
        public void UT01_EachSymbolAppearsExactlyTwice()
        {
            var deck = new DeckShuffler(1).Next(BoardSpec.Easy);
            Assert.That(deck.Length, Is.EqualTo(12));
            for (int s = 0; s < 6; s++) Assert.That(deck.Count(x => x == s), Is.EqualTo(2));
        }

        [Test]
        public void UT01_StartsAllDownWithZeroCounters()
        {
            var g = new MemoryLogic(new DeckShuffler(1).Next(BoardSpec.Easy));
            for (int i = 0; i < 12; i++) Assert.That(g.FaceAt(i), Is.EqualTo(CardFace.Down));
            Assert.That(new[] { g.Matched, g.Misses, g.Score }, Is.EqualTo(new[] { 0, 0, 0 }));
        }

        [Test]
        public void UT07_SameSeedReproducesTheSequenceOfDeals()
        {
            var a = new DeckShuffler(42);
            var b = new DeckShuffler(42);
            for (int i = 0; i < 3; i++)
            {
                var deck = a.Next(BoardSpec.Easy);
                Assert.That(deck, Is.Not.Empty);
                Assert.That(b.Next(BoardSpec.Easy), Is.EqualTo(deck));
            }
        }

        [Test]
        public void UT07_NextDealDiffersAndOtherSeedDiffers()
        {
            var a = new DeckShuffler(42);
            var first = a.Next(BoardSpec.Easy);
            Assert.That(a.Next(BoardSpec.Easy), Is.Not.EqualTo(first));
            Assert.That(new DeckShuffler(43).Next(BoardSpec.Easy), Is.Not.EqualTo(first));
        }
    }
}
