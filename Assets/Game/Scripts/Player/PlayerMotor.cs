using UnityEngine;
using CryptEscape.Game;

namespace CryptEscape.Player
{
    /// <summary>8方向移動と回避のダッシュ。向き（Facing）は最後に動いた方向。</summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMotor : MonoBehaviour
    {
        Rigidbody body;
        Vector3 dashVelocity;
        float dashRemaining;

        public Vector3 Facing { get; private set; } = Vector3.forward;
        public bool IsDashing => dashRemaining > 0f;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            var director = GameDirector.Instance;
            var velocity = body.linearVelocity;

            if (dashRemaining > 0f)
            {
                dashRemaining -= Time.fixedDeltaTime;
                velocity.x = dashVelocity.x;
                velocity.z = dashVelocity.z;
            }
            else if (director != null && director.ControlsEnabled)
            {
                var input = director.Input.Move;
                var move = new Vector3(input.x, 0f, input.y);
                velocity.x = move.x * director.Config.moveSpeed;
                velocity.z = move.z * director.Config.moveSpeed;
                if (move.sqrMagnitude > 0.01f) Facing = move.normalized;
            }
            else
            {
                velocity.x = 0f;
                velocity.z = 0f;
            }

            body.linearVelocity = velocity;
            body.MoveRotation(Quaternion.LookRotation(Facing, Vector3.up));
        }

        public void Dash(Vector3 direction, float distance, float duration)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.01f) direction = Facing;
            Facing = direction.normalized;
            dashVelocity = Facing * (distance / duration);
            dashRemaining = duration;
        }

        /// <summary>復活・初期化用。位置と速度を戻し、向きを +Z にする。</summary>
        public void ResetTo(Vector3 position)
        {
            dashRemaining = 0f;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.position = position;
            transform.position = position;
            Facing = Vector3.forward;
        }
    }
}
