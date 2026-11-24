using System;
using System.Collections.Generic;

namespace MemoryMatch.Core
{
    public enum CardFace { Down, Up, Matched }

    public enum GamePhase { PickFirst, PickSecond, ShowingMiss, Cleared, GameOver }

    /// <summary>神経衰弱のルール。Unity に依存しない。</summary>
    public sealed class MemoryLogic
    {
        public const int Columns = 4;
        public const int Rows = 3;
        public const int CardCount = Columns * Rows;
        public const int PairCount = CardCount / 2;

        public const int MatchScore = 100;
        public const int MissPenalty = 10;
        public const int MissLimit = 5;

        private int[] _deck;
        private readonly CardFace[] _faces = new CardFace[CardCount];
        private readonly List<int> _turned = new List<int>();

        public MemoryLogic(int[] deck) => Restart(deck);

        public int Matched { get; private set; }
        public int Misses { get; private set; }
        public int Score { get; private set; }

        public GamePhase Phase { get; private set; }
        public int Cursor { get; private set; }

        private bool IsOver => Phase == GamePhase.Cleared || Phase == GamePhase.GameOver;

        public void MoveCursor(int dx, int dy)
        {
            if (IsOver) return;
            int x = Math.Clamp(Cursor % Columns + dx, 0, Columns - 1);
            int y = Math.Clamp(Cursor / Columns + dy, 0, Rows - 1);
            Cursor = y * Columns + x;
        }

        public void Restart(int[] deck)
        {
            _deck = (int[])deck.Clone();
            Array.Clear(_faces, 0, _faces.Length);
            _turned.Clear();
            Matched = Misses = Score = Cursor = 0;
            Phase = GamePhase.PickFirst;
        }

        public void PressSpace()
        {
            if (IsOver) return;
            if (Phase == GamePhase.ShowingMiss) { TurnBack(); return; }
            if (_faces[Cursor] != CardFace.Down) return;
            _faces[Cursor] = CardFace.Up;
            _turned.Add(Cursor);
            if (_turned.Count == 2) Judge();
            else Phase = GamePhase.PickSecond;
        }

        private void Judge()
        {
            if (_deck[_turned[0]] == _deck[_turned[1]])
            {
                foreach (int i in _turned) _faces[i] = CardFace.Matched;
                _turned.Clear();
                Matched++;
                Score += MatchScore;
                Phase = Matched == PairCount ? GamePhase.Cleared : GamePhase.PickFirst;
            }
            else
            {
                Misses++;
                Score = Math.Max(0, Score - MissPenalty);
                Phase = Misses == MissLimit ? GamePhase.GameOver : GamePhase.ShowingMiss;
            }
        }

        private void TurnBack()
        {
            foreach (int i in _turned) _faces[i] = CardFace.Down;
            _turned.Clear();
            Phase = GamePhase.PickFirst;
        }

        public CardFace FaceAt(int index) => _faces[index];

        /// <summary>見えているカードの絵柄。裏なら -1（覗けない）。</summary>
        public int VisibleSymbolAt(int index) => _faces[index] == CardFace.Down ? -1 : _deck[index];
    }
}
