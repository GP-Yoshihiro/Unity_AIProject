using System;
using UnityEngine;

namespace MemoryMatch
{
    public enum ArtKind { Table, Panel, CardFront, CardBack, CourtFrame, Chip }

    /// <summary>テーブル、カード、枠の絵をコードで描く。外部アセットは使わない。</summary>
    public static class TableArt
    {
        private static readonly Sprite[] Cache = new Sprite[Enum.GetValues(typeof(ArtKind)).Length];

        private static readonly Color Gold = new Color(0.86f, 0.7f, 0.34f);

        public static Sprite Get(ArtKind kind)
        {
            int i = (int)kind;
            if (Cache[i] == null) Cache[i] = Create(kind);
            return Cache[i];
        }

        private static Sprite Create(ArtKind kind)
        {
            switch (kind)
            {
                case ArtKind.Table: return Draw(1024, 576, 0, TablePixel);
                case ArtKind.Panel: return Draw(64, 64, 20, (p, s) => Cover(Box(p, s, 14f, 1f), Color.white));
                case ArtKind.CardFront: return Draw(168, 234, 0, FrontPixel);
                case ArtKind.CardBack: return Draw(168, 234, 0, BackPixel);
                case ArtKind.CourtFrame: return Draw(96, 144, 0, CourtPixel);
                default: return Draw(96, 96, 0, ChipPixel);
            }
        }

        private static Sprite Draw(int w, int h, int border, Func<Vector2, Vector2, Color> pixel)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Trilinear, hideFlags = HideFlags.DontSave };
            var size = new Vector2(w, h);
            var colors = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++) colors[y * w + x] = pixel(new Vector2(x + 0.5f, y + 0.5f) - size / 2f, size);
            }
            tex.SetPixels(colors);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.one / 2f, 100f, 0, SpriteMeshType.FullRect, Vector4.one * border);
        }

        /// <summary>角丸の四角までの距離。内側が負。</summary>
        private static float Box(Vector2 p, Vector2 size, float radius, float inset)
        {
            var q = new Vector2(Mathf.Abs(p.x), Mathf.Abs(p.y)) - (size / 2f - Vector2.one * (inset + radius));
            return new Vector2(Mathf.Max(q.x, 0f), Mathf.Max(q.y, 0f)).magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0f) - radius;
        }

        private static Color Cover(float distance, Color color)
        {
            color.a *= Mathf.Clamp01(0.5f - distance);
            return color;
        }

        /// <summary>距離0の線を width の太さで重ねる。</summary>
        private static Color Line(Color under, float distance, float width, Color line)
        {
            return Color.Lerp(under, new Color(line.r, line.g, line.b, under.a), Mathf.Clamp01(width / 2f + 0.5f - Mathf.Abs(distance)) * line.a);
        }

        private static Color TablePixel(Vector2 p, Vector2 size)
        {
            float d = Box(p, size, 34f, 13f);
            if (d > 0f)
            {
                // 木の縁
                float shade = 0.5f + 0.5f * Mathf.Cos((d - 6f) / 6f * 1.4f);
                float grain = Mathf.PerlinNoise(p.x * 0.015f + 40f, p.y * 0.3f + 400f) * 0.06f;
                var wood = Color.Lerp(new Color(0.16f, 0.07f, 0.03f), new Color(0.4f + grain, 0.21f + grain * 0.6f, 0.1f), shade);
                return Line(wood, d - 1f, 2f, new Color(0.08f, 0.03f, 0.01f, 0.8f));
            }

            float r = new Vector2(p.x / size.x, p.y / size.y).magnitude * 1.45f;
            var felt = Color.Lerp(new Color(0.07f, 0.45f, 0.27f), new Color(0.015f, 0.2f, 0.12f), Mathf.Clamp01(r * r));
            float n = (Mathf.Repeat(Mathf.Sin(p.x * 12.9898f + p.y * 78.233f) * 4375.85f, 1f) - 0.5f) * 0.035f;
            felt = new Color(felt.r + n, felt.g + n, felt.b + n, 1f);
            felt = Line(felt, d + 7f, 2f, new Color(Gold.r, Gold.g, Gold.b, 0.75f));
            // ディーラー側の弧
            float arc = new Vector2(p.x, (p.y - 420f) * 1.15f).magnitude - 560f;
            felt = Line(felt, arc, 2f, new Color(Gold.r, Gold.g, Gold.b, 0.35f));
            return Line(felt, arc - 22f, 1.5f, new Color(Gold.r, Gold.g, Gold.b, 0.25f));
        }

        private static Color FrontPixel(Vector2 p, Vector2 size)
        {
            float d = Box(p, size, 15f, 1f);
            var paper = Color.Lerp(new Color(1f, 0.995f, 0.97f), new Color(0.95f, 0.94f, 0.9f), Mathf.InverseLerp(size.y / 2f, -size.y / 2f, p.y));
            return Cover(d, Line(paper, d + 1.5f, 2f, new Color(0.55f, 0.53f, 0.48f, 0.8f)));
        }

        private static Color BackPixel(Vector2 p, Vector2 size)
        {
            float d = Box(p, size, 15f, 1f);
            var color = new Color(0.97f, 0.96f, 0.93f);
            float inner = Box(p, size, 9f, 10f);
            if (inner < 0f)
            {
                color = new Color(0.6f, 0.08f, 0.13f);
                float a = Mathf.Abs(Mathf.Repeat((p.x + p.y) / 16f, 1f) - 0.5f);
                float b = Mathf.Abs(Mathf.Repeat((p.x - p.y) / 16f, 1f) - 0.5f);
                if (Mathf.Min(a, b) < 0.1f) color = new Color(0.82f, 0.4f, 0.4f);
                else if (a > 0.38f && b > 0.38f) color = Gold;
                color = Line(color, inner + 6f, 2f, new Color(0.97f, 0.93f, 0.85f, 1f));
                float gem = Mathf.Abs(p.x) / 30f + Mathf.Abs(p.y) / 42f;
                if (gem < 1f) color = gem > 0.86f ? Gold : new Color(0.42f, 0.04f, 0.08f);
                if (gem < 0.42f) color = gem > 0.3f ? Gold : new Color(0.75f, 0.15f, 0.2f);
            }
            return Cover(d, Line(color, d + 1.5f, 2f, new Color(0.45f, 0.43f, 0.4f, 0.8f)));
        }

        private static Color CourtPixel(Vector2 p, Vector2 size)
        {
            float d = Box(p, size, 8f, 2f);
            var color = Color.Lerp(new Color(1f, 0.95f, 0.75f), new Color(0.98f, 0.87f, 0.55f), Mathf.Abs(p.y) / (size.y / 2f));
            float stripe = Mathf.Abs(Mathf.Repeat((p.x + p.y) / 14f, 1f) - 0.5f);
            if (stripe < 0.07f) color = new Color(0.93f, 0.78f, 0.45f);
            color = Line(color, d + 2f, 3f, new Color(0.62f, 0.42f, 0.1f, 1f));
            return Cover(d, Line(color, d + 7f, 1.5f, new Color(0.62f, 0.42f, 0.1f, 0.8f)));
        }

        private static Color ChipPixel(Vector2 p, Vector2 size)
        {
            float r = p.magnitude;
            float angle = Mathf.Repeat(Mathf.Atan2(p.y, p.x) / (Mathf.PI * 2f) * 8f, 1f);
            var color = r > 35f && angle < 0.5f ? Color.white : new Color(0.72f, 0.72f, 0.72f);
            if (r <= 35f) color = new Color(0.8f, 0.8f, 0.8f);
            color = Line(color, r - 35f, 2f, Color.white);
            return Cover(r - 46f, color);
        }
    }
}
