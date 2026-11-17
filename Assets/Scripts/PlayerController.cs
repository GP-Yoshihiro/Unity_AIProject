using UnityEngine;
using UnityEngine.InputSystem;

namespace BallCollect
{
    /// <summary>
    /// WASD で球を XZ 平面上に動かす。W=+Z, S=-Z, D=+X, A=-X。
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] float moveSpeed = 5f;

        /// <summary>false の間は入力を無視して水平方向の速度を 0 にする（TIME UP 用）。</summary>
        public bool canMove = true;

        Rigidbody body;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            var direction = canMove ? ReadDirection() : Vector3.zero;
            var velocity = body.linearVelocity;
            velocity.x = direction.x * moveSpeed;
            velocity.z = direction.z * moveSpeed;
            body.linearVelocity = velocity;
        }

        static Vector3 ReadDirection()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return Vector3.zero;

            var direction = Vector3.zero;
            if (keyboard.wKey.isPressed) direction.z += 1f;
            if (keyboard.sKey.isPressed) direction.z -= 1f;
            if (keyboard.dKey.isPressed) direction.x += 1f;
            if (keyboard.aKey.isPressed) direction.x -= 1f;
            return Vector3.ClampMagnitude(direction, 1f);
        }

        /// <summary>位置と速度を初期状態に戻す。</summary>
        public void ResetTo(Vector3 position)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.position = position;
            transform.position = position;
        }
    }
}
