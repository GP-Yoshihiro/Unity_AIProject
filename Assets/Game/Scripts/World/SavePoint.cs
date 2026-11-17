using UnityEngine;

namespace CryptEscape.World
{
    /// <summary>祈りの台座。範囲内で保存キーを押すと復活地点になる（判定は GameDirector）。</summary>
    public class SavePoint : MonoBehaviour
    {
        public int savePointId;
        [SerializeField] Vector3 respawnOffset = new Vector3(0f, 0f, -1.5f);
        [SerializeField] Renderer glow;

        public Vector3 RespawnPosition => transform.position + respawnOffset;

        public void SetActiveSave(bool isCurrent)
        {
            if (glow == null) return;
            glow.material.color = isCurrent ? new Color(0.4f, 1f, 1f) : new Color(0.25f, 0.55f, 0.6f);
        }
    }
}
