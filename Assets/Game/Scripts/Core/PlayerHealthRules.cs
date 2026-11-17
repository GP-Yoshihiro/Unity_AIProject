using System;

namespace CryptEscape.Core
{
    /// <summary>ハートと無敵時間。無敵中の被弾は無視するので、同じ瞬間の複数の被弾は1回分になる。</summary>
    public class PlayerHealthRules
    {
        public int MaxHearts { get; }
        public int Hearts { get; private set; }
        public float InvincibleRemaining { get; private set; }
        public bool IsInvincible => InvincibleRemaining > 0f;
        public bool IsDead => Hearts <= 0;

        public PlayerHealthRules(int maxHearts)
        {
            MaxHearts = maxHearts;
            Reset();
        }

        public bool TryDamage(int amount, float invincibleSeconds)
        {
            if (IsDead || IsInvincible || amount <= 0) return false;

            Hearts = Math.Max(0, Hearts - amount);
            InvincibleRemaining = invincibleSeconds;
            return true;
        }

        public bool TryHeal(int amount)
        {
            if (IsDead || Hearts >= MaxHearts) return false;
            Hearts = Math.Min(MaxHearts, Hearts + amount);
            return true;
        }

        public void GrantInvincibility(float seconds) =>
            InvincibleRemaining = Math.Max(InvincibleRemaining, seconds);

        public void Tick(float deltaTime) =>
            InvincibleRemaining = Math.Max(0f, InvincibleRemaining - deltaTime);

        public void Reset()
        {
            Hearts = MaxHearts;
            InvincibleRemaining = 0f;
        }
    }
}
