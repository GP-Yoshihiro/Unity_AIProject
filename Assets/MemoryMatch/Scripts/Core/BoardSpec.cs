namespace MemoryMatch.Core
{
    /// <summary>盤面の種類。並び、ミス上限、カードの一覧を決める。</summary>
    public sealed class BoardSpec
    {
        public static readonly BoardSpec Easy = new BoardSpec(4, 3, 5);
        public static readonly BoardSpec Normal = new BoardSpec(4, 4, 10);
        public static readonly BoardSpec Hard = new BoardSpec(6, 4, 20);

        /// <summary>対戦用。ジョーカーなし52枚。番号は スート×13＋数字-1。ミス上限なし。</summary>
        public static readonly BoardSpec Trump = new BoardSpec(13, 4, 0, 13);

        private readonly int _rankCount;

        private BoardSpec(int columns, int rows, int missLimit, int rankCount = 0)
        {
            Columns = columns;
            Rows = rows;
            MissLimit = missLimit;
            _rankCount = rankCount;
        }

        public int Columns { get; }
        public int Rows { get; }
        public int MissLimit { get; }
        public int CardCount => Columns * Rows;
        public int PairCount => CardCount / 2;

        public bool IsTrump => _rankCount > 0;

        /// <summary>同じ値のカード2枚がペアになる。</summary>
        public int KeyOf(int card) => IsTrump ? card % _rankCount : card;

        /// <summary>シャッフル前のカード。</summary>
        public int[] CreateCards()
        {
            var cards = new int[CardCount];
            for (int i = 0; i < cards.Length; i++) cards[i] = IsTrump ? i : i / 2;
            return cards;
        }
    }
}
