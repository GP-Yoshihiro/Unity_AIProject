using UnityEngine;
using UnityEngine.UI;

namespace MemoryMatch
{
    /// <summary>実行時に表示部品を作る。</summary>
    public static class UiBuilder
    {
        public static Text AddText(string name, Transform parent, TextAnchor anchor, bool bold = true)
        {
            var text = Add<Text>(name, parent);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            text.alignment = anchor;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        public static Image AddImage(string name, Transform parent, ArtKind kind, Color color)
        {
            var image = Add<Image>(name, parent);
            ArtImage.Apply(image, kind);
            image.color = color;
            return image;
        }

        public static void Put(Graphic graphic, float x, float y, float width, float height)
        {
            graphic.rectTransform.anchoredPosition = new Vector2(x, y);
            graphic.rectTransform.sizeDelta = new Vector2(width, height);
        }

        private static T Add<T>(string name, Transform parent) where T : Component
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.AddComponent<T>();
        }
    }
}
