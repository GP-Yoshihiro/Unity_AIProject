using UnityEngine;
using UnityEngine.UI;
using CryptEscape.Game;

namespace CryptEscape.UI
{
    /// <summary>HUD シーンのルート。読み込まれたら GameDirector に登録する。</summary>
    public class HudView : MonoBehaviour
    {
        [SerializeField] Text coinText;
        [SerializeField] Text livesText;
        [SerializeField] Text timerText;
        [SerializeField] Text promptText;
        [SerializeField] Image[] hearts;
        [SerializeField] GameObject bossPanel;
        [SerializeField] RectTransform bossFill;

        static readonly Color HeartFull = new Color(0.9f, 0.15f, 0.2f);
        static readonly Color HeartEmpty = new Color(0.25f, 0.25f, 0.28f, 0.8f);

        void Awake() => GameDirector.Register(this);
        void OnDestroy() => GameDirector.Unregister(this);

        public void SetCoins(int count, int total) => coinText.text = $"COIN {count}/{total}";
        public void SetLives(int lives) => livesText.text = $"LIVES {lives}";
        public void SetTimer(string time) => timerText.text = time;
        public void SetPrompt(string text) => promptText.text = text ?? "";

        public void SetHearts(int current)
        {
            for (int i = 0; i < hearts.Length; i++)
                hearts[i].color = i < current ? HeartFull : HeartEmpty;
        }

        public void SetBoss(bool visible, int hp, int maxHp)
        {
            if (bossPanel.activeSelf != visible) bossPanel.SetActive(visible);
            if (!visible) return;
            float ratio = maxHp > 0 ? Mathf.Clamp01((float)hp / maxHp) : 0f;
            bossFill.anchorMax = new Vector2(ratio, 1f);
        }
    }
}
