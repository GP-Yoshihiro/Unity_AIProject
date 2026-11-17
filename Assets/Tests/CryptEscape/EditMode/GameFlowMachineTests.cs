using NUnit.Framework;
using CryptEscape.Core;

namespace CryptEscape.Tests
{
    public class GameFlowMachineTests
    {
        [Test]
        public void StartsAtTitle_AndStartGoesToPlaying()
        {
            var flow = new GameFlowMachine();
            Assert.AreEqual(GameFlow.Title, flow.Current);

            Assert.IsTrue(flow.StartGame());
            Assert.AreEqual(GameFlow.Playing, flow.Current);
        }

        [Test]
        public void PauseAndResume_OnlyFromPlaying()
        {
            var flow = new GameFlowMachine();
            Assert.IsFalse(flow.Pause(), "タイトルではポーズできない");

            flow.StartGame();
            Assert.IsTrue(flow.Pause());
            Assert.AreEqual(GameFlow.Paused, flow.Current);
            Assert.IsTrue(flow.Resume());
            Assert.AreEqual(GameFlow.Playing, flow.Current);
        }

        [Test]
        public void Dying_ThenRespawnOrGameOver()
        {
            var flow = new GameFlowMachine();
            flow.StartGame();

            Assert.IsTrue(flow.BeginDying());
            Assert.AreEqual(GameFlow.Dying, flow.Current);
            Assert.IsTrue(flow.FinishDying(hasLivesLeft: true));
            Assert.AreEqual(GameFlow.Playing, flow.Current);

            flow.BeginDying();
            flow.FinishDying(hasLivesLeft: false);
            Assert.AreEqual(GameFlow.GameOver, flow.Current);
        }

        [Test]
        public void Restart_FromAnyStateExceptTitle_GoesToPlaying()
        {
            foreach (var setup in new System.Action<GameFlowMachine>[]
            {
                f => { },
                f => f.Pause(),
                f => f.BeginDying(),
                f => { f.BeginDying(); f.FinishDying(false); },
                f => f.Clear(),
            })
            {
                var flow = new GameFlowMachine();
                flow.StartGame();
                setup(flow);

                Assert.IsTrue(flow.Restart(), $"from {flow.Current}");
                Assert.AreEqual(GameFlow.Playing, flow.Current);
            }

            Assert.IsFalse(new GameFlowMachine().Restart(), "タイトルからは Restart しない");
        }

        [Test]
        public void Clear_OnlyFromPlaying_AndToTitle()
        {
            var flow = new GameFlowMachine();
            Assert.IsFalse(flow.Clear());

            flow.StartGame();
            Assert.IsTrue(flow.Clear());
            Assert.AreEqual(GameFlow.Cleared, flow.Current);

            Assert.IsTrue(flow.ToTitle());
            Assert.AreEqual(GameFlow.Title, flow.Current);
        }

        [Test]
        public void TimerRuns_OnlyWhilePlayingOrDying()
        {
            Assert.IsTrue(GameFlowMachine.TimerRuns(GameFlow.Playing));
            Assert.IsTrue(GameFlowMachine.TimerRuns(GameFlow.Dying));
            Assert.IsFalse(GameFlowMachine.TimerRuns(GameFlow.Paused));
            Assert.IsFalse(GameFlowMachine.TimerRuns(GameFlow.Title));
            Assert.IsFalse(GameFlowMachine.TimerRuns(GameFlow.GameOver));
            Assert.IsFalse(GameFlowMachine.TimerRuns(GameFlow.Cleared));
        }
    }
}
