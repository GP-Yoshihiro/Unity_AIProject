using NUnit.Framework;
using UnityEngine;
using CryptEscape.Core;

namespace CryptEscape.Tests
{
    public class SwordArcTests
    {
        static bool Hit(Vector3 target) =>
            SwordArc.Contains(Vector3.zero, Vector3.forward, target, range: 1.6f, halfAngleDeg: 60f);

        [Test]
        public void InFront_WithinRange_Hits() => Assert.IsTrue(Hit(new Vector3(0, 0, 1.5f)));

        [Test]
        public void TooFar_Misses() => Assert.IsFalse(Hit(new Vector3(0, 0, 1.7f)));

        [Test]
        public void Behind_Misses() => Assert.IsFalse(Hit(new Vector3(0, 0, -1f)));

        [Test]
        public void Side_Over60Degrees_Misses() => Assert.IsFalse(Hit(new Vector3(1f, 0, 0.3f)));

        [Test]
        public void Diagonal_Under60Degrees_Hits() => Assert.IsTrue(Hit(new Vector3(0.8f, 0, 0.8f)));

        [Test]
        public void HeightDifference_IsIgnored() => Assert.IsTrue(Hit(new Vector3(0, 1.2f, 1f)));
    }
}
