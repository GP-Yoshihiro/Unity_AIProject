using UnityEngine;

namespace CryptEscape.Enemies
{
    /// <summary>ボスの攻撃の予兆表示。突進は向きを示す線、範囲攻撃は地面の円。</summary>
    public class AttackTelegraph : MonoBehaviour
    {
        [SerializeField] Transform chargeLine;
        [SerializeField] Transform aoeCircle;

        void Awake() => HideAll();

        public void ShowChargeLine(Vector3 from, Vector3 direction, float length)
        {
            direction.y = 0f;
            direction.Normalize();
            chargeLine.gameObject.SetActive(true);
            chargeLine.position = new Vector3(from.x, 0.05f, from.z) + direction * (length * 0.5f);
            chargeLine.rotation = Quaternion.LookRotation(direction, Vector3.up);
            chargeLine.localScale = new Vector3(1.6f, 0.02f, length);
        }

        public void ShowAoeCircle(Vector3 center, float radius)
        {
            aoeCircle.gameObject.SetActive(true);
            aoeCircle.position = new Vector3(center.x, 0.05f, center.z);
            aoeCircle.rotation = Quaternion.identity;
            aoeCircle.localScale = new Vector3(radius * 2f, 0.01f, radius * 2f);
        }

        public void HideAll()
        {
            if (chargeLine != null) chargeLine.gameObject.SetActive(false);
            if (aoeCircle != null) aoeCircle.gameObject.SetActive(false);
        }
    }
}
