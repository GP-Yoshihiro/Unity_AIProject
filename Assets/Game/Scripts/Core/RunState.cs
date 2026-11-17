using System.Collections.Generic;

namespace CryptEscape.Core
{
    /// <summary>
    /// 1回のプレイの進行（data-model の RunState）。
    /// 復活では保持し、全初期化（ゲームオーバー後・即リスタート）でだけ初期値へ戻す。
    /// </summary>
    public class RunState
    {
        public const int CoinsToOpenDoor = 5;
        public const int StartPointId = -1;

        readonly HashSet<int> coins = new HashSet<int>();
        readonly HashSet<int> defeatedEnemies = new HashSet<int>();
        readonly HashSet<int> usedPotions = new HashSet<int>();

        public int StartLives { get; }
        public int Lives { get; set; }
        public int RespawnPointId { get; set; }
        public int CoinCount => coins.Count;
        public bool BossDoorOpen { get; private set; }
        public bool BossDefeated { get; private set; }
        public bool ExitOpen => BossDefeated;

        public RunState(int startLives)
        {
            StartLives = startLives;
            FullReset();
        }

        public bool TryCollectCoin(int coinId)
        {
            if (!coins.Add(coinId)) return false;
            if (coins.Count >= CoinsToOpenDoor) BossDoorOpen = true;
            return true;
        }

        public bool HasCoin(int coinId) => coins.Contains(coinId);

        public void DefeatBoss() => BossDefeated = true;

        public void MarkEnemyDefeated(int enemyId) => defeatedEnemies.Add(enemyId);
        public bool IsEnemyDefeated(int enemyId) => defeatedEnemies.Contains(enemyId);

        public void MarkPotionUsed(int potionId) => usedPotions.Add(potionId);
        public bool IsPotionUsed(int potionId) => usedPotions.Contains(potionId);

        public void FullReset()
        {
            coins.Clear();
            defeatedEnemies.Clear();
            usedPotions.Clear();
            Lives = StartLives;
            RespawnPointId = StartPointId;
            BossDoorOpen = false;
            BossDefeated = false;
        }
    }
}
