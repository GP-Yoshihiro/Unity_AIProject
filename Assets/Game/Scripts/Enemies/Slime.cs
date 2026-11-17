using CryptEscape.Game;

namespace CryptEscape.Enemies
{
    /// <summary>近づくと追いかけて体当たりする（予備動作なし、1秒ごと）。</summary>
    public class Slime : EnemyBase
    {
        protected override EnemyStats Stats =>
            GameDirector.Instance != null ? GameDirector.Instance.Config.slime : FallbackStats;

        static readonly EnemyStats FallbackStats = new EnemyStats
        {
            maxHp = 2, moveSpeed = 2.5f, detectRange = 5f, leashRange = 8f,
            attackRange = 1.1f, attackDamage = 1, attackWindup = 0f, attackCooldown = 1f,
        };
    }
}
