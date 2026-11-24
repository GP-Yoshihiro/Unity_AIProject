using System;
using System.Collections.Generic;
using System.Linq;

namespace MemoryMatch.Core
{
    public enum CardFace { Down, Up, Matched }

    public enum GamePhase { PickFirst, PickSecond, ShowingMiss, Cleared, GameOver }

    /// <summary>神経衰弱のルール。Unity に依存しない。</summary>
    public sealed class MemoryLogic
    {
        public const int MatchScore = 100;
        public const int MaxMultiplier = 5;
        public const int MissPenalty = 10;

        private int[] _deck;
        private readonly CardFace[] _faces;
        private readonly int[] _owners;
        private readonly int[] _pairs;
        private readonly int[] _scores;
        private readonly int[] _combos;
        private readonly List<int> _turned = new List<int>();

        public MemoryLogic(int[] deck, int playerCount = 1, BoardSpec spec = null)
        {
            Spec = spec ?? BoardSpec.Easy;
            _faces = new CardFace[Spec.CardCount];
            _owners = new int[Spec.CardCount];
            PlayerCount = playerCount;
            _pairs = new int[playerCount];
            _scores = new int[playerCount];
            _combos = new int[playerCount];
            Restart(deck);
        }

        public BoardSpec Spec { get; }
        public int PlayerCount { get; }
        public int CurrentPlayer { get; private set; }
        public int OwnerAt(int index) => _faces[index] == CardFace.Matched ? _owners[index] : -1;
        public bool IsVersus => PlayerCount > 1;

        public int ScoreOf(int player) => _scores[player];
        public int ComboOf(int player) => _combos[player];

        /// <summary>スコアの高い順の順位（1〜）。同点は同じ順位。</summary>
        public int RankOf(int player) => 1 + _scores.Count(s => s > _scores[player]);

        public int PairsOf(int player) => _pairs[player];

        public int Matched { get; private set; }
        public int Misses { get; private set; }
        /// <summary>手番の人のスコアと連続数（1人用では自分の値）。</summary>
        public int Score => _scores[CurrentPlayer];
        public int Combo => _combos[CurrentPlayer];

        public GamePhase Phase { get; private set; }
        public int Cursor { get; private set; }

        private bool IsOver => Phase == GamePhase.Cleared || Phase == GamePhase.GameOver;

        public void MoveCursor(int dx, int dy)
        {
            if (IsOver) return;
            int x = Math.Clamp(Cursor % Spec.Columns + dx, 0, Spec.Columns - 1);
            int y = Math.Clamp(Cursor / Spec.Columns + dy, 0, Spec.Rows - 1);
            Cursor = y * Spec.Columns + x;
        }

        public void Restart(int[] deck)
        {
            _deck = (int[])deck.Clone();
            Array.Clear(_faces, 0, _faces.Length);
            Array.Clear(_pairs, 0, _pairs.Length);
            Array.Clear(_scores, 0, _scores.Length);
            Array.Clear(_combos, 0, _combos.Length);
            CurrentPlayer = 0;
            _turned.Clear();
            Matched = Misses = Cursor = 0;
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
            int player = CurrentPlayer;
            if (Spec.KeyOf(_deck[_turned[0]]) == Spec.KeyOf(_deck[_turned[1]]))
            {
                foreach (int i in _turned)
                {
                    _faces[i] = CardFace.Matched;
                    _owners[i] = player;
                }
                _turned.Clear();
                _pairs[player]++;
                _combos[player]++;
                _scores[player] += MatchScore * Math.Min(_combos[player], MaxMultiplier);
                Matched++;
                Phase = Matched == Spec.PairCount ? GamePhase.Cleared : GamePhase.PickFirst;
            }
            else
            {
                Misses++;
                _combos[player] = 0;
                _scores[player] = Math.Max(0, _scores[player] - MissPenalty);
                Phase = !IsVersus && Misses == Spec.MissLimit ? GamePhase.GameOver : GamePhase.ShowingMiss;
            }
        }

        private void TurnBack()
        {
            foreach (int i in _turned) _faces[i] = CardFace.Down;
            _turned.Clear();
            CurrentPlayer = (CurrentPlayer + 1) % PlayerCount;
            Phase = GamePhase.PickFirst;
        }

        /// <summary>見えているカードの判定キー。裏なら -1。</summary>
        public int VisibleKeyAt(int index) => _faces[index] == CardFace.Down ? -1 : Spec.KeyOf(_deck[index]);

        public CardFace FaceAt(int index) => _faces[index];

        /// <summary>見えているカードの絵柄。裏なら -1（覗けない）。</summary>
        public int VisibleSymbolAt(int index) => _faces[index] == CardFace.Down ? -1 : _deck[index];
    }
}
