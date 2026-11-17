using System.Collections.Generic;

namespace BallCollect
{
    /// <summary>
    /// 回収数・クリア判定・初期化のルール。Unity に依存しないので EditMode テストから直接使える。
    /// </summary>
    public class BallCollectRules
    {
        readonly HashSet<int> collectedIds = new HashSet<int>();

        public int TotalItems { get; }
        public int CollectedCount => collectedIds.Count;
        public bool IsCleared { get; private set; }

        public BallCollectRules(int totalItems)
        {
            TotalItems = totalItems;
            Reset();
        }

        /// <summary>
        /// アイテムを回収する。初回だけ true を返し、同じアイテムは二重に数えない。
        /// </summary>
        public bool TryCollect(int itemId)
        {
            if (IsCleared) return false;
            if (!collectedIds.Add(itemId)) return false;

            if (collectedIds.Count >= TotalItems) IsCleared = true;
            return true;
        }

        public void Reset()
        {
            collectedIds.Clear();
            IsCleared = false;
        }
    }
}
