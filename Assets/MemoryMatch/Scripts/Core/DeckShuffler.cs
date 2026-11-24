using System;

namespace MemoryMatch.Core
{
    /// <summary>配置を作る。乱数列を1本持ち、同じシードなら呼ぶ順に同じ配置を返す。</summary>
    public sealed class DeckShuffler
    {
        private readonly Random _random;

        public DeckShuffler(int seed) => _random = new Random(seed);

        public int[] Next(BoardSpec spec)
        {
            var deck = spec.CreateCards();
            for (int i = deck.Length - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (deck[i], deck[j]) = (deck[j], deck[i]);
            }
            return deck;
        }
    }
}
