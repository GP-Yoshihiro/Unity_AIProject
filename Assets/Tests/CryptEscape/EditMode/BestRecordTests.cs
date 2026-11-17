using System.Collections.Generic;
using NUnit.Framework;
using CryptEscape.Core;

namespace CryptEscape.Tests
{
    public class BestRecordTests
    {
        static List<Split> Splits(float t) => new List<Split> { new Split("Coin1", t / 2), new Split("Escape", t) };

        [Test]
        public void FirstClear_IsAlwaysBest()
        {
            var best = new BestRecord();
            Assert.IsFalse(best.HasRecord);

            Assert.IsTrue(best.TryUpdate(120f, Splits(120f)));
            Assert.AreEqual(120f, best.BestTime);
        }

        [Test]
        public void SlowerRun_DoesNotUpdate()
        {
            var best = new BestRecord();
            best.TryUpdate(100f, Splits(100f));

            Assert.IsFalse(best.TryUpdate(110f, Splits(110f)));
            Assert.AreEqual(100f, best.BestTime);
            Assert.IsTrue(best.TryUpdate(90f, Splits(90f)));
            Assert.AreEqual(90f, best.BestTime);
        }

        [Test]
        public void DiffAgainstBest()
        {
            var best = new BestRecord();
            best.TryUpdate(100f, Splits(100f));

            Assert.AreEqual(-5f, best.DiffFrom(95f), 1e-5);
            Assert.AreEqual(3f, best.DiffFrom(103f), 1e-5);
        }

        [Test]
        public void Json_RoundTrip()
        {
            var best = new BestRecord();
            best.TryUpdate(77.5f, Splits(77.5f));

            var loaded = BestRecord.FromJson(best.ToJson());

            Assert.IsTrue(loaded.HasRecord);
            Assert.AreEqual(77.5f, loaded.BestTime, 1e-5);
            Assert.AreEqual(2, loaded.BestSplits.Count);
            Assert.AreEqual("Escape", loaded.BestSplits[1].name);
        }

        [Test]
        public void FromJson_EmptyOrBroken_IsNoRecord()
        {
            Assert.IsFalse(BestRecord.FromJson(null).HasRecord);
            Assert.IsFalse(BestRecord.FromJson("").HasRecord);
            Assert.IsFalse(BestRecord.FromJson("{broken").HasRecord);
        }
    }
}
