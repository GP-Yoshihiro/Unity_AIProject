using UnityEngine;
using CryptEscape.Core;
using CryptEscape.Game;

namespace CryptEscape.Enemies
{
    /// <summary>
    /// ボスの体。行動の順番と時間は BossBrain が決め、ここは移動・予兆の表示・当たり判定を行う。
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class BossController : MonoBehaviour
    {
        [SerializeField] AttackTelegraph telegraph;
        [SerializeField] Renderer bodyRenderer;

        [Header("State (read-only)")]
        public string state;
        public int hp;

        BossBrain brain;
        Rigidbody body;
        Vector3 home;
        Quaternion homeRotation;
        Vector3 chargeDir;
        bool chargeHitDone;
        Color baseColor;
        float flashTimer;

        public BossBrain Brain => brain;
        public bool IsDefeated => brain != null && brain.State == BossState.Defeated;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            home = transform.position;
            homeRotation = transform.rotation;
            if (bodyRenderer != null) baseColor = bodyRenderer.material.color;
        }

        void Start()
        {
            var cfg = GameDirector.Instance != null ? GameDirector.Instance.Config : null;
            brain = new BossBrain(cfg != null ? cfg.bossTimings : new BossTimings
            {
                maxHp = 20, idle = 1f, chargeTell = 0.8f, chargeMaxTime = 0.9f,
                chargeRecover = 1.5f, aoeTell = 1f, aoeRecover = 1.2f,
            });
            Publish();
        }

        void Update()
        {
            if (flashTimer > 0f)
            {
                flashTimer -= Time.deltaTime;
                if (flashTimer <= 0f) ApplyStateColor();
            }
        }

        void FixedUpdate()
        {
            var director = GameDirector.Instance;
            if (brain == null || director == null || !director.WorldRunning || !brain.IsActive)
            {
                StopMoving();
                Publish();
                return;
            }

            var cfg = director.Config;
            var ev = brain.Tick(Time.fixedDeltaTime);
            var toPlayer = director.PlayerPosition - transform.position;
            toPlayer.y = 0f;

            switch (ev)
            {
                case BossEvent.ChargeTellStarted:
                    chargeDir = toPlayer.sqrMagnitude > 0.01f ? toPlayer.normalized : transform.forward;
                    telegraph.ShowChargeLine(transform.position, chargeDir, cfg.bossChargeSpeed * cfg.bossTimings.chargeMaxTime);
                    director.Audio.Play(AudioManager.BossTell);
                    break;
                case BossEvent.ChargeStarted:
                    telegraph.HideAll();
                    chargeHitDone = false;
                    break;
                case BossEvent.AoeTellStarted:
                    telegraph.ShowAoeCircle(transform.position, cfg.bossAoeRadius);
                    director.Audio.Play(AudioManager.BossTell);
                    break;
                case BossEvent.AoeHit:
                    telegraph.HideAll();
                    if (toPlayer.magnitude <= cfg.bossAoeRadius) director.DamagePlayer(cfg.bossAoeDamage);
                    break;
            }

            switch (brain.State)
            {
                case BossState.Idle:
                    MoveFlat(toPlayer.normalized * cfg.bossWalkSpeed);
                    Face(toPlayer);
                    break;
                case BossState.ChargeTell:
                    StopMoving();
                    Face(chargeDir);
                    break;
                case BossState.Charge:
                    MoveFlat(chargeDir * cfg.bossChargeSpeed);
                    Face(chargeDir);
                    if (!chargeHitDone && toPlayer.magnitude <= cfg.bossBodyRadius + 0.6f)
                    {
                        chargeHitDone = true;
                        director.DamagePlayer(cfg.bossChargeDamage);
                    }
                    break;
                default:
                    StopMoving();
                    if (brain.State == BossState.Recover) Face(toPlayer);
                    break;
            }

            ApplyStateColorIfIdle();
            Publish();
        }

        void OnCollisionEnter(Collision collision)
        {
            // 突進中に壁へ当たったら止まる
            if (brain != null && brain.State == BossState.Charge && collision.collider.CompareTag("Wall"))
                brain.EndCharge();
        }

        public void Activate()
        {
            if (brain == null || brain.State != BossState.Dormant) return;
            brain.Activate();
            Publish();
        }

        /// <summary>剣が当たった。この攻撃で撃破したら true。</summary>
        public bool TakeHit(int damage)
        {
            if (brain == null || !brain.IsActive) return false;
            bool defeated = brain.TakeDamage(damage);
            flashTimer = 0.1f;
            if (bodyRenderer != null) bodyRenderer.material.color = Color.white;
            if (defeated)
            {
                telegraph.HideAll();
                StopMoving();
            }
            Publish();
            return defeated;
        }

        /// <summary>プレイヤーが倒れて復活したとき、満タン・初期位置・待機に戻す。</summary>
        public void ResetBoss()
        {
            brain?.Reset();
            telegraph.HideAll();
            body.linearVelocity = Vector3.zero;
            body.position = home;
            transform.SetPositionAndRotation(home, homeRotation);
            ApplyStateColor();
            Publish();
        }

        void Publish()
        {
            if (brain == null) return;
            state = brain.State.ToString();
            hp = brain.Hp;
        }

        void MoveFlat(Vector3 v)
        {
            var current = body.linearVelocity;
            current.x = v.x;
            current.z = v.z;
            body.linearVelocity = current;
        }

        void StopMoving() => MoveFlat(Vector3.zero);

        void Face(Vector3 dir)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f) body.MoveRotation(Quaternion.LookRotation(dir.normalized, Vector3.up));
        }

        void ApplyStateColorIfIdle()
        {
            if (flashTimer <= 0f) ApplyStateColor();
        }

        void ApplyStateColor()
        {
            if (bodyRenderer == null || brain == null) return;
            var c = baseColor;
            if (brain.State == BossState.ChargeTell || brain.State == BossState.AoeTell) c = Color.Lerp(baseColor, Color.red, 0.6f);
            if (brain.State == BossState.Recover) c = Color.Lerp(baseColor, Color.gray, 0.5f);
            if (brain.State == BossState.Defeated) c = new Color(0.2f, 0.2f, 0.2f);
            bodyRenderer.material.color = c;
        }
    }
}
