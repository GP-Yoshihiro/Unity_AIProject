using System;

namespace MemoryMatch.Core
{
    public enum GameMode { Solo, VsCpu, VsHuman }

    /// <summary>開始前のモードと人数の選択。</summary>
    public sealed class MenuState
    {
        public const int MinPlayers = 2;
        public const int MaxPlayers = 4;

        private int _versusPlayers = MinPlayers;

        public GameMode Mode { get; private set; }
        public int PlayerCount => Mode == GameMode.Solo ? 1 : _versusPlayers;

        public bool IsCpu(int player) => Mode == GameMode.VsCpu && player > 0;

        public void Move(int dx, int dy)
        {
            Mode = (GameMode)Math.Clamp((int)Mode + dy, (int)GameMode.Solo, (int)GameMode.VsHuman);
            if (Mode != GameMode.Solo) _versusPlayers = Math.Clamp(_versusPlayers + dx, MinPlayers, MaxPlayers);
        }
    }
}
