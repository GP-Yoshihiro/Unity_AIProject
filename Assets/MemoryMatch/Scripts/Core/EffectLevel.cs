using System;

namespace MemoryMatch.Core
{
    /// <summary>コンボ数に応じた演出の強さ。段階0は演出なし、5が最大。</summary>
    public readonly struct EffectLevel
    {
        public const int Max = 5;

        private static readonly EffectLevel[] Levels =
        {
            new EffectLevel(0, 0, 0, 0f, 0f),
            new EffectLevel(1, 12, 44, 2f, 0.04f),
            new EffectLevel(2, 28, 60, 6f, 0.1f),
            new EffectLevel(3, 54, 78, 11f, 0.17f),
            new EffectLevel(4, 90, 98, 17f, 0.25f),
            new EffectLevel(5, 150, 120, 24f, 0.35f),
        };

        private EffectLevel(int level, int particles, int textSize, float shake, float flash)
        {
            Level = level;
            Particles = particles;
            TextSize = textSize;
            Shake = shake;
            Flash = flash;
        }

        public int Level { get; }
        public int Particles { get; }
        public int TextSize { get; }
        public float Shake { get; }
        public float Flash { get; }

        public static EffectLevel For(int combo) => Levels[Math.Clamp(combo, 0, Max)];
    }
}
