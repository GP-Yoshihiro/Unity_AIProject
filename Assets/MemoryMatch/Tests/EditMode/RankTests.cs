using System.Linq;
using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class RankTests
    {
        private static void Take(MemoryLogic g, int pair)
        {
            FlipTests.Space(g, pair * 2);
            FlipTests.Space(g, pair * 2 + 1);
        }

        private static void Miss(MemoryLogic g)
        {
            FlipTests.Space(g, 8);
            FlipTests.Space(g, 10);
            g.PressSpace();
        }

        [Test]
        public void UT11_EachPlayerHasOwnScoreAndCombo()
        {
            var g = VersusTests.NewGame(2);
            Take(g, 0);
            Take(g, 1);
            Assert.That(g.ScoreOf(0), Is.EqualTo(300));
            Assert.That(g.ComboOf(0), Is.EqualTo(2));
            Miss(g);
            Assert.That(g.ScoreOf(0), Is.EqualTo(290));
            Assert.That(g.ComboOf(0), Is.EqualTo(0));
            Take(g, 2);
            Assert.That(g.ScoreOf(1), Is.EqualTo(100));
            Assert.That(g.ScoreOf(0), Is.EqualTo(290));
        }

        [Test]
        public void UT11_ScoreFloorsAtZeroAndManyMissesDoNotEndTheGame()
        {
            var g = VersusTests.NewGame(2);
            for (int n = 0; n < 60; n++) Miss(g);
            Assert.That(g.Phase, Is.EqualTo(GamePhase.PickFirst));
            Assert.That(g.ScoreOf(0), Is.EqualTo(0));
            Assert.That(g.ScoreOf(1), Is.EqualTo(0));
        }

        [Test]
        public void UT11_RanksFollowFinalScore()
        {
            var g = VersusTests.NewGame(2);
            Take(g, 0);              // P1 100
            Miss(g);                 // P1 90
            Take(g, 1);              // P2 100
            Miss(g);                 // P2 90
            Take(g, 2);              // P1 190
            Miss(g);                 // P1 180
            for (int p = 3; p < 5; p++) Take(g, p); // P2 190, 390
            FlipTests.Space(g, 10);
            FlipTests.Space(g, 11);  // P2 690
            Assert.That(g.Phase, Is.EqualTo(GamePhase.Cleared));
            Assert.That(new[] { g.ScoreOf(0), g.ScoreOf(1) }, Is.EqualTo(new[] { 180, 690 }));
            Assert.That(new[] { g.RankOf(0), g.RankOf(1) }, Is.EqualTo(new[] { 2, 1 }));
        }

        [Test]
        public void UT11_FewerPairsCanWinByCombo()
        {
            var spec = BoardSpec.Hard; // 12ペア
            var g = new MemoryLogic(spec.CreateCards(), 2, spec);
            void TakeAt(int pair) { DifficultyTests.Select(g, pair * 2); DifficultyTests.Select(g, pair * 2 + 1); }
            void MissAt() { DifficultyTests.Select(g, 22); DifficultyTests.Select(g, 20); g.PressSpace(); }
            for (int p = 0; p < 5; p++) TakeAt(p);                  // P1 1500
            MissAt();                                                // P1 1490
            for (int p = 5; p < 10; p++) { TakeAt(p); MissAt(); MissAt(); } // P2 90×5=450、P1 は-10×5で1440
            TakeAt(10);                                              // P2 550
            TakeAt(11);                                              // P2 750
            Assert.That(g.Phase, Is.EqualTo(GamePhase.Cleared));
            Assert.That(new[] { g.PairsOf(0), g.PairsOf(1) }, Is.EqualTo(new[] { 5, 7 }));
            Assert.That(new[] { g.ScoreOf(0), g.ScoreOf(1) }, Is.EqualTo(new[] { 1440, 750 }));
            Assert.That(new[] { g.RankOf(0), g.RankOf(1) }, Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void UT11_TiesShareARankAndTheNextRankIsSkipped()
        {
            var g = VersusTests.NewGame(3);
            Take(g, 0); Miss(g);     // P1 90
            Take(g, 1); Miss(g);     // P2 90
            for (int p = 2; p < 6; p++) Take(g, p); // P3 1000
            Assert.That(new[] { g.RankOf(0), g.RankOf(1), g.RankOf(2) }, Is.EqualTo(new[] { 2, 2, 1 }));
        }

        [Test]
        public void UT11_RestartResetsScoresAndCombos()
        {
            var g = VersusTests.NewGame(2);
            Take(g, 0);
            Assert.That(g.ScoreOf(0), Is.EqualTo(100));
            g.Restart(new[] { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5 });
            Assert.That(new[] { g.ScoreOf(0), g.ComboOf(0) }, Is.EqualTo(new[] { 0, 0 }));
        }
    }
}
