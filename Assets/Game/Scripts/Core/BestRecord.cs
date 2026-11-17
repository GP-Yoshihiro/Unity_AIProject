using System;
using System.Collections.Generic;
using UnityEngine;

namespace CryptEscape.Core
{
    /// <summary>ベストタイムと、そのときの区間タイム。JSON で PlayerPrefs に保存する。</summary>
    public class BestRecord
    {
        [Serializable]
        class Data
        {
            public float bestTime = -1f;
            public List<Split> splits = new List<Split>();
        }

        Data data = new Data();

        public bool HasRecord => data.bestTime >= 0f;
        public float BestTime => data.bestTime;
        public IReadOnlyList<Split> BestSplits => data.splits;

        /// <summary>今回のタイムがベストなら更新して true。</summary>
        public bool TryUpdate(float time, IReadOnlyList<Split> splits)
        {
            if (HasRecord && time >= data.bestTime) return false;

            data.bestTime = time;
            data.splits = new List<Split>(splits);
            return true;
        }

        /// <summary>ベストとの差（秒）。マイナスなら速い。記録がなければ 0。</summary>
        public float DiffFrom(float time) => HasRecord ? time - data.bestTime : 0f;

        public string ToJson() => JsonUtility.ToJson(data);

        public static BestRecord FromJson(string json)
        {
            var record = new BestRecord();
            if (string.IsNullOrEmpty(json)) return record;

            try
            {
                var parsed = JsonUtility.FromJson<Data>(json);
                if (parsed != null)
                {
                    parsed.splits ??= new List<Split>();
                    record.data = parsed;
                }
            }
            catch (ArgumentException)
            {
                // 壊れた保存データは「記録なし」として扱う
            }
            return record;
        }
    }
}
