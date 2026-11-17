using UnityEngine;
using CryptEscape.Game;

namespace CryptEscape.Enemies
{
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Windup,
        Return,
        Stagger,
        Dead,
    }

    /// <summary>
    /// ザコ敵の共通処理。初期位置からの縄張り（leashRange）の中だけでプレイヤーを追い、見失うと戻る（research R3）。
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public abstract class EnemyBase : MonoBehaviour
    {
        public int enemyId;
        [SerializeField] protected Renderer bodyRenderer;

        [Header("State (read-only)")]
        public int hp;
        public EnemyState state;

        protected Rigidbody body;
        protected Vector3 home;
        protected float attackCooldown;
        protected float stateTimer;
        Color baseColor;
        float flashTimer;

        protected abstract EnemyStats Stats { get; }
        protected virtual Color WindupColor => new Color(1f, 0.35f, 0.2f);

        protected virtual void Awake()
        {
            body = GetComponent<Rigidbody>();
            home = transform.position;
            if (bodyRenderer != null) baseColor = bodyRenderer.material.color;
            hp = Stats.maxHp;
            state = IdleState;
        }

        protected virtual EnemyState IdleState => EnemyState.Idle;

        void Update()
        {
            if (flashTimer > 0f)
            {
                flashTimer -= Time.deltaTime;
                if (flashTimer <= 0f) SetColor(state == EnemyState.Windup ? WindupColor : baseColor);
            }
        }

        void FixedUpdate()
        {
            var director = GameDirector.Instance;
            if (director == null || !director.WorldRunning || state == EnemyState.Dead)
            {
                Stop();
                return;
            }

            float dt = Time.fixedDeltaTime;
            attackCooldown -= dt;
            stateTimer -= dt;

            var playerPos = director.PlayerPosition;
            float distToPlayer = FlatDistance(transform.position, playerPos);
            bool playerInTerritory = FlatDistance(home, playerPos) <= Stats.leashRange;
            bool canSee = director.PlayerTargetable && playerInTerritory;

            switch (state)
            {
                case EnemyState.Idle:
                case EnemyState.Patrol:
                    if (canSee && distToPlayer <= Stats.detectRange) state = EnemyState.Chase;
                    else UpdateIdle(dt);
                    break;

                case EnemyState.Chase:
                    if (!canSee)
                    {
                        state = EnemyState.Return;
                        break;
                    }
                    if (distToPlayer <= Stats.attackRange && attackCooldown <= 0f)
                    {
                        StartAttack(director, distToPlayer);
                        break;
                    }
                    MoveToward(playerPos, Stats.moveSpeed);
                    break;

                case EnemyState.Windup:
                    Stop();
                    if (stateTimer <= 0f)
                    {
                        if (canSee && FlatDistance(transform.position, director.PlayerPosition) <= Stats.attackRange + 0.4f)
                            director.DamagePlayer(Stats.attackDamage);
                        attackCooldown = Stats.attackCooldown;
                        SetColor(baseColor);
                        state = EnemyState.Chase;
                    }
                    break;

                case EnemyState.Return:
                    if (canSee && distToPlayer <= Stats.detectRange)
                    {
                        state = EnemyState.Chase;
                        break;
                    }
                    if (FlatDistance(transform.position, home) < 0.3f)
                    {
                        Stop();
                        state = IdleState;
                    }
                    else MoveToward(home, Stats.moveSpeed);
                    break;

                case EnemyState.Stagger:
                    if (stateTimer <= 0f) state = EnemyState.Chase;
                    break;
            }
        }

        protected virtual void UpdateIdle(float dt) => Stop();

        void StartAttack(GameDirector director, float distToPlayer)
        {
            if (Stats.attackWindup <= 0f)
            {
                director.DamagePlayer(Stats.attackDamage);
                attackCooldown = Stats.attackCooldown;
                return;
            }
            state = EnemyState.Windup;
            stateTimer = Stats.attackWindup;
            SetColor(WindupColor);
        }

        /// <summary>剣が当たった。体力が0になったら true。</summary>
        public bool TakeHit(int damage, Vector3 from)
        {
            if (state == EnemyState.Dead) return false;

            hp -= damage;
            flashTimer = 0.1f;
            SetColor(Color.white);

            var push = transform.position - from;
            push.y = 0f;
            body.linearVelocity = push.normalized * 6f;

            if (hp <= 0)
            {
                hp = 0;
                state = EnemyState.Dead;
                return true;
            }
            state = EnemyState.Stagger;
            stateTimer = 0.3f;
            return false;
        }

        /// <summary>復活時に生きている敵を初期位置・満タンへ戻す。</summary>
        public virtual void ResetToHome()
        {
            hp = Stats.maxHp;
            state = IdleState;
            attackCooldown = 0f;
            stateTimer = 0f;
            body.linearVelocity = Vector3.zero;
            body.position = home;
            transform.position = home;
            SetColor(baseColor);
        }

        protected void MoveToward(Vector3 target, float speed)
        {
            var dir = target - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f)
            {
                Stop();
                return;
            }
            dir.Normalize();
            var v = body.linearVelocity;
            v.x = dir.x * speed;
            v.z = dir.z * speed;
            body.linearVelocity = v;
            body.MoveRotation(Quaternion.LookRotation(dir, Vector3.up));
        }

        protected void Stop()
        {
            if (body == null) return;
            var v = body.linearVelocity;
            v.x = 0f;
            v.z = 0f;
            body.linearVelocity = v;
        }

        void SetColor(Color c)
        {
            if (bodyRenderer != null) bodyRenderer.material.color = c;
        }

        protected static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}
