using UnityEngine;
using CryptEscape.Core;

namespace CryptEscape.Game
{
    /// <summary>調整用の数値。初期値は spec の Assumptions と data-model.md に従う。</summary>
    [CreateAssetMenu(menuName = "CryptEscape/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Player")]
        public float moveSpeed = 6f;
        public int maxHearts = 5;
        public int startLives = 3;
        public float hitInvincibility = 1.0f;
        public float attackCooldown = 0.4f;
        public float attackRange = 1.6f;
        public float attackHalfAngle = 60f;
        public int attackDamage = 1;
        public float dodgeDistance = 3f;
        public float dodgeDuration = 0.3f;
        public float dodgeCooldown = 0.8f;
        public float dyingDuration = 1.5f;

        [Header("Items")]
        public int potionHeal = 2;
        public float saveRange = 1.8f;
        public float doorHintRange = 3.5f;

        [Header("Slime")]
        public EnemyStats slime = new EnemyStats
        {
            maxHp = 2, moveSpeed = 2.5f, detectRange = 5f, leashRange = 8f,
            attackRange = 1.1f, attackDamage = 1, attackWindup = 0f, attackCooldown = 1.0f,
        };

        [Header("Goblin")]
        public EnemyStats goblin = new EnemyStats
        {
            maxHp = 3, moveSpeed = 3f, detectRange = 6f, leashRange = 10f,
            attackRange = 1.5f, attackDamage = 1, attackWindup = 0.4f, attackCooldown = 1.2f,
        };

        [Header("Boss")]
        public BossTimings bossTimings = new BossTimings
        {
            maxHp = 20, idle = 1.0f, chargeTell = 0.8f, chargeMaxTime = 0.9f,
            chargeRecover = 1.5f, aoeTell = 1.0f, aoeRecover = 1.2f,
        };
        public float bossChargeSpeed = 14f;
        public float bossWalkSpeed = 1.5f;
        public float bossAoeRadius = 3.5f;
        public float bossBodyRadius = 1.3f;
        public int bossChargeDamage = 2;
        public int bossAoeDamage = 1;
    }

    [System.Serializable]
    public struct EnemyStats
    {
        public int maxHp;
        public float moveSpeed;
        public float detectRange;
        public float leashRange;
        public float attackRange;
        public int attackDamage;
        public float attackWindup;
        public float attackCooldown;
    }
}
