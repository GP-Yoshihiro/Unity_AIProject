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

        private static readonly string[] Symbols = { "●", "▲", "■", "◆", "★", "♥" };
        private static readonly Color[] SymbolColors =
        {
            new Color(0.85f, 0.15f, 0.15f), new Color(0.1f, 0.5f, 0.15f), new Color(0.1f, 0.3f, 0.85f),
            new Color(0.55f, 0.15f, 0.7f), new Color(0.75f, 0.45f, 0f), new Color(0.85f, 0.2f, 0.5f),
        };

        [SerializeField] private Image background;
        [SerializeField] private Text symbol;
        [SerializeField] private Text owner;

        public void ShowBack()
        {
            background.color = BackColor;
            symbol.text = "";
            owner.text = "";
        }

        public void ShowFront(int symbolId, bool matched, string ownerLabel)
        {
            background.color = matched ? MatchedColor : FrontColor;
            symbol.text = Symbols[symbolId];
            symbol.color = SymbolColors[symbolId];
            owner.text = ownerLabel;
        }
    }
}
