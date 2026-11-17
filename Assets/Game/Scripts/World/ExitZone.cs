using UnityEngine;
using CryptEscape.Game;
using CryptEscape.Player;

namespace CryptEscape.World
{
    /// <summary>出口の判定。出口が開いていれば（ボス撃破後）触れた時点でクリア。</summary>
    [RequireComponent(typeof(Collider))]
    public class ExitZone : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerMotor>() == null) return;
            GameDirector.Instance?.TryEscape();
        }
    }
}
