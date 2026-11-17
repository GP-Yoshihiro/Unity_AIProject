using UnityEngine;

namespace BallCollect
{
    /// <summary>
    /// Player が触れたら GameManager に通知するアイテム。トリガー Collider が必要。
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CollectibleItem : MonoBehaviour
    {
        [SerializeField] BallCollectGame game;

        public int ItemId { get; set; }

        public void Bind(BallCollectGame owner) => game = owner;

        void OnTriggerEnter(Collider other)
        {
            if (game == null) return;
            if (other.GetComponentInParent<PlayerController>() == null) return;
            game.Collect(this);
        }
    }
}
