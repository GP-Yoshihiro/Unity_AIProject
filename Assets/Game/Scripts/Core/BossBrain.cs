using System;

namespace CryptEscape.Core
{
    public enum BossState
    {
        Dormant,
        Idle,
        ChargeTell,
        Charge,
        Recover,
        AoeTell,
        Defeated,
    }

    /// <summary>Tick で起きた出来事。BossController が見た目と当たり判定に反映する。</summary>
    public enum BossEvent
    {
        None,
        ChargeTellStarted,
        ChargeStarted,
        ChargeEnded,
        AoeTellStarted,
        AoeHit,
    }

    [Serializable]
    public struct BossTimings
    {
        public int maxHp;
        public float idle;
        public float chargeTell;
        public float chargeMaxTime;
        public float chargeRecover;
        public float aoeTell;
        public float aoeRecover;
    }

    /// <summary>
    /// ボスの行動の状態遷移。予兆 → 攻撃 → 隙 を、突進と範囲攻撃で交互に繰り返す。
    /// Idle → ChargeTell → Charge → Recover → AoeTell →(AoeHit)→ Recover → ChargeTell ...
    /// </summary>
    public class BossBrain
    {
        readonly BossTimings t;
        float stateTime;
        bool nextIsCharge;

        public BossState State { get; private set; }
        public int Hp { get; private set; }
        public int MaxHp => t.maxHp;
        public bool IsActive => State != BossState.Dormant && State != BossState.Defeated;

        public BossBrain(BossTimings timings)
        {
            t = timings;
            Reset();
        }

        public void Activate()
        {
            if (State != BossState.Dormant) return;
            Enter(BossState.Idle);
            nextIsCharge = true;
        }

        public BossEvent Tick(float deltaTime)
        {
            if (!IsActive) return BossEvent.None;

            stateTime += deltaTime;
            switch (State)
            {
                case BossState.Idle:
                    if (stateTime >= t.idle) return StartNextAttack();
                    break;
                case BossState.ChargeTell:
                    if (stateTime >= t.chargeTell)
                    {
                        Enter(BossState.Charge);
                        return BossEvent.ChargeStarted;
                    }
                    break;
                case BossState.Charge:
                    if (stateTime >= t.chargeMaxTime)
                    {
                        EndCharge();
                        return BossEvent.ChargeEnded;
                    }
                    break;
                case BossState.AoeTell:
                    if (stateTime >= t.aoeTell)
                    {
                        Enter(BossState.Recover);
                        recoverDuration = t.aoeRecover;
                        return BossEvent.AoeHit;
                    }
                    break;
                case BossState.Recover:
                    if (stateTime >= recoverDuration) return StartNextAttack();
                    break;
            }
            return BossEvent.None;
        }

        float recoverDuration;

        /// <summary>壁に当たったときなど、突進を早めに終える。</summary>
        public void EndCharge()
        {
            if (State != BossState.Charge) return;
            Enter(BossState.Recover);
            recoverDuration = t.chargeRecover;
        }

        /// <summary>ダメージを与える。この攻撃で撃破したときだけ true。</summary>
        public bool TakeDamage(int amount)
        {
            if (!IsActive || amount <= 0) return false;

            Hp = Math.Max(0, Hp - amount);
            if (Hp > 0) return false;

            Enter(BossState.Defeated);
            return true;
        }

        public void Reset()
        {
            Hp = t.maxHp;
            nextIsCharge = true;
            Enter(BossState.Dormant);
        }

        BossEvent StartNextAttack()
        {
            var charge = nextIsCharge;
            nextIsCharge = !nextIsCharge;
            Enter(charge ? BossState.ChargeTell : BossState.AoeTell);
            return charge ? BossEvent.ChargeTellStarted : BossEvent.AoeTellStarted;
        }

        void Enter(BossState next)
        {
            State = next;
            stateTime = 0f;
        }
    }
}
