using UnityEngine;
using CryptEscape.Game;
using CryptEscape.Player;

namespace CryptEscape.World
{
    /// <summary>金貨。プレイヤーが触れたら GameDirector に通知する。取得済みは GameDirector が非アクティブにする。</summary>
    [RequireComponent(typeof(Collider))]
    public class Coin : MonoBehaviour
    {
        public int coinId;
        [SerializeField] Transform visual;

        void Update()
        {
            if (visual != null) visual.Rotate(0f, 120f * Time.deltaTime, 0f, Space.World);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerMotor>() == null) return;
            GameDirector.Instance?.CollectCoin(this);
        }
    }
}
