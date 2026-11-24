using System;

namespace MemoryMatch.Core
{
    public enum GameMode { Solo, VsCpu, VsHuman }

    /// <summary>開始前のモード・難易度・人数の選択。</summary>
    public sealed class MenuState
    {
        public const int MinPlayers = 2;
        public const int MaxPlayers = 4;

        private static readonly BoardSpec[] Difficulties = { BoardSpec.Easy, BoardSpec.Normal, BoardSpec.Hard };
        private static readonly string[] DifficultyNames = { "EASY", "NORMAL", "HARD" };

        private int _difficulty;
        private int _versusPlayers = MinPlayers;

        public GameMode Mode { get; private set; }
        public int PlayerCount => Mode == GameMode.Solo ? 1 : _versusPlayers;

        public BoardSpec Spec => Mode == GameMode.Solo ? Difficulties[_difficulty] : BoardSpec.Trump;
        public string DifficultyName => DifficultyNames[_difficulty];

        public bool IsCpu(int player) => Mode == GameMode.VsCpu && player > 0;

        public void Move(int dx, int dy)
        {
            Mode = (GameMode)Math.Clamp((int)Mode + dy, (int)GameMode.Solo, (int)GameMode.VsHuman);
            if (Mode == GameMode.Solo) _difficulty = Math.Clamp(_difficulty + dx, 0, Difficulties.Length - 1);
            else _versusPlayers = Math.Clamp(_versusPlayers + dx, MinPlayers, MaxPlayers);
        }
    }
}
