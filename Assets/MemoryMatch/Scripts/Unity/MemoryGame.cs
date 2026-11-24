using System;
using System.Text;
using MemoryMatch.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MemoryMatch
{
    /// <summary>入力をルールへ渡し、結果を表示する。公開プロパティは CLI での状態取得に使う。</summary>
    public sealed class MemoryGame : MonoBehaviour
    {
        [SerializeField] private bool useFixedSeed;
        [SerializeField] private int fixedSeed = 1;
        [SerializeField] private CardView[] cards;
        [SerializeField] private RectTransform cursorFrame;
        [SerializeField] private Text matchedText;
        [SerializeField] private Text missesText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text resultText;

        private DeckShuffler _shuffler;
        private MemoryLogic _logic;

        public string Phase => _logic == null ? "" : _logic.Phase.ToString();
        public int Matched => _logic?.Matched ?? 0;
        public int Misses => _logic?.Misses ?? 0;
        public int Score => _logic?.Score ?? 0;
        public int Cursor => _logic?.Cursor ?? 0;

        /// <summary>00〜11の表裏。裏は「--」、表は絵柄の文字、成立済みは「*」付き。裏の絵柄は出さない。</summary>
        public string Cards => DescribeCards();

        private void Awake()
        {
            Application.runInBackground = true;
            _shuffler = new DeckShuffler(useFixedSeed ? fixedSeed : Guid.NewGuid().GetHashCode());
            _logic = new MemoryLogic(_shuffler.Next());
            Render();
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            bool changed = true;
            if (kb.rKey.wasPressedThisFrame) _logic.Restart(_shuffler.Next());
            else if (kb.leftArrowKey.wasPressedThisFrame) _logic.MoveCursor(-1, 0);
            else if (kb.rightArrowKey.wasPressedThisFrame) _logic.MoveCursor(1, 0);
            else if (kb.upArrowKey.wasPressedThisFrame) _logic.MoveCursor(0, -1);
            else if (kb.downArrowKey.wasPressedThisFrame) _logic.MoveCursor(0, 1);
            else if (kb.spaceKey.wasPressedThisFrame) _logic.PressSpace();
            else changed = false;

            if (changed) Render();
        }

        private void Render()
        {
            for (int i = 0; i < cards.Length; i++)
            {
                if (_logic.FaceAt(i) == CardFace.Down) cards[i].ShowBack();
                else cards[i].ShowFront(_logic.VisibleSymbolAt(i), _logic.FaceAt(i) == CardFace.Matched);
            }
            cursorFrame.anchoredPosition = ((RectTransform)cards[_logic.Cursor].transform).anchoredPosition;
            matchedText.text = $"PAIRS {_logic.Matched}/{MemoryLogic.PairCount}";
            missesText.text = $"MISS {_logic.Misses}/{MemoryLogic.MissLimit}";
            scoreText.text = $"SCORE {_logic.Score}";
            bool over = _logic.Phase == GamePhase.Cleared || _logic.Phase == GamePhase.GameOver;
            resultText.text = _logic.Phase == GamePhase.Cleared ? "CLEAR!" : "GAME OVER";
            resultText.gameObject.SetActive(over);
        }

        private string DescribeCards()
        {
            if (_logic == null) return "";
            var sb = new StringBuilder();
            for (int i = 0; i < MemoryLogic.CardCount; i++)
            {
                if (i > 0) sb.Append(' ');
                var face = _logic.FaceAt(i);
                if (face == CardFace.Down) sb.Append("--");
                else sb.Append((char)('A' + _logic.VisibleSymbolAt(i))).Append(face == CardFace.Matched ? "*" : "");
            }
            return sb.ToString();
        }
    }
}
