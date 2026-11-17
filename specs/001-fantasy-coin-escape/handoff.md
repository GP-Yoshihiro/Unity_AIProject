# 実装レポート（引き継ぎ）: 古城脱出 ― 五枚の金貨

- ブランチ: `feature/fantasy-coin-escape-game`（main から作成）
- 日付: 2026-11-17
- 状態: **P1〜P2 の実装はひととおり完了。プレイ検証は途中**（ボス戦・クリアまで未到達）

## 1. 現在の到達点

| 区分 | 状態 |
|------|------|
| 仕様・設計 | spec / plan / research / data-model / contracts / quickstart / tasks / constitution 作成済み |
| ルール（Core） | 8クラス実装、EditMode テスト 41/41 合格 |
| シーン構成 | Additive 管理の7シーン（Boot, Title, Dungeon, HUD, PauseDialog, GameOverDialog, ClearDialog） |
| ゲーム本体 | プレイヤー（移動・剣・回避・ハート）、スライム、ゴブリン、ボス（突進・範囲攻撃・予兆）、金貨・回復薬・台座・ボスの扉・出口、HUD・タイトル・各ダイアログ、RTA タイマー・区間・ベスト保存、効果音/BGM の仕組み |
| 迷宮 | 37×29 マス（1マス2m）。金貨5・台座3・回復薬3・スライム6・ゴブリン5・ボス1 |
| プレイ検証 | Additive 遷移、Q1・Q2・Q4〜Q10 を確認済み。Q3・Q11〜Q16・Q18 は未確認（`verification.md`） |
| 素材 | Asset Store の候補を調査済み（`assets.md`）。取り込みはユーザー作業待ち。現在は仮素材（標準図形） |

## 2. 構成

### シーン（Additive、`Assets/Game/Scenes/`）

| シーン | 役割 |
|--------|------|
| Boot | 常駐。`/Game` に GameDirector・GameInput・SceneDirector・AudioManager |
| Title | タイトル画面（背景カメラ付き） |
| Dungeon | 迷宮・プレイヤー・敵・ボス・カメラ・ライト。ルートの `DungeonContext` が参照をまとめる |
| HUD | 金貨・ハート・残機・タイマー・案内・ボスの体力バー |
| PauseDialog / GameOverDialog / ClearDialog | 開くときに Additive で読み込み、閉じるときに破棄 |

- Play ボタンを押すと、開いているシーンに関係なく Boot から始まる（`EditorSceneManager.playModeStartScene`）
- 即リスタート／ゲームオーバー後の続行は Dungeon を読み直して全初期化（実測 0.05 秒）
- 各シーンのルートのコンポーネントが読み込み時に `GameDirector.Register(...)` で自分を登録する

### コード（`Assets/Game/Scripts/`）

| フォルダ | 内容 |
|----------|------|
| Core | Unity 非依存のルール: GameFlowMachine, RunState, LivesAndRespawn, PlayerHealthRules, SwordArc, BossBrain, RtaTimer, BestRecord |
| Game | GameDirector（進行の中心）, SceneDirector, GameInput, ButtonEdge, GameConfig, AudioManager, DungeonContext |
| Player | PlayerMotor, PlayerCombat, PlayerHealth, CameraFollow |
| Enemies | EnemyBase, Slime, Goblin, BossController, AttackTelegraph |
| World | Coin, Potion, SavePoint, ExitZone, BossArena |
| UI | HudView, TitleView, DialogView |

### 生成ツール

- `Assets/Game/Editor/CryptEscapeBuilder.cs`: 入力アセット・設定・マテリアル・Prefab・7シーン・Build Settings を一括生成
  - unity-cli: `unity-cli editor eval --mode statements "return CryptEscape.EditorTools.CryptEscapeBuilder.BuildAll(false);"`
  - メニュー: CryptEscape > Build All
  - Prefab は既存なら作り直さない（素材の差し替えを守るため）。作り直すときは `BuildAll(true)`
- `Assets/Game/Data/DungeonLayout.txt`: 迷宮のマップ定義（記号はファイル先頭に記載）。`specs/001-fantasy-coin-escape/tools/make_layout.py` で生成・到達可能性を検証
- `Assets/Game/Data/GameConfig.asset`: 調整用の数値

## 3. 検証の進め方（次の担当向け）

- 手順は `quickstart.md`、状態の名前は `contracts/cli-state.md`
- テスト用 PowerShell ヘルパー: `specs/001-fantasy-coin-escape/tools/`
  - `ce.ps1`: unity-cli の呼び出し、状態表示（`S`）、Play 開始/停止、撮影、テスト実行（runId 照合つき）
  - `nav.ps1`: マップ定義から経路を引いて移動（`GoToCell r c -Fight`）、近くの敵と戦う、ボス戦（`BossFight`、未使用）
  - 使い方: `. tools\ce.ps1; . tools\nav.ps1`（スクリプト冒頭の `Set-Location` を自分の環境に合わせる）
- テストは必ず `filter: "CryptEscape.Tests"` を付けて、`runId` が一致する結果を見る。開いているシーンに未保存の変更があると `run_tests` は拒否される

## 4. 未完了・既知の課題

| 優先 | 内容 | メモ |
|------|------|------|
| 高 | **ボス戦〜クリアの実プレイ確認**（Q3, Q11〜Q13, Q16） | AI の操作では金貨5枚まで到達できていない（次項） |
| 高 | **AI テスト操作が戦闘で負けやすい／移動ツールが止まることがある** | CLI 1回で約0.2秒遅れる。最後の実行では `GoToCell 26 2` が (23,9) 付近から進まなくなった（原因未特定。`MoveAxis` の補正が収束していない可能性）。対策案: (1) 移動ツールのデバッグ、(2) ボス戦などの単体シナリオは「前提（金貨5枚・ボスの間の手前）」を eval で用意し、戦闘そのものは実入力で確認する（前提の用意であることを記録に明記） |
| 中 | ポーズ（Q14）、R キーのリスタート（Q15）、ゲームパッド（Q18）の確認 | 実装済み・未検証 |
| 中 | 難易度の確認 | AI 操作に合わせて敵を少しゆるめた（`verification.md`）。人が遊んで再調整する |
| 中 | Asset Store 素材の差し替え（US7, T071〜T075） | ユーザーが `assets.md` の手順で取り込み後に実施 |
| 低 | UI は英語 | 日本語化は日本語フォント導入後（research R7） |
| 低 | GameInput は C# ラッパーを生成せず、アクション名で参照している | tasks T007 からの変更。動作に問題なし |

## 5. 次にやること（推奨順）

1. `nav.ps1` の移動が止まる原因を調べる（または前提を eval で用意する方式に切り替える）
2. Q3 → Q11〜Q13 → Q16 を確認し、クリア画面の画像を残す
3. Q14・Q15（R キー）・Q18 を確認
4. tasks.md の Phase 10（通しプレイ、数値調整、最終確認）
5. 素材が届いたら Phase 9（US7）

## 6. コミット履歴（このブランチ）

- 仕様書 → 計画・設計・tasks・constitution → Core とテスト（Additive 方式へ設計変更）→ ゲーム本体と7シーン → 本コミット（検証記録・調整・引き継ぎ）
