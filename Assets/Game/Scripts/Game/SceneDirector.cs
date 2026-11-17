using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CryptEscape.Game
{
    /// <summary>シーンとダイアログの Additive 読み込み・破棄を、指定した順に1つずつ実行する（research R1）。</summary>
    public class SceneDirector : MonoBehaviour
    {
        public const string Boot = "Boot";
        public const string Title = "Title";
        public const string Dungeon = "Dungeon";
        public const string Hud = "HUD";
        public const string PauseDialog = "PauseDialog";
        public const string GameOverDialog = "GameOverDialog";
        public const string ClearDialog = "ClearDialog";

        public static readonly string[] Dialogs = { PauseDialog, GameOverDialog, ClearDialog };

        public readonly struct Op
        {
            public readonly bool load;
            public readonly string scene;
            Op(bool load, string scene) { this.load = load; this.scene = scene; }
            public static Op Load(string scene) => new Op(true, scene);
            public static Op Unload(string scene) => new Op(false, scene);
        }

        public bool Busy { get; private set; }

        /// <summary>直前の一連の読み込み・破棄にかかった実時間（秒）。即リスタートの計測用。</summary>
        public float LastDuration { get; private set; }

        public static bool IsLoaded(string scene) => SceneManager.GetSceneByName(scene).isLoaded;

        public void Run(IEnumerable<Op> ops, Action onDone)
        {
            if (Busy)
            {
                Debug.LogWarning("[SceneDirector] Busy. Ignored scene request.");
                return;
            }
            StartCoroutine(RunOps(new List<Op>(ops), onDone));
        }

        IEnumerator RunOps(List<Op> ops, Action onDone)
        {
            Busy = true;
            float started = Time.realtimeSinceStartup;

            foreach (var op in ops)
            {
                if (op.load)
                {
                    if (!IsLoaded(op.scene))
                        yield return SceneManager.LoadSceneAsync(op.scene, LoadSceneMode.Additive);
                    if (op.scene == Dungeon || op.scene == Title)
                        SceneManager.SetActiveScene(SceneManager.GetSceneByName(op.scene));
                }
                else if (IsLoaded(op.scene))
                {
                    yield return SceneManager.UnloadSceneAsync(op.scene);
                }
            }

            LastDuration = Time.realtimeSinceStartup - started;
            Busy = false;
            onDone?.Invoke();
        }

        public static string LoadedScenesText()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                if (!s.isLoaded) continue;
                if (sb.Length > 0) sb.Append('|');
                sb.Append(s.name);
            }
            return sb.ToString();
        }

        public static string TopDialog()
        {
            for (int i = Dialogs.Length - 1; i >= 0; i--)
                if (IsLoaded(Dialogs[i])) return Dialogs[i];
            return "";
        }
    }
}
