using NUnit.Framework;
using CryptEscape.Core;

namespace CryptEscape.Tests
{
    public class RtaTimerTests
    {
        [Test]
        public void Runs_OnlyWhilePlayingOrDying()
        {
            var timer = new RtaTimer();
            timer.Tick(1f, GameFlow.Title);
            timer.Tick(1f, GameFlow.Playing);
            timer.Tick(1f, GameFlow.Paused);
            timer.Tick(0.5f, GameFlow.Dying);

            Assert.AreEqual(1.5f, timer.Elapsed, 1e-5);
        }

        [Test]
        public void Splits_RecordedOncePerName_InOrder()
        {
            var timer = new RtaTimer();
            timer.Tick(10f, GameFlow.Playing);
            Assert.IsTrue(timer.Split("Coin1"));
            Assert.IsFalse(timer.Split("Coin1"));
            timer.Tick(5f, GameFlow.Playing);
            timer.Split("Coin2");

            Assert.AreEqual(2, timer.Splits.Count);
            Assert.AreEqual("Coin1", timer.Splits[0].name);
            Assert.AreEqual(10f, timer.Splits[0].time, 1e-5);
            Assert.AreEqual(15f, timer.Splits[1].time, 1e-5);
        }

        [Test]
        public void Finish_FreezesTime_AndAddsEscape()
        {
            var timer = new RtaTimer();
            timer.Tick(42f, GameFlow.Playing);
            timer.Finish();
            timer.Tick(5f, GameFlow.Playing);

            Assert.IsTrue(timer.Finished);
            Assert.AreEqual(42f, timer.Elapsed, 1e-5);
            Assert.AreEqual("Escape", timer.Splits[timer.Splits.Count - 1].name);
        }

        [Test]
        public void Reset_ClearsTimeAndSplits()
        {
            var timer = new RtaTimer();
            timer.Tick(3f, GameFlow.Playing);
            timer.Split("Coin1");
            timer.Finish();

            timer.Reset();

            Assert.AreEqual(0f, timer.Elapsed);
            Assert.AreEqual(0, timer.Splits.Count);
            Assert.IsFalse(timer.Finished);
        }

        [Test]
        public void Format_MinutesSecondsHundredths()
        {
            Assert.AreEqual("00:00.00", RtaTimer.Format(0f));
            Assert.AreEqual("01:23.45", RtaTimer.Format(83.456f));
            Assert.AreEqual("10:00.00", RtaTimer.Format(600f));
        }
    }
}
