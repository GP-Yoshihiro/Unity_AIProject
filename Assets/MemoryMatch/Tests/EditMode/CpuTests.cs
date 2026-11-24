using System.Linq;
using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class CpuTests
    {
        private static readonly int[] All = Enumerable.Range(0, 12).ToArray();

        [Test]
        public void UT12_PicksAKnownPair()
        {
            var cpu = new CpuPlayer(1);
            cpu.Observe(3, 0);
            cpu.Observe(5, 1);
            cpu.Observe(9, 0);
            int first = cpu.ChooseFirst(All);
            Assert.That(first, Is.EqualTo(3).Or.EqualTo(9));
            Assert.That(cpu.ChooseSecond(All.Where(i => i != first).ToArray(), 0), Is.EqualTo(first == 3 ? 9 : 3));
        }

        [Test]
        public void UT12_SecondPickUsesMemoryOfTheMate()
        {
            var cpu = new CpuPlayer(1);
            cpu.Observe(7, 2);
            cpu.Observe(0, 2);
            Assert.That(cpu.ChooseSecond(All.Where(i => i != 0).ToArray(), 2), Is.EqualTo(7));
        }

        [Test]
        public void UT12_RemembersOnlyTheLastFourCards()
        {
            for (int seed = 0; seed < 50; seed++)
            {
                var cpu = new CpuPlayer(seed);
                cpu.Observe(0, 0);
                for (int i = 1; i <= 4; i++) cpu.Observe(i, i);
                Assert.That(cpu.RememberedCards, Is.EquivalentTo(new[] { 1, 2, 3, 4 }));
            }
        }

        [Test]
        public void UT12_SeeingACardAgainRefreshesIt()
        {
            var cpu = new CpuPlayer(1);
            for (int i = 0; i < 4; i++) cpu.Observe(i, i);
            cpu.Observe(0, 0);
            cpu.Observe(4, 4);
            Assert.That(cpu.RememberedCards, Is.EquivalentTo(new[] { 0, 2, 3, 4 }));
        }

        [Test]
        public void UT12_ExploresOnlySelectableCardsAndPrefersUnknown()
        {
            for (int seed = 0; seed < 50; seed++)
            {
                var cpu = new CpuPlayer(seed);
                cpu.Observe(1, 1);
                Assert.That(cpu.ChooseFirst(new[] { 1, 6 }), Is.EqualTo(6));
                Assert.That(cpu.ChooseSecond(new[] { 1, 8 }, 5), Is.EqualTo(8));
                Assert.That(cpu.ChooseFirst(new[] { 1 }), Is.EqualTo(1));
            }
        }

        [Test]
        public void UT12_DoesNotPickRememberedCardsThatWereTaken()
        {
            var cpu = new CpuPlayer(1);
            cpu.Observe(3, 0);
            cpu.Observe(9, 0);
            Assert.That(cpu.ChooseFirst(new[] { 1 }), Is.EqualTo(1));
        }

        [Test]
        public void UT12_ResetForgetsEverything()
        {
            var cpu = new CpuPlayer(1);
            cpu.Observe(3, 0);
            Assert.That(cpu.RememberedCards, Is.Not.Empty);
            cpu.Reset();
            Assert.That(cpu.RememberedCards, Is.Empty);
        }
    }
}
