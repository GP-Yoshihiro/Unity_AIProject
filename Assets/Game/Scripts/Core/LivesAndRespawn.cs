namespace CryptEscape.Core
{
    public enum DeathOutcome
    {
        Respawn,
        GameOver,
    }

    /// <summary>残機と復活地点のルール。残機3は今のプレイヤーを含み、3回目に倒れるとゲームオーバー。</summary>
    public class LivesAndRespawn
    {
        readonly RunState run;

        public LivesAndRespawn(RunState run) => this.run = run;

        /// <summary>祈りの台座で保存する。体力は変えない。</summary>
        public void Save(int savePointId) => run.RespawnPointId = savePointId;

        /// <summary>ハートが0になったときに呼ぶ。残機を1減らし、続けられるかを返す。</summary>
        public DeathOutcome OnPlayerDied()
        {
            if (run.Lives > 0) run.Lives--;
            return run.Lives > 0 ? DeathOutcome.Respawn : DeathOutcome.GameOver;
        }
    }
}
