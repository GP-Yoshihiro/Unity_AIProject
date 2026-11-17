namespace CryptEscape.Core
{
    public enum GameFlow
    {
        Title,
        Playing,
        Paused,
        Dying,
        GameOver,
        Cleared,
    }

    /// <summary>画面の状態遷移（data-model の GameFlow）。不正な遷移は false を返して何もしない。</summary>
    public class GameFlowMachine
    {
        public GameFlow Current { get; private set; } = GameFlow.Title;

        public static bool TimerRuns(GameFlow flow) => flow == GameFlow.Playing || flow == GameFlow.Dying;

        public bool StartGame() => Move(GameFlow.Title, GameFlow.Playing);
        public bool Pause() => Move(GameFlow.Playing, GameFlow.Paused);
        public bool Resume() => Move(GameFlow.Paused, GameFlow.Playing);
        public bool BeginDying() => Move(GameFlow.Playing, GameFlow.Dying);
        public bool Clear() => Move(GameFlow.Playing, GameFlow.Cleared);

        public bool FinishDying(bool hasLivesLeft) =>
            Move(GameFlow.Dying, hasLivesLeft ? GameFlow.Playing : GameFlow.GameOver);

        /// <summary>タイトル以外のどの状態からでも、最初からやり直して Playing にする。</summary>
        public bool Restart()
        {
            if (Current == GameFlow.Title) return false;
            Current = GameFlow.Playing;
            return true;
        }

        public bool ToTitle()
        {
            if (Current == GameFlow.Title) return false;
            Current = GameFlow.Title;
            return true;
        }

        bool Move(GameFlow from, GameFlow to)
        {
            if (Current != from) return false;
            Current = to;
            return true;
        }
    }
}
