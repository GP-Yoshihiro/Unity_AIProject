using UnityEngine;

namespace CryptEscape.Core
{
    /// <summary>剣の攻撃範囲（前方の扇形）。高さの差は無視して XZ 平面で判定する。</summary>
    public static class SwordArc
    {
        public static bool Contains(Vector3 origin, Vector3 forward, Vector3 target, float range, float halfAngleDeg)
        {
            var toTarget = target - origin;
            toTarget.y = 0f;
            forward.y = 0f;

            if (toTarget.sqrMagnitude > range * range) return false;
            if (toTarget.sqrMagnitude < 1e-6f) return true;

            return Vector3.Angle(forward, toTarget) <= halfAngleDeg;
        }
    }
}
