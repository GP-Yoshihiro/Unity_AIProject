using UnityEngine;
using UnityEngine.UI;

namespace MemoryMatch
{
    /// <summary>得点枠。見出し・大きな値・補足を1つの枠に収める。</summary>
    public sealed class StatPanel : MonoBehaviour
    {
        private static readonly Color Idle = new Color(0.45f, 0.38f, 0.2f);
        private static readonly Color Active = new Color(1f, 0.85f, 0.3f);
        private static readonly Color Body = new Color(0.04f, 0.1f, 0.08f, 1f);

        private Image _frame;
        private Image _body;
        private Image _header;
        private Text _title;
        private Text _tag;
        private Text _value;
        private Text _detail;

        private void Ensure()
        {
            if (_frame != null) return;
            _frame = UiBuilder.AddImage(name + "_Frame", transform, ArtKind.Panel, Idle);
            _body = UiBuilder.AddImage(name + "_Body", transform, ArtKind.Panel, Body);
            _header = UiBuilder.AddImage(name + "_Header", transform, ArtKind.Panel, Color.gray);
            _title = UiBuilder.AddText(name + "_Title", transform, TextAnchor.MiddleLeft);
            _tag = UiBuilder.AddText(name + "_Tag", transform, TextAnchor.MiddleRight);
            _value = UiBuilder.AddText(name + "_Value", transform, TextAnchor.MiddleCenter);
            _detail = UiBuilder.AddText(name + "_Detail", transform, TextAnchor.MiddleLeft);
        }

        public void Place(Vector2 position, Vector2 size)
        {
            Ensure();
            ((RectTransform)transform).anchoredPosition = position;
            float w = size.x, h = size.y;
            UiBuilder.Put(_frame, 0f, 0f, w, h);
            UiBuilder.Put(_body, 0f, 0f, w - 8f, h - 8f);
            UiBuilder.Put(_header, 0f, h / 2f - 20f, w - 14f, 30f);
            UiBuilder.Put(_title, 0f, h / 2f - 20f, w - 38f, 30f);
            UiBuilder.Put(_tag, 0f, h / 2f - 20f, w - 38f, 30f);
            _title.fontSize = 22;
            _tag.fontSize = 19;
        }

        public void Show(string title, string value, string detail, Color color, bool active)
        {
            Ensure();
            bool hasDetail = detail.Length > 0;
            float w = _frame.rectTransform.sizeDelta.x;
            UiBuilder.Put(_value, hasDetail ? -w * 0.18f : 0f, -16f, hasDetail ? w * 0.56f : w - 20f, 46f);
            UiBuilder.Put(_detail, w * 0.29f, -16f, w * 0.34f, 46f);
            _value.fontSize = 48;
            _detail.fontSize = 20;
            _title.text = title;
            _tag.text = active ? "TURN" : "";
            _value.text = value;
            _detail.text = detail;
            _header.color = color;
            _frame.color = active ? Active : Idle;
            _tag.color = Color.white;
            _value.color = active ? Active : Color.white;
            _detail.color = new Color(0.9f, 0.92f, 0.88f);
            _title.color = Color.white;
        }
    }
}
