using System.Collections.Generic;
using System.Text;
using UnityEngine;
using CryptEscape.Core;
using CryptEscape.Enemies;
using CryptEscape.UI;
using CryptEscape.World;

namespace CryptEscape.Game
{
    /// <summary>
    /// Boot シーンに常駐し、ゲーム全体の進行を管理する。
    /// 画面の切り替えは SceneDirector で Additive に行い、各シーンのルートは読み込み時に自分を登録する。
    /// 公開フィールドは unity-cli から状態を読むためのもの（contracts/cli-state.md）。
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class GameDirector : MonoBehaviour
    {
        public const string BestRecordKey = "CryptEscape.BestRun";

        public static GameDirector Instance { get; private set; }

        [SerializeField] GameConfig config;
        [SerializeField] GameInput input;
        [SerializeField] SceneDirector scenes;
        [SerializeField] AudioManager audioManager;

        [Header("State (read-only, for unity-cli)")]
        public string flow;
        public string loadedScenes;
        public string topDialog;
        public int dialogSelection;
        public bool sceneBusy;
        public int coinCount;
        public int hearts;
        public int lives;
        public int respawnPointId;
        public bool bossDoorOpen;
        public int bossHp;
        public string bossState;
        public bool bossDefeated;
        public bool exitOpen;
        public float elapsed;
        public string splitsText;
        public float bestTime;
        public string lastSounds;
        public string bgm;
        public bool savePromptVisible;
        public string prompt;
        public float lastSceneOpSeconds;

        readonly GameFlowMachine flowMachine = new GameFlowMachine();
        readonly RtaTimer timer = new RtaTimer();
        RunState run;
        LivesAndRespawn livesRules;
        BestRecord best;

        DungeonContext dungeon;
        HudView hud;
        TitleView title;
        readonly List<DialogView> dialogs = new List<DialogView>();

        float dyingRemaining;
        DeathOutcome pendingOutcome;
        bool arenaActive;
        string flashMessage;
        float flashRemaining;
        string[] dialogOptions = new string[0];

        public GameConfig Config => config;
        public GameInput Input => input;
        public AudioManager Audio => audioManager;
        public GameFlow CurrentFlow => flowMachine.Current;

        /// <summary>プレイヤーが操作できるか（Playing で、シーン切り替え中でない）。</summary>
        public bool ControlsEnabled => CurrentFlow == GameFlow.Playing && !scenes.Busy && dungeon != null;

        /// <summary>敵やボスが動くか。</summary>
        public bool WorldRunning => (CurrentFlow == GameFlow.Playing || CurrentFlow == GameFlow.Dying) && !scenes.Busy && dungeon != null;

        /// <summary>敵がプレイヤーを狙うか（倒れている間は狙わない）。</summary>
        public bool PlayerTargetable => CurrentFlow == GameFlow.Playing;

        public Vector3 PlayerPosition => dungeon != null ? dungeon.player.transform.position : Vector3.zero;

        #region Registration (Additive scenes)

        public static void Register(DungeonContext d) { if (Instance != null) Instance.dungeon = d; }
        public static void Unregister(DungeonContext d) { if (Instance != null && Instance.dungeon == d) Instance.dungeon = null; }
        public static void Register(HudView h) { if (Instance != null) Instance.hud = h; }
        public static void Unregister(HudView h) { if (Instance != null && Instance.hud == h) Instance.hud = null; }
        public static void Register(TitleView t) { if (Instance != null) { Instance.title = t; Instance.RefreshTitle(); } }
        public static void Unregister(TitleView t) { if (Instance != null && Instance.title == t) Instance.title = null; }
        public static void Register(DialogView d) { if (Instance != null) { Instance.dialogs.Add(d); Instance.RefreshDialog(); } }
        public static void Unregister(DialogView d) { if (Instance != null) Instance.dialogs.Remove(d); }

        #endregion

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            run = new RunState(config.startLives);
            livesRules = new LivesAndRespawn(run);
            best = BestRecord.FromJson(PlayerPrefs.GetString(BestRecordKey, ""));
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            Time.timeScale = 1f;
        }

        void Start()
        {
            Time.timeScale = 0f;
            audioManager.PlayBgm(AudioManager.BgmTitle);
            scenes.Run(new[] { SceneDirector.Op.Load(SceneDirector.Title) }, null);
        }

        void Update()
        {
            if (!scenes.Busy)
            {
                switch (CurrentFlow)
                {
                    case GameFlow.Title: UpdateTitle(); break;
                    case GameFlow.Playing: UpdatePlaying(); break;
                    case GameFlow.Paused: UpdatePaused(); break;
                    case GameFlow.Dying: UpdateDying(); break;
                    case GameFlow.GameOver: UpdateGameOver(); break;
                    case GameFlow.Cleared: UpdateCleared(); break;
                }
                timer.Tick(Time.unscaledDeltaTime, CurrentFlow);
            }

            Time.timeScale = WorldRunning ? 1f : 0f;
            RefreshHud();
            Publish();
        }

        #region Flow per state

        void UpdateTitle()
        {
            if (input.Submit) StartGame();
        }

        void UpdatePlaying()
        {
            if (input.Restart) { Restart(); return; }
            if (input.Pause) { OpenPause(); return; }

            UpdatePromptsAndSave();
        }

        void UpdatePaused()
        {
            if (input.Restart) { Restart(); return; }
            if (input.Pause || input.Cancel) { ClosePause(); return; }
            if (!NavigateDialog()) return;

            switch (dialogSelection)
            {
                case 0: ClosePause(); break;
                case 1: Restart(); break;
                case 2: ToTitle(); break;
            }
        }

        void UpdateDying()
        {
            if (input.Restart) { Restart(); return; }
            dyingRemaining -= Time.unscaledDeltaTime;
            if (dyingRemaining > 0f) return;

            if (pendingOutcome == DeathOutcome.Respawn)
            {
                RespawnInDungeon();
                flowMachine.FinishDying(hasLivesLeft: true);
            }
            else
            {
                flowMachine.FinishDying(hasLivesLeft: false);
                audioManager.Play(AudioManager.GameOver);
                OpenDialog(SceneDirector.GameOverDialog);
            }
        }

        void UpdateGameOver()
        {
            if (input.Restart) { Restart(); return; }
            if (!NavigateDialog()) return;
            if (dialogSelection == 0) Restart();
            else ToTitle();
        }

        void UpdateCleared()
        {
            if (input.Restart) { Restart(); return; }
            if (!NavigateDialog()) return;
            if (dialogSelection == 0) Restart();
            else ToTitle();
        }

        /// <summary>ダイアログの上下移動。決定されたら true。</summary>
        bool NavigateDialog()
        {
            if (dialogOptions.Length == 0) return false;
            if (input.NavUp) dialogSelection = (dialogSelection + dialogOptions.Length - 1) % dialogOptions.Length;
            if (input.NavDown) dialogSelection = (dialogSelection + 1) % dialogOptions.Length;
            CurrentDialogView()?.SetSelection(dialogSelection);
            return input.Submit;
        }

        #endregion

        #region Scene transitions (Additive)

        void StartGame()
        {
            run.FullReset();
            timer.Reset();
            scenes.Run(new[]
            {
                SceneDirector.Op.Unload(SceneDirector.Title),
                SceneDirector.Op.Load(SceneDirector.Dungeon),
                SceneDirector.Op.Load(SceneDirector.Hud),
            }, () =>
            {
                flowMachine.StartGame();
                OnDungeonLoaded();
            });
        }

        /// <summary>即リスタート。ダイアログを閉じ、Dungeon を読み直して全初期化する。</summary>
        public void Restart()
        {
            run.FullReset();
            timer.Reset();
            var ops = new List<SceneDirector.Op>();
            foreach (var d in SceneDirector.Dialogs) ops.Add(SceneDirector.Op.Unload(d));
            ops.Add(SceneDirector.Op.Unload(SceneDirector.Dungeon));
            ops.Add(SceneDirector.Op.Load(SceneDirector.Dungeon));
            ops.Add(SceneDirector.Op.Load(SceneDirector.Hud));
            scenes.Run(ops, () =>
            {
                flowMachine.Restart();
                OnDungeonLoaded();
            });
        }

        void ToTitle()
        {
            var ops = new List<SceneDirector.Op>();
            foreach (var d in SceneDirector.Dialogs) ops.Add(SceneDirector.Op.Unload(d));
            ops.Add(SceneDirector.Op.Unload(SceneDirector.Hud));
            ops.Add(SceneDirector.Op.Unload(SceneDirector.Dungeon));
            ops.Add(SceneDirector.Op.Load(SceneDirector.Title));
            scenes.Run(ops, () =>
            {
                flowMachine.ToTitle();
                run.FullReset();
                timer.Reset();
                audioManager.PlayBgm(AudioManager.BgmTitle);
                RefreshTitle();
            });
        }

        void OpenPause()
        {
            flowMachine.Pause();
            OpenDialog(SceneDirector.PauseDialog);
        }

        void ClosePause()
        {
            scenes.Run(new[] { SceneDirector.Op.Unload(SceneDirector.PauseDialog) }, () => flowMachine.Resume());
        }

        void OpenDialog(string dialogScene)
        {
            dialogSelection = 0;
            dialogOptions = OptionsFor(dialogScene);
            scenes.Run(new[] { SceneDirector.Op.Load(dialogScene) }, RefreshDialog);
        }

        void OnDungeonLoaded()
        {
            arenaActive = false;
            dyingRemaining = 0f;
            flashRemaining = 0f;
            dialogOptions = new string[0];

            for (int i = 0; i < dungeon.coins.Length; i++) dungeon.coins[i].coinId = i;
            for (int i = 0; i < dungeon.potions.Length; i++) dungeon.potions[i].potionId = i;
            for (int i = 0; i < dungeon.enemies.Length; i++) dungeon.enemies[i].enemyId = i;
            for (int i = 0; i < dungeon.savePoints.Length; i++)
            {
                dungeon.savePoints[i].savePointId = i;
                dungeon.savePoints[i].SetActiveSave(false);
            }

            dungeon.player.ResetTo(dungeon.startPoint.position);
            dungeon.player.GetComponent<Player.PlayerHealth>().ResetHealth();
            dungeon.cameraFollow.SetTarget(dungeon.player.transform);
            dungeon.cameraFollow.SnapToTarget();
            dungeon.bossDoor.SetActive(true);
            dungeon.exitGate.SetActive(true);
            audioManager.PlayBgm(AudioManager.BgmExplore);
        }

        #endregion

        #region Gameplay events

        public void CollectCoin(Coin coin)
        {
            if (!ControlsEnabled || !run.TryCollectCoin(coin.coinId)) return;

            coin.gameObject.SetActive(false);
            audioManager.Play(AudioManager.Coin);
            timer.Split("Coin" + run.CoinCount);

            if (run.BossDoorOpen && dungeon.bossDoor.activeSelf)
            {
                dungeon.bossDoor.SetActive(false);
                audioManager.Play(AudioManager.DoorOpen);
                Flash("THE SEALED DOOR HAS OPENED");
            }
        }

        public void TryUsePotion(Potion potion)
        {
            if (!ControlsEnabled || run.IsPotionUsed(potion.potionId)) return;
            var health = dungeon.player.GetComponent<Player.PlayerHealth>();
            if (!health.Rules.TryHeal(config.potionHeal)) return;

            run.MarkPotionUsed(potion.potionId);
            potion.gameObject.SetActive(false);
            audioManager.Play(AudioManager.Heal);
        }

        public void PlayerAttack(Vector3 origin, Vector3 facing)
        {
            audioManager.Play(AudioManager.Attack);
            foreach (var enemy in dungeon.enemies)
            {
                if (!enemy.gameObject.activeInHierarchy || enemy.state == EnemyState.Dead) continue;
                if (!SwordArc.Contains(origin, facing, enemy.transform.position, config.attackRange + 0.4f, config.attackHalfAngle)) continue;

                audioManager.Play(AudioManager.Hit);
                if (enemy.TakeHit(config.attackDamage, origin))
                {
                    run.MarkEnemyDefeated(enemy.enemyId);
                    enemy.gameObject.SetActive(false);
                }
            }

            var boss = dungeon.boss;
            if (boss != null && boss.Brain != null && boss.Brain.IsActive &&
                SwordArc.Contains(origin, facing, boss.transform.position, config.attackRange + config.bossBodyRadius, config.attackHalfAngle))
            {
                audioManager.Play(AudioManager.Hit);
                if (boss.TakeHit(config.attackDamage)) OnBossDefeated();
            }
        }

        void OnBossDefeated()
        {
            run.DefeatBoss();
            timer.Split("BossDefeat");
            dungeon.exitGate.SetActive(false);
            audioManager.Play(AudioManager.BossDefeat);
            audioManager.PlayBgm(AudioManager.BgmExplore);
            Flash("THE EXIT HAS OPENED");
        }

        public void DamagePlayer(int amount)
        {
            if (CurrentFlow != GameFlow.Playing || dungeon == null) return;
            var health = dungeon.player.GetComponent<Player.PlayerHealth>();
            if (!health.Rules.TryDamage(amount, config.hitInvincibility)) return;

            audioManager.Play(AudioManager.Hurt);
            if (health.Rules.IsDead) OnPlayerDied();
        }

        void OnPlayerDied()
        {
            pendingOutcome = livesRules.OnPlayerDied();
            flowMachine.BeginDying();
            dyingRemaining = config.dyingDuration;
            Flash(pendingOutcome == DeathOutcome.Respawn ? "YOU FELL..." : "");
        }

        /// <summary>残機ありで倒れた後。金貨・倒した敵・使った回復薬は保持し、プレイヤー・生きている敵・ボスを戻す。</summary>
        void RespawnInDungeon()
        {
            var pos = dungeon.RespawnPosition(run.RespawnPointId);
            dungeon.player.ResetTo(pos);
            dungeon.player.GetComponent<Player.PlayerHealth>().ResetHealth();
            dungeon.player.GetComponent<Player.PlayerCombat>().ResetCombat();
            dungeon.cameraFollow.SnapToTarget();

            foreach (var enemy in dungeon.enemies)
            {
                bool defeated = run.IsEnemyDefeated(enemy.enemyId);
                enemy.gameObject.SetActive(!defeated);
                if (!defeated) enemy.ResetToHome();
            }

            if (!run.BossDefeated) dungeon.boss.ResetBoss();
            arenaActive = false;
            audioManager.PlayBgm(AudioManager.BgmExplore);
        }

        public void OnArenaEntered()
        {
            if (arenaActive || run.BossDefeated || !ControlsEnabled) return;
            arenaActive = true;
            dungeon.boss.Activate();
            audioManager.PlayBgm(AudioManager.BgmBoss);
        }

        public void TryEscape()
        {
            if (!ControlsEnabled || !run.ExitOpen) return;

            timer.Finish();
            if (best.TryUpdate(timer.Elapsed, timer.Splits))
            {
                PlayerPrefs.SetString(BestRecordKey, best.ToJson());
                PlayerPrefs.Save();
                newBest = true;
            }
            else newBest = false;

            flowMachine.Clear();
            audioManager.Play(AudioManager.Clear);
            OpenDialog(SceneDirector.ClearDialog);
        }

        bool newBest;

        void UpdatePromptsAndSave()
        {
            savePromptVisible = false;
            string text = null;
            var playerPos = PlayerPosition;

            foreach (var sp in dungeon.savePoints)
            {
                if (Vector3.Distance(Flat(sp.transform.position), Flat(playerPos)) > config.saveRange) continue;
                savePromptVisible = true;
                text = "PRESS E / (Y) TO PRAY AND SAVE";
                if (input.Save)
                {
                    livesRules.Save(sp.savePointId);
                    foreach (var other in dungeon.savePoints) other.SetActiveSave(other == sp);
                    audioManager.Play(AudioManager.Save);
                    Flash("SAVED");
                }
                break;
            }

            if (text == null && dungeon.bossDoor.activeSelf &&
                Vector3.Distance(Flat(dungeon.bossDoor.transform.position), Flat(playerPos)) <= config.doorHintRange)
            {
                text = $"THE DOOR IS SEALED - COINS {run.CoinCount}/{RunState.CoinsToOpenDoor}";
            }

            prompt = text;
        }

        #endregion

        #region View refresh

        void Flash(string message)
        {
            flashMessage = message;
            flashRemaining = 2f;
        }

        void RefreshHud()
        {
            if (hud == null) return;
            var health = dungeon != null ? dungeon.player.GetComponent<Player.PlayerHealth>() : null;

            hud.SetCoins(run.CoinCount, RunState.CoinsToOpenDoor);
            hud.SetLives(run.Lives);
            hud.SetHearts(health != null ? health.Hearts : 0);
            hud.SetTimer(RtaTimer.Format(timer.Elapsed));

            if (flashRemaining > 0f) flashRemaining -= Time.unscaledDeltaTime;
            string shown = flashRemaining > 0f && !string.IsNullOrEmpty(flashMessage) ? flashMessage : prompt;
            if (CurrentFlow != GameFlow.Playing && CurrentFlow != GameFlow.Dying) shown = "";
            hud.SetPrompt(shown);

            var boss = dungeon != null ? dungeon.boss : null;
            bool showBoss = boss != null && boss.Brain != null && arenaActive && !run.BossDefeated;
            hud.SetBoss(showBoss, boss != null && boss.Brain != null ? boss.Brain.Hp : 0, config.bossTimings.maxHp);
        }

        void RefreshTitle()
        {
            if (title == null) return;
            title.SetBest(best.HasRecord ? "BEST  " + RtaTimer.Format(best.BestTime) : "BEST  --:--.--");
        }

        DialogView CurrentDialogView()
        {
            var top = SceneDirector.TopDialog();
            foreach (var d in dialogs)
                if (d != null && d.DialogName == top) return d;
            return null;
        }

        void RefreshDialog()
        {
            var view = CurrentDialogView();
            if (view == null) return;

            dialogOptions = OptionsFor(view.DialogName);
            switch (view.DialogName)
            {
                case SceneDirector.PauseDialog:
                    view.SetContent("PAUSED", "TIME  " + RtaTimer.Format(timer.Elapsed), dialogOptions);
                    break;
                case SceneDirector.GameOverDialog:
                    view.SetContent("GAME OVER", $"COINS {run.CoinCount}/5    TIME {RtaTimer.Format(timer.Elapsed)}\nCONTINUE STARTS OVER FROM THE BEGINNING", dialogOptions);
                    break;
                case SceneDirector.ClearDialog:
                    view.SetContent("ESCAPED!", ClearBody(), dialogOptions);
                    break;
            }
            view.SetSelection(dialogSelection);
        }

        static string[] OptionsFor(string dialogScene)
        {
            switch (dialogScene)
            {
                case SceneDirector.PauseDialog: return new[] { "RESUME", "RESTART", "TITLE" };
                case SceneDirector.GameOverDialog: return new[] { "CONTINUE", "TITLE" };
                case SceneDirector.ClearDialog: return new[] { "RETRY", "TITLE" };
                default: return new string[0];
            }
        }

        string ClearBody()
        {
            var sb = new StringBuilder();
            sb.Append("CLEAR TIME  ").Append(RtaTimer.Format(timer.Elapsed)).Append('\n');
            if (newBest) sb.Append("NEW BEST!\n");
            else
            {
                float diff = best.DiffFrom(timer.Elapsed);
                sb.Append("BEST  ").Append(RtaTimer.Format(best.BestTime)).Append("  (+").Append(RtaTimer.Format(diff)).Append(")\n");
            }
            sb.Append('\n');
            foreach (var s in timer.Splits)
                sb.Append(s.name.PadRight(11)).Append(RtaTimer.Format(s.time)).Append('\n');
            return sb.ToString();
        }

        void Publish()
        {
            flow = CurrentFlow.ToString();
            loadedScenes = SceneDirector.LoadedScenesText();
            topDialog = SceneDirector.TopDialog();
            sceneBusy = scenes.Busy;
            coinCount = run.CoinCount;
            lives = run.Lives;
            respawnPointId = run.RespawnPointId;
            bossDoorOpen = run.BossDoorOpen;
            bossDefeated = run.BossDefeated;
            exitOpen = run.ExitOpen;
            elapsed = timer.Elapsed;
            bestTime = best.HasRecord ? best.BestTime : -1f;
            lastSounds = audioManager.HistoryText;
            bgm = audioManager.CurrentBgm;
            lastSceneOpSeconds = scenes.LastDuration;

            var sb = new StringBuilder();
            foreach (var s in timer.Splits)
            {
                if (sb.Length > 0) sb.Append('|');
                sb.Append(s.name).Append(' ').Append(RtaTimer.Format(s.time));
            }
            splitsText = sb.ToString();

            if (dungeon != null)
            {
                hearts = dungeon.player.GetComponent<Player.PlayerHealth>().Hearts;
                var boss = dungeon.boss;
                bossHp = boss != null && boss.Brain != null ? boss.Brain.Hp : 0;
                bossState = boss != null && boss.Brain != null ? boss.Brain.State.ToString() : "";
            }
            if (CurrentFlow != GameFlow.Playing) savePromptVisible = false;
        }

        static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

        #endregion
    }
}
