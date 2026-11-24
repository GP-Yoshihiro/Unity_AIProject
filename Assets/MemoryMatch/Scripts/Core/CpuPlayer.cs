using System;
using System.Collections.Generic;
using System.Linq;

namespace MemoryMatch.Core
{
    /// <summary>CPU。公開されたカードの直近4枚の記憶だけで選ぶ。配置そのものは受け取らない。</summary>
    public sealed class CpuPlayer
    {
        public const int MemorySize = 4;

        private readonly Random _random;
        private readonly List<(int index, int symbol)> _memory = new List<(int index, int symbol)>();

        public CpuPlayer(int seed) => _random = new Random(seed);

        public int[] RememberedCards => _memory.Select(m => m.index).ToArray();

        public void Reset() => _memory.Clear();

        /// <summary>誰かがめくって見えたカードを覚える。</summary>
        public void Observe(int index, int symbol)
        {
            _memory.RemoveAll(m => m.index == index);
            _memory.Add((index, symbol));
            if (_memory.Count > MemorySize) _memory.RemoveAt(0);
        }

        public int ChooseFirst(int[] selectable)
        {
            var known = _memory.Where(m => selectable.Contains(m.index)).ToArray();
            foreach (var m in known)
            {
                if (known.Any(o => o.index != m.index && o.symbol == m.symbol)) return m.index;
            }
            return Explore(selectable);
        }

        public int ChooseSecond(int[] selectable, int firstSymbol)
        {
            foreach (var m in _memory)
            {
                if (m.symbol == firstSymbol && selectable.Contains(m.index)) return m.index;
            }
            return Explore(selectable);
        }

        private int Explore(int[] selectable)
        {
            var unknown = selectable.Except(RememberedCards).ToArray();
            var from = unknown.Length > 0 ? unknown : selectable;
            return from[_random.Next(from.Length)];
        }
    }
}
