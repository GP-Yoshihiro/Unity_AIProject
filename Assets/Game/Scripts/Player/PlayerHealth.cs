using UnityEngine;
using CryptEscape.Core;
using CryptEscape.Game;

namespace CryptEscape.Player
{
    /// <summary>ハートと無敵時間（PlayerHealthRules）を持ち、被弾中は見た目を点滅させる。</summary>
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] GameObject visual;

        PlayerHealthRules rules;

        public PlayerHealthRules Rules => rules ??= new PlayerHealthRules(GameDirector.Instance != null ? GameDirector.Instance.Config.maxHearts : 5);
        public int Hearts => Rules.Hearts;

        void Update()
        {
            Rules.Tick(Time.deltaTime);
            if (visual != null)
            {
                bool show = !Rules.IsInvincible || Mathf.Repeat(Time.time, 0.16f) < 0.08f;
                if (Rules.IsDead) show = true;
                if (visual.activeSelf != show) visual.SetActive(show);
            }
        }

        public void ResetHealth()
        {
            Rules.Reset();
            if (visual != null) visual.SetActive(true);
        }
    }
}
