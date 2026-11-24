using System.Linq;
using MemoryMatch.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryMatch
{
    /// <summary>成立時の演出。表示だけを担い、ルールと入力には関わらない。</summary>
    public sealed class ComboEffects : MonoBehaviour
    {
        private const float PopupSeconds = 1.6f;
        private const float ShakeSeconds = 0.5f;
        private const float FlashSeconds = 0.4f;
        private const float Gravity = 900f;

        private static readonly Color[] LevelColors =
        {
            Color.white, Color.white, new Color(1f, 0.9f, 0.3f), new Color(1f, 0.6f, 0.15f), new Color(1f, 0.35f, 0.6f), Color.white,
        };

        [SerializeField] private RectTransform stage;
        [SerializeField] private RectTransform particleLayer;
        [SerializeField] private Image flash;
        [SerializeField] private Text popup;

        private sealed class Particle
        {
            public RectTransform Rect;
            public Image Image;
            public Vector2 Velocity;
            public float Spin;
            public float Life;
            public float Age;
            public bool Alive => Age < Life;
        }

        private Particle[] _particles;
        private EffectLevel _level;
        private float _age = float.MaxValue;
        private Vector2 _stageHome;

        public int Level => _level.Level;
        public int ComboShown { get; private set; }
        public int PointsShown { get; private set; }
        public int PlayCount { get; private set; }
        public int ActiveParticles => _particles == null ? 0 : _particles.Count(p => p.Alive);
        public bool IsPlaying => _age < PopupSeconds || ActiveParticles > 0;
        public string PopupText => popup.gameObject.activeSelf ? popup.text : "";

        private void Awake()
        {
            _stageHome = stage.anchoredPosition;
            _particles = new Particle[EffectLevel.For(EffectLevel.Max).Particles];
            for (int i = 0; i < _particles.Length; i++)
            {
                var go = new GameObject("Particle", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(particleLayer, false);
                go.SetActive(false);
                var image = go.GetComponent<Image>();
                image.raycastTarget = false;
                _particles[i] = new Particle { Rect = (RectTransform)go.transform, Image = image };
            }
            StopAll();
        }

        public void PlayMatch(Vector2[] origins, int combo, int points)
        {
            StopAll();
            _level = EffectLevel.For(combo);
            _age = 0f;
            ComboShown = combo;
            PointsShown = points;
            PlayCount++;
            popup.text = combo >= 2 ? $"{combo} COMBO!\n+{points}" : $"+{points}";
            popup.fontSize = _level.TextSize;
            popup.gameObject.SetActive(true);

            for (int i = 0; i < _level.Particles; i++) Launch(_particles[i], origins[i % origins.Length]);
            Animate();
        }

        public void StopAll()
        {
            _age = float.MaxValue;
            _level = EffectLevel.For(0);
            ComboShown = PointsShown = 0;
            foreach (var p in _particles)
            {
                p.Age = p.Life = 0f;
                p.Rect.gameObject.SetActive(false);
            }
            popup.gameObject.SetActive(false);
            flash.gameObject.SetActive(false);
            stage.anchoredPosition = _stageHome;
        }

        private void Launch(Particle p, Vector2 origin)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float speed = Random.Range(120f, 260f + 110f * _level.Level);
            p.Velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed + Vector2.up * (60f * _level.Level);
            p.Spin = Random.Range(-540f, 540f);
            p.Life = Random.Range(0.6f, 0.9f + 0.15f * _level.Level);
            p.Age = 0f;
            p.Rect.anchoredPosition = origin;
            p.Rect.sizeDelta = Vector2.one * Random.Range(8f, 12f + 3f * _level.Level);
            p.Image.color = _level.Level >= EffectLevel.Max ? Color.HSVToRGB(Random.value, 0.75f, 1f) : Color.Lerp(LevelColors[_level.Level], Color.white, Random.Range(0f, 0.5f));
            p.Rect.gameObject.SetActive(true);
        }

        private void Update()
        {
            if (!IsPlaying) return;
            float dt = Time.deltaTime;
            _age += dt;
            foreach (var p in _particles)
            {
                if (!p.Alive) continue;
                p.Age += dt;
                p.Velocity += Vector2.down * (Gravity * dt);
                p.Rect.anchoredPosition += p.Velocity * dt;
                p.Rect.Rotate(0f, 0f, p.Spin * dt);
                var c = p.Image.color;
                c.a = Mathf.Clamp01(2f * (1f - p.Age / p.Life));
                p.Image.color = c;
                if (!p.Alive) p.Rect.gameObject.SetActive(false);
            }
            Animate();
            if (!IsPlaying) StopAll();
        }

        private void Animate()
        {
            bool showPopup = _age < PopupSeconds;
            popup.gameObject.SetActive(showPopup);
            if (showPopup)
            {
                float punch = 1f + 0.6f * Mathf.Exp(-_age * 7f) * Mathf.Cos(_age * 18f);
                popup.rectTransform.localScale = Vector3.one * punch;
                var color = _level.Level >= EffectLevel.Max ? Color.HSVToRGB(Mathf.Repeat(_age * 0.8f, 1f), 0.6f, 1f) : LevelColors[_level.Level];
                color.a = Mathf.Clamp01((PopupSeconds - _age) / 0.4f);
                popup.color = color;
            }

            float shake = Mathf.Clamp01(1f - _age / ShakeSeconds) * _level.Shake;
            stage.anchoredPosition = _stageHome + (shake > 0f ? Random.insideUnitCircle * shake : Vector2.zero);

            float flashAlpha = Mathf.Clamp01(1f - _age / FlashSeconds) * _level.Flash;
            flash.gameObject.SetActive(flashAlpha > 0f);
            flash.color = new Color(1f, 1f, 1f, flashAlpha);
        }
    }
}
