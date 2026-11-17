using System;
using System.Collections.Generic;

namespace BallCollect
{
    /// <summary>
    /// 回収数・クリア判定・制限時間・初期化のルール。Unity に依存しないので EditMode テストから直接使える。
    /// 1回の更新では TryCollect を先に、Tick を後に呼ぶ。これにより5個目の回収と時間切れが
    /// 同じ更新で起きた場合は回収（CLEAR）が優先される。
    /// </summary>
    public class BallCollectRules
    {
        public const float DefaultTimeLimit = 30f;

        readonly HashSet<int> collectedIds = new HashSet<int>();

        public int TotalItems { get; }
        public float TimeLimit { get; }
        public int CollectedCount => collectedIds.Count;
        public bool IsCleared { get; private set; }
        public bool IsTimeUp { get; private set; }
        public float RemainingTime { get; private set; }

        /// <summary>CLEAR でも TIME UP でもない間だけ true。</summary>
        public bool IsPlaying => !IsCleared && !IsTimeUp;

        public BallCollectRules(int totalItems, float timeLimit = DefaultTimeLimit)
        {
            TotalItems = totalItems;
            TimeLimit = timeLimit;
            Reset();
        }

        /// <summary>
        /// アイテムを回収する。初回だけ true を返し、同じアイテムは二重に数えない。
        /// CLEAR後・TIME UP後は回収しない。
        /// </summary>
        public bool TryCollect(int itemId)
        {
            if (!IsPlaying) return false;
            if (!collectedIds.Add(itemId)) return false;

            if (collectedIds.Count >= TotalItems) IsCleared = true;
            return true;
        }

        /// <summary>残り時間を進める。CLEAR後は時計が止まり、TIME UP に変わらない。</summary>
        public void Tick(float deltaTime)
        {
            if (!IsPlaying) return;

            RemainingTime = Math.Max(0f, RemainingTime - deltaTime);
            if (RemainingTime <= 0f) IsTimeUp = true;
        }

        public void Reset()
        {
            collectedIds.Clear();
            IsCleared = false;
            IsTimeUp = false;
            RemainingTime = TimeLimit;
        }
    }
}
