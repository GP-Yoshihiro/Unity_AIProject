using UnityEngine;

namespace CryptEscape.Player
{
    /// <summary>斜め上からプレイヤーを追従する見下ろしカメラ。画面の上が +Z。</summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Vector3 offset = new Vector3(0f, 21f, -8f);
        [SerializeField] float smoothTime = 0.12f;

        Vector3 velocity;

        public void SetTarget(Transform newTarget) => target = newTarget;

        void LateUpdate()
        {
            if (target == null) return;
            var desired = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
            transform.rotation = Quaternion.LookRotation(-offset.normalized, Vector3.up);
        }

        public void SnapToTarget()
        {
            if (target == null) return;
            transform.position = target.position + offset;
            transform.rotation = Quaternion.LookRotation(-offset.normalized, Vector3.up);
            velocity = Vector3.zero;
        }
    }
}
