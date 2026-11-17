using UnityEngine;
using CryptEscape.Game;

namespace CryptEscape.Enemies
{
    /// <summary>2点の間を巡回し、プレイヤーを見つけると予備動作（赤く光る）のあと武器で攻撃する。</summary>
    public class Goblin : EnemyBase
    {
        [SerializeField] Vector3 patrolA;
        [SerializeField] Vector3 patrolB;

        bool towardB = true;

        protected override EnemyStats Stats =>
            GameDirector.Instance != null ? GameDirector.Instance.Config.goblin : FallbackStats;

        static readonly EnemyStats FallbackStats = new EnemyStats
        {
            maxHp = 3, moveSpeed = 3f, detectRange = 6f, leashRange = 10f,
            attackRange = 1.5f, attackDamage = 1, attackWindup = 0.4f, attackCooldown = 1.2f,
        };

        protected override EnemyState IdleState => EnemyState.Patrol;

        public void SetPatrol(Vector3 a, Vector3 b)
        {
            patrolA = a;
            patrolB = b;
        }

        protected override void UpdateIdle(float dt)
        {
            if (patrolA == patrolB)
            {
                Stop();
                return;
            }
            var target = towardB ? patrolB : patrolA;
            if (FlatDistance(transform.position, target) < 0.3f) towardB = !towardB;
            MoveToward(target, Stats.moveSpeed * 0.6f);
        }

        public override void ResetToHome()
        {
            base.ResetToHome();
            towardB = true;
        }
    }
}
