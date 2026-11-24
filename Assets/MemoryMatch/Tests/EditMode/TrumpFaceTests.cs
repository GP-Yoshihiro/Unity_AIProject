using System.Linq;
using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class TrumpFaceTests
    {
        [TestCase(0, "A", "♠", false, false)]
        [TestCase(22, "10", "♥", true, false)]
        [TestCase(36, "J", "♦", true, true)]
        [TestCase(51, "K", "♣", false, true)]
        public void UT16_RankSuitColorAndCourt(int card, string rank, string suit, bool red, bool court)
        {
            var face = new TrumpFace(card);
            Assert.That(face.Rank, Is.EqualTo(rank));
            Assert.That(face.Suit, Is.EqualTo(suit));
            Assert.That(face.IsRed, Is.EqualTo(red));
            Assert.That(face.IsCourt, Is.EqualTo(court));
        }

        [Test]
        public void UT16_PipCountEqualsTheNumberAndCourtsHaveNone()
        {
            for (int card = 0; card < 52; card++)
            {
                int number = card % 13 + 1;
                Assert.That(new TrumpFace(card).Pips.Count, Is.EqualTo(number > 10 ? 0 : number));
            }
        }

        [Test]
        public void UT16_PipsAreDistinctAndInsideTheFace()
        {
            for (int n = 0; n < 10; n++)
            {
                var pips = new TrumpFace(n).Pips;
                Assert.That(pips.Distinct().Count(), Is.EqualTo(n + 1));
                foreach (var (x, y) in pips)
                {
                    Assert.That(x, Is.InRange(-1f, 1f));
                    Assert.That(y, Is.InRange(-1f, 1f));
                }
                Assert.That(pips.Sum(p => p.x), Is.EqualTo(0f).Within(0.001f));
            }
        }
    }
}
