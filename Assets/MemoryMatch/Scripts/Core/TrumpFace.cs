using System;
using System.Collections.Generic;

namespace MemoryMatch.Core
{
    /// <summary>トランプ1枚の面。番号は スート×13＋数字-1。</summary>
    public sealed class TrumpFace
    {
        private const float L = -1f, R = 1f, C = 0f;
        private const float Third = 1f / 3f;

        private static readonly string[] Suits = { "♠", "♥", "♦", "♣" };
        private static readonly string[] Ranks = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };

        /// <summary>A〜10のマークの位置。x・y とも -1〜1（上が +1）。</summary>
        private static readonly (float x, float y)[][] PipTable =
        {
            new[] { (C, 0f) },
            new[] { (C, 1f), (C, -1f) },
            new[] { (C, 1f), (C, 0f), (C, -1f) },
            new[] { (L, 1f), (R, 1f), (L, -1f), (R, -1f) },
            new[] { (L, 1f), (R, 1f), (C, 0f), (L, -1f), (R, -1f) },
            new[] { (L, 1f), (R, 1f), (L, 0f), (R, 0f), (L, -1f), (R, -1f) },
            new[] { (L, 1f), (R, 1f), (C, 0.5f), (L, 0f), (R, 0f), (L, -1f), (R, -1f) },
            new[] { (L, 1f), (R, 1f), (C, 0.5f), (L, 0f), (R, 0f), (C, -0.5f), (L, -1f), (R, -1f) },
            new[] { (L, 1f), (R, 1f), (L, Third), (R, Third), (C, 0f), (L, -Third), (R, -Third), (L, -1f), (R, -1f) },
            new[] { (L, 1f), (R, 1f), (C, 2f * Third), (L, Third), (R, Third), (L, -Third), (R, -Third), (C, -2f * Third), (L, -1f), (R, -1f) },
        };

        private readonly int _suit;
        private readonly int _rank;

        public TrumpFace(int card)
        {
            _suit = card / Ranks.Length;
            _rank = card % Ranks.Length;
        }

        public string Rank => Ranks[_rank];
        public string Suit => Suits[_suit];
        public bool IsRed => _suit == 1 || _suit == 2;
        public bool IsCourt => _rank >= PipTable.Length;
        public IReadOnlyList<(float x, float y)> Pips => IsCourt ? Array.Empty<(float, float)>() : PipTable[_rank];
    }
}
