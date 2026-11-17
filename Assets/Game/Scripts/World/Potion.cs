using UnityEngine;
using CryptEscape.Game;
using CryptEscape.Player;

namespace CryptEscape.World
{
    /// <summary>回復薬。ハートが満タンなら取らずに残す。上に立ったまま被弾した場合も拾えるよう Stay で判定する。</summary>
    [RequireComponent(typeof(Collider))]
    public class Potion : MonoBehaviour
    {
        public int potionId;

        void OnTriggerStay(Collider other)
        {
            if (other.GetComponentInParent<PlayerMotor>() == null) return;
            GameDirector.Instance?.TryUsePotion(this);
        }
    }
}
