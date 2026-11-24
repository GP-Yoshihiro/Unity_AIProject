using UnityEngine;
using UnityEngine.UI;

namespace MemoryMatch
{
    /// <summary>カード1枚の表示。裏の間は絵柄の情報を持たない。</summary>
    public sealed class CardView : MonoBehaviour
    {
        private static readonly Color BackColor = new Color(0.2f, 0.3f, 0.5f);
        private static readonly Color FrontColor = new Color(0.95f, 0.95f, 0.92f);
        private static readonly Color MatchedColor = new Color(0.65f, 0.85f, 0.65f);

        private static readonly string[] Symbols = { "●", "▲", "■", "◆", "★", "♥", "♠", "♣", "▼", "＋", "×", "♪" };
        private static readonly Color[] SymbolColors =
        {
            new Color(0.85f, 0.15f, 0.15f), new Color(0.1f, 0.5f, 0.15f), new Color(0.1f, 0.3f, 0.85f),
            new Color(0.55f, 0.15f, 0.7f), new Color(0.75f, 0.45f, 0f), new Color(0.85f, 0.2f, 0.5f),
            new Color(0.1f, 0.1f, 0.1f), new Color(0f, 0.45f, 0.45f), new Color(0.45f, 0.3f, 0.1f), new Color(0.35f, 0.35f, 0.85f), new Color(0.7f, 0.1f, 0.1f), new Color(0.2f, 0.4f, 0.2f),
        };

        private static readonly string[] Suits = { "♠", "♥", "♦", "♣" };
        private static readonly string[] Ranks = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
        private static readonly Color RedSuit = new Color(0.8f, 0.1f, 0.1f);
        private static readonly Color BlackSuit = new Color(0.1f, 0.1f, 0.1f);

        [SerializeField] private Image background;
        [SerializeField] private Text symbol;
        [SerializeField] private Text owner;

        private const float MotionSeconds = 0.45f;
        private float _bounceAge = float.MaxValue;
        private float _wiggleAge = float.MaxValue;

        public void ShowBack()
        {
            background.color = BackColor;
            symbol.text = "";
            owner.text = "";
        }

        public void ShowFront(int card, bool trump, bool matched, string ownerLabel)
        {
            background.color = matched ? MatchedColor : FrontColor;
            owner.text = ownerLabel;
            float unit = Mathf.Min(background.rectTransform.rect.width, background.rectTransform.rect.height);
            if (trump)
            {
                int suit = card / Ranks.Length;
                symbol.text = Suits[suit] + "\n" + Ranks[card % Ranks.Length];
                symbol.color = suit == 1 || suit == 2 ? RedSuit : BlackSuit;
                symbol.fontSize = Mathf.RoundToInt(unit * 0.42f);
            }
            else
            {
                symbol.text = Symbols[card];
                symbol.color = SymbolColors[card];
                symbol.fontSize = Mathf.RoundToInt(unit * 0.62f);
            }
        }

        /// <summary>めくったときに弾む。</summary>
        public void Bounce() => _bounceAge = 0f;

        /// <summary>ミスしたときに左右へ揺れる。</summary>
        public void Wiggle() => _wiggleAge = 0f;

        public void ResetMotion()
        {
            _bounceAge = _wiggleAge = float.MaxValue;
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
        }

        private void Update()
        {
            if (_bounceAge < MotionSeconds)
            {
                _bounceAge += Time.deltaTime;
                float k = Mathf.Clamp01(1f - _bounceAge / MotionSeconds);
                transform.localScale = Vector3.one * (1f + 0.18f * k * Mathf.Cos(_bounceAge * 22f));
                if (_bounceAge >= MotionSeconds) transform.localScale = Vector3.one;
            }
            if (_wiggleAge < MotionSeconds)
            {
                _wiggleAge += Time.deltaTime;
                float k = Mathf.Clamp01(1f - _wiggleAge / MotionSeconds);
                transform.localRotation = Quaternion.Euler(0f, 0f, 9f * k * Mathf.Sin(_wiggleAge * 40f));
                if (_wiggleAge >= MotionSeconds) transform.localRotation = Quaternion.identity;
            }
        }

        public void Place(Vector2 position, Vector2 size)
        {
            background.rectTransform.anchoredPosition = position;
            background.rectTransform.sizeDelta = size;
            symbol.rectTransform.sizeDelta = size;
            symbol.rectTransform.anchoredPosition = new Vector2(0f, size.y * 0.06f);
            owner.rectTransform.sizeDelta = new Vector2(size.x, 22f);
            owner.rectTransform.anchoredPosition = new Vector2(0f, -size.y / 2f + 11f);
            owner.fontSize = 18;
        }
    }
}
