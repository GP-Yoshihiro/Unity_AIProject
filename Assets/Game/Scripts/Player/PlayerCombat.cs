using UnityEngine;
using CryptEscape.Game;

namespace CryptEscape.Player
{
    /// <summary>剣の攻撃（前方の扇形）と回避。命中の判定は GameDirector が迷宮内の敵とボスに対して行う。</summary>
    [RequireComponent(typeof(PlayerMotor), typeof(PlayerHealth))]
    public class PlayerCombat : MonoBehaviour
    {
        [SerializeField] GameObject slashEffect;

        PlayerMotor motor;
        PlayerHealth health;
        float attackCooldown;
        float dodgeCooldown;
        float slashTimer;

        void Awake()
        {
            motor = GetComponent<PlayerMotor>();
            health = GetComponent<PlayerHealth>();
            if (slashEffect != null) slashEffect.SetActive(false);
        }

        void Update()
        {
            attackCooldown -= Time.deltaTime;
            dodgeCooldown -= Time.deltaTime;
            if (slashTimer > 0f)
            {
                slashTimer -= Time.deltaTime;
                if (slashTimer <= 0f && slashEffect != null) slashEffect.SetActive(false);
            }

            var director = GameDirector.Instance;
            if (director == null || !director.ControlsEnabled) return;
            var cfg = director.Config;

            if (director.Input.Attack && attackCooldown <= 0f)
            {
                attackCooldown = cfg.attackCooldown;
                slashTimer = 0.12f;
                if (slashEffect != null) slashEffect.SetActive(true);
                director.PlayerAttack(transform.position, motor.Facing);
            }

            if (director.Input.Dodge && dodgeCooldown <= 0f)
            {
                dodgeCooldown = cfg.dodgeCooldown;
                var input = director.Input.Move;
                motor.Dash(new Vector3(input.x, 0f, input.y), cfg.dodgeDistance, cfg.dodgeDuration);
                health.Rules.GrantInvincibility(cfg.dodgeDuration);
            }
        }

        public void ResetCombat()
        {
            attackCooldown = 0f;
            dodgeCooldown = 0f;
            slashTimer = 0f;
            if (slashEffect != null) slashEffect.SetActive(false);
        }
    }
}
