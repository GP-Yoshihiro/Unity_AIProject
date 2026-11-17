using UnityEngine;
using UnityEngine.InputSystem;

namespace CryptEscape.Game
{
    /// <summary>
    /// Input Actions（contracts/input-actions.md）を読み、このフレームで押されたボタンをまとめる。
    /// 他のスクリプトより先に実行する。
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class GameInput : MonoBehaviour
    {
        [SerializeField] InputActionAsset actions;

        InputAction move, attack, dodge, save, pause, restart, navigate, submit, cancel;
        ButtonEdge attackEdge, dodgeEdge, saveEdge, pauseEdge, restartEdge, submitEdge, cancelEdge, upEdge, downEdge;

        public Vector2 Move { get; private set; }
        public bool Attack { get; private set; }
        public bool Dodge { get; private set; }
        public bool Save { get; private set; }
        public bool Pause { get; private set; }
        public bool Restart { get; private set; }
        public bool Submit { get; private set; }
        public bool Cancel { get; private set; }
        public bool NavUp { get; private set; }
        public bool NavDown { get; private set; }

        void OnEnable()
        {
            var gameplay = actions.FindActionMap("Gameplay", true);
            var ui = actions.FindActionMap("UI", true);
            move = gameplay.FindAction("Move", true);
            attack = gameplay.FindAction("Attack", true);
            dodge = gameplay.FindAction("Dodge", true);
            save = gameplay.FindAction("Save", true);
            pause = gameplay.FindAction("Pause", true);
            restart = gameplay.FindAction("Restart", true);
            navigate = ui.FindAction("Navigate", true);
            submit = ui.FindAction("Submit", true);
            cancel = ui.FindAction("Cancel", true);
            actions.Enable();
        }

        void OnDisable() => actions.Disable();

        void Update()
        {
            Move = Vector2.ClampMagnitude(move.ReadValue<Vector2>(), 1f);
            Attack = attackEdge.Update(attack.IsPressed());
            Dodge = dodgeEdge.Update(dodge.IsPressed());
            Save = saveEdge.Update(save.IsPressed());
            Pause = pauseEdge.Update(pause.IsPressed());
            Restart = restartEdge.Update(restart.IsPressed());
            Submit = submitEdge.Update(submit.IsPressed());
            Cancel = cancelEdge.Update(cancel.IsPressed());

            float y = navigate.ReadValue<Vector2>().y;
            NavUp = upEdge.Update(y > 0.5f);
            NavDown = downEdge.Update(y < -0.5f);
        }
    }
}
