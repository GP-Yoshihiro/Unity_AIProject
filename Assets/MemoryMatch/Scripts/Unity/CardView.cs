using System.Collections.Generic;
using MemoryMatch.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryMatch
{
    /// <summary>カード1枚の表示。裏の間は絵柄の情報を持たない。</summary>
    public sealed class CardView : MonoBehaviour
    {
        private static readonly Color MatchedColor = new Color(0.86f, 0.84f, 0.74f);

        private static readonly string[] Symbols = { "●", "▲", "■", "◆", "★", "♥", "♠", "♣", "▼", "＋", "×", "♪" };
        private static readonly Color[] SymbolColors =
        {
            new Color(0.85f, 0.15f, 0.15f), new Color(0.1f, 0.5f, 0.15f), new Color(0.1f, 0.3f, 0.85f),
            new Color(0.55f, 0.15f, 0.7f), new Color(0.75f, 0.45f, 0f), new Color(0.85f, 0.2f, 0.5f),
            new Color(0.1f, 0.1f, 0.1f), new Color(0f, 0.45f, 0.45f), new Color(0.45f, 0.3f, 0.1f), new Color(0.35f, 0.35f, 0.85f), new Color(0.7f, 0.1f, 0.1f), new Color(0.2f, 0.4f, 0.2f),
        };

        private static readonly Color RedSuit = new Color(0.8f, 0.06f, 0.1f);
        private static readonly Color BlackSuit = new Color(0.08f, 0.08f, 0.1f);

        private const float FlipSeconds = 0.16f;
        private const float MotionSeconds = 0.45f;

        [SerializeField] private Image background;
        [SerializeField] private Text symbol;
        [SerializeField] private Text owner;

        private readonly List<Text> _pips = new List<Text>();
        private Image _court;
        private Image _chip;
        private Text _topIndex;
        private Text _bottomIndex;
        private Vector2 _size;
        private float _bounceAge = float.MaxValue;
        private float _wiggleAge = float.MaxValue;

        private void Ensure()
        {
            if (_court != null) return;
            var shadow = background.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
            shadow.effectDistance = new Vector2(3f, -4f);

            _court = UiBuilder.AddImage("Court", transform, ArtKind.CourtFrame, Color.white);
            symbol.transform.SetAsLastSibling();
            symbol.fontStyle = FontStyle.Bold;
            symbol.raycastTarget = false;
            _topIndex = UiBuilder.AddText("TopIndex", transform, TextAnchor.UpperCenter);
            _bottomIndex = UiBuilder.AddText("BottomIndex", transform, TextAnchor.UpperCenter);
            _bottomIndex.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 180f);
            _topIndex.lineSpacing = _bottomIndex.lineSpacing = 0.8f;
            _chip = UiBuilder.AddImage("OwnerChip", transform, ArtKind.Chip, Color.white);
            owner.transform.SetParent(_chip.transform, false);
            owner.color = Color.white;
            owner.horizontalOverflow = HorizontalWrapMode.Overflow;
            owner.verticalOverflow = VerticalWrapMode.Overflow;
            var outline = owner.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
        }

        public void ShowBack()
        {
            Ensure();
            ArtImage.Apply(background, ArtKind.CardBack);
            background.color = Color.white;
            ClearFace();
        }

        private void ClearFace()
        {
            symbol.text = _topIndex.text = _bottomIndex.text = owner.text = "";
            _court.gameObject.SetActive(false);
            _chip.gameObject.SetActive(false);
            foreach (var pip in _pips)
            {
                pip.text = "";
                pip.gameObject.SetActive(false);
            }
        }

        public void ShowFront(int card, bool trump, bool matched, string ownerLabel, Color ownerColor)
        {
            Ensure();
            ClearFace();
            ArtImage.Apply(background, ArtKind.CardFront);
            background.color = matched ? MatchedColor : Color.white;

            if (trump) ShowTrump(new TrumpFace(card));
            else
            {
                symbol.text = Symbols[card];
                symbol.fontSize = Mathf.RoundToInt(_size.x * 0.8f);
                SetIndex(Symbols[card], SymbolColors[card], 0.18f);
            }

            if (ownerLabel.Length > 0)
            {
                _chip.gameObject.SetActive(true);
                _chip.transform.SetAsLastSibling();
                _chip.color = ownerColor;
                owner.text = ownerLabel;
            }
        }

        private void ShowTrump(TrumpFace face)
        {
            var color = face.IsRed ? RedSuit : BlackSuit;
            SetIndex(face.Rank + "\n" + face.Suit, color, 0.25f);
            if (face.IsCourt)
            {
                _court.gameObject.SetActive(true);
                symbol.text = face.Rank + "\n" + face.Suit;
                symbol.fontSize = Mathf.RoundToInt(_size.x * 0.34f);
                return;
            }

            var pips = face.Pips;
            float scale = pips.Count == 1 ? 0.55f : 0.23f;
            while (_pips.Count < pips.Count) _pips.Add(UiBuilder.AddText("Pip", transform, TextAnchor.MiddleCenter, false));
            for (int i = 0; i < pips.Count; i++)
            {
                var pip = _pips[i];
                pip.gameObject.SetActive(true);
                pip.text = face.Suit;
                pip.color = color;
                pip.fontSize = Mathf.RoundToInt(_size.x * scale);
                UiBuilder.Put(pip, pips[i].x * _size.x * 0.16f, pips[i].y * _size.y * 0.33f, _size.x * 0.3f, _size.x * 0.3f);
                pip.rectTransform.localRotation = Quaternion.Euler(0f, 0f, pips[i].y < -0.01f ? 180f : 0f);
            }
        }

        private void SetIndex(string text, Color color, float scale)
        {
            symbol.color = color;
            foreach (var index in new[] { _topIndex, _bottomIndex })
            {
                index.text = text;
                index.color = color;
                index.fontSize = Mathf.RoundToInt(_size.x * scale);
            }
        }

        /// <summary>めくる動き。横に開いてから弾む。</summary>
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
                float bounce = 1f + 0.14f * k * Mathf.Cos(_bounceAge * 22f);
                transform.localScale = new Vector3(bounce * Mathf.Clamp01(_bounceAge / FlipSeconds), bounce, 1f);
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
            Ensure();
            _size = size;
            float w = size.x, h = size.y;
            UiBuilder.Put(background, position.x, position.y, w, h);
            UiBuilder.Put(symbol, 0f, 0f, w, h);
            symbol.alignment = TextAnchor.MiddleCenter;
            symbol.lineSpacing = 0.85f;
            UiBuilder.Put(_court, 0f, 0f, w * 0.52f, h * 0.76f);
            UiBuilder.Put(_topIndex, -w * 0.36f, 0f, w * 0.26f, h * 0.94f);
            UiBuilder.Put(_bottomIndex, w * 0.36f, 0f, w * 0.26f, h * 0.94f);
            float chip = Mathf.Min(34f, w * 0.38f);
            UiBuilder.Put(_chip, -w * 0.35f, -h * 0.4f, chip, chip);
            UiBuilder.Put(owner, 0f, 0f, chip, chip);
            owner.alignment = TextAnchor.MiddleCenter;
            owner.fontStyle = FontStyle.Bold;
            owner.fontSize = 12;
        }
    }
}
