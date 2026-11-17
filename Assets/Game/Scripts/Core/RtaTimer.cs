using System;
using System.Collections.Generic;

namespace CryptEscape.Core
{
    [Serializable]
    public struct Split
    {
        public string name;
        public float time;

        public Split(string name, float time)
        {
            this.name = name;
            this.time = time;
        }
    }

    /// <summary>RTA の経過タイマーと区間タイム。Playing と Dying の間だけ進む。</summary>
    public class RtaTimer
    {
        public const string EscapeSplit = "Escape";

        readonly List<Split> splits = new List<Split>();

        public float Elapsed { get; private set; }
        public bool Finished { get; private set; }
        public IReadOnlyList<Split> Splits => splits;

        public void Tick(float unscaledDeltaTime, GameFlow flow)
        {
            if (Finished || !GameFlowMachine.TimerRuns(flow)) return;
            Elapsed += unscaledDeltaTime;
        }

        /// <summary>区間を記録する。同じ名前は1回だけ。</summary>
        public bool Split(string name)
        {
            if (Finished) return false;
            foreach (var s in splits)
                if (s.name == name) return false;

            splits.Add(new Split(name, Elapsed));
            return true;
        }

        public void Finish()
        {
            if (Finished) return;
            Split(EscapeSplit);
            Finished = true;
        }

        public void Reset()
        {
            Elapsed = 0f;
            Finished = false;
            splits.Clear();
        }

        public static string Format(float seconds)
        {
            if (seconds < 0f) seconds = 0f;
            int hundredths = (int)Math.Floor(seconds * 100f + 1e-3f);
            int minutes = hundredths / 6000;
            int secs = hundredths / 100 % 60;
            int cs = hundredths % 100;
            return $"{minutes:00}:{secs:00}.{cs:00}";
        }
    }
}
