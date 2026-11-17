using UnityEngine;
using CryptEscape.Game;
using CryptEscape.Player;

namespace CryptEscape.World
{
    /// <summary>ボスの間の範囲。プレイヤーが入るとボス戦が始まる。</summary>
    [RequireComponent(typeof(Collider))]
    public class BossArena : MonoBehaviour
    {
        void OnTriggerStay(Collider other)
        {
            if (other.GetComponentInParent<PlayerMotor>() == null) return;
            GameDirector.Instance?.OnArenaEntered();
        }
    }
}
