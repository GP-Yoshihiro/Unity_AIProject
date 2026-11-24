using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class MenuTests
    {
        [Test]
        public void UT09_ModesAreChosenVerticallyAndStopAtEnds()
        {
            var m = new MenuState();
            Assert.That(m.Mode, Is.EqualTo(GameMode.Solo));
            m.Move(0, -1);
            Assert.That(m.Mode, Is.EqualTo(GameMode.Solo));
            m.Move(0, 1);
            Assert.That(m.Mode, Is.EqualTo(GameMode.VsCpu));
            m.Move(0, 1);
            m.Move(0, 1);
            Assert.That(m.Mode, Is.EqualTo(GameMode.VsHuman));
        }

        [Test]
        public void UT09_PlayersAreTwoToFourInVersus()
        {
            var m = new MenuState();
            m.Move(0, 1);
            m.Move(-1, 0);
            Assert.That(m.PlayerCount, Is.EqualTo(2));
            for (int i = 0; i < 5; i++) m.Move(1, 0);
            Assert.That(m.PlayerCount, Is.EqualTo(4));
        }

        [Test]
        public void UT09_SoloIsAlwaysOnePlayer()
        {
            var m = new MenuState();
            m.Move(1, 0);
            Assert.That(m.PlayerCount, Is.EqualTo(1));
            m.Move(0, 1);
            Assert.That(m.PlayerCount, Is.EqualTo(2));
        }

        [Test]
        public void UT09_OnlyVsCpuHasCpuPlayersAndPlayerOneIsHuman()
        {
            var m = new MenuState();
            Assert.That(m.IsCpu(0), Is.False);
            m.Move(0, 1);
            Assert.That(m.IsCpu(0), Is.False);
            Assert.That(m.IsCpu(1), Is.True);
            m.Move(0, 1);
            Assert.That(m.IsCpu(1), Is.False);
        }

        [Test]
        public void UT13_SoloChoosesDifficultyHorizontallyAndStopsAtEnds()
        {
            var m = new MenuState();
            Assert.That(m.Spec, Is.SameAs(BoardSpec.Easy));
            m.Move(-1, 0);
            Assert.That(m.Spec, Is.SameAs(BoardSpec.Easy));
            m.Move(1, 0);
            Assert.That(m.Spec, Is.SameAs(BoardSpec.Normal));
            m.Move(1, 0);
            m.Move(1, 0);
            Assert.That(m.Spec, Is.SameAs(BoardSpec.Hard));
            Assert.That(m.DifficultyName, Is.EqualTo("HARD"));
        }

        [Test]
        public void UT14_VersusAlwaysUsesTrumpAndKeepsSoloDifficulty()
        {
            var m = new MenuState();
            m.Move(1, 0);
            m.Move(0, 1);
            Assert.That(m.Spec, Is.SameAs(BoardSpec.Trump));
            m.Move(1, 0);
            m.Move(0, 1);
            Assert.That(m.Spec, Is.SameAs(BoardSpec.Trump));
            m.Move(0, -1);
            m.Move(0, -1);
            Assert.That(m.Spec, Is.SameAs(BoardSpec.Normal));
        }
    }
}
