# Implementation Plan: 古城脱出 ― 五枚の金貨

**Branch**: `feature/fantasy-coin-escape-game` | **Date**: 2026-11-17 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/001-fantasy-coin-escape/spec.md`

## Summary

見下ろし3Dの迷宮探索アクション。金貨5枚でボスの間が開き、ボスを倒すと出口が開いて脱出でクリア。剣の近接攻撃、ザコ敵2種、突進と範囲攻撃を使うボス、ハート・残機・セーブ台座、RTA 向けのタイマー・区間タイム・ベストタイム・即リスタートを持つ。

技術方針: シーンとダイアログは Additive 読み込みで管理（常駐の Boot に Title / Dungeon / HUD / 各ダイアログを重ねる。research R1）、迷宮は文字のマップ定義から Editor 拡張で生成（R2）、敵は NavMesh を使わず縄張り内で直接追う（R3）、入力は Input System のアクション（R4）。進行ルールは純粋な C# クラスにして EditMode テストで確かめ、Play 中の動作は unity-cli で実際に入力を送って確かめる。見た目は仮素材で先に完成させ、Asset Store 素材は後から Visual 子オブジェクトを差し替える（R8）。

## Technical Context

**Language/Version**: C#（Unity 6000.6.4f1）

**Primary Dependencies**: Input System 1.14.2、uGUI、Unity Test Framework 1.1.33（いずれも導入済み）。新しいパッケージは追加しない

**Storage**: PlayerPrefs（ベストタイムと区間タイムのみ）

**Testing**: EditMode テスト（NUnit）＋ unity-cli による Play 中の操作と状態取得、Game View の画像

**Target Platform**: Windows の Unity Editor 上での Play（ビルドはスコープ外）

**Project Type**: Unity ゲーム（単一プロジェクト）

**Performance Goals**: 60fps 目安。即リスタートから操作可能まで1秒以内（SC-006）

**Constraints**: Built-in レンダーパイプライン、外部素材はユーザーが入手したものだけ、制限時間なし

**Scale/Scope**: 1シーン、迷宮1エリア（部屋7〜9）、ザコ敵10〜14体、ボス1体、金貨5、台座3、回復薬3。初見10分前後

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原則 | 計画での対応 | 判定 |
|------|-------------|------|
| I. 実物で確認する | quickstart の Q1〜Q18 を状態の再取得と画像で確認し、verification.md に判定を記録 | ✅ |
| II. 操作して確かめる | 検証は `input_keyboard` / `input_gamepad` で行う。状態を書き換える検証用操作は作らない（cli-state.md） | ✅ |
| III. ロジックを分けてテスト | RunState・LivesAndRespawn・RtaTimer・BossBrain の遷移を純粋な C# にし、`filter: CryptEscape.Tests` で実行 | ✅ |
| IV. 小さく段階的に | 下記のフェーズを P1 から順に。各フェーズの終わりに Play で確認してコミット | ✅ |
| V. 状態を読めるようにする | `GameDirector` に状態を集約して公開。見た目は Visual 子に分離 | ✅ |
| 技術上の制約 | Input System のみ、新規パッケージなし、素材はユーザー入手分のみ | ✅ |

Phase 1 設計後の再確認: data-model・contracts・quickstart はいずれも上記を満たす。違反なし。

## 実装フェーズ

各フェーズの最後に「compile error 0 → テスト → Play で確認 → 記録 → コミット」を行う。

| フェーズ | 内容 | 主な確認（quickstart） |
|----------|------|------------------------|
| 1. 土台 | フォルダ・asmdef・入力アクション・GameConfig・runInBackground。ルールクラスとテスト | テスト合格 |
| 2. 迷宮とプレイヤー（US1 の一部） | マップ定義と DungeonBuilder、プレイヤー移動・カメラ、金貨、ボスの扉、HUD | Q1, Q2, Q3 |
| 3. 戦闘（US2） | 剣の攻撃・回避・被弾と無敵、スライム、ゴブリン | Q4, Q5, Q6 |
| 4. 残機とセーブ（US3） | 台座での保存、倒れる→復活／ゲームオーバー、回復薬 | Q7〜Q10 |
| 5. ボスと脱出（US4, US1 完成） | ボスの行動と予兆、体力バー、出口、クリア | Q11〜Q13 |
| 6. RTA と画面（US5, US6） | タイマー・区間・ベスト保存・即リスタート、タイトル・ポーズ・ゲームオーバー・クリア画面、音の仕組み | Q14〜Q17 |
| 7. 仕上げ | ゲームパッド確認、通しプレイ、数値の調整、検証記録 | Q18、SC-001〜SC-003 |
| 8. 素材の差し替え（US7） | ユーザーが Asset Store 素材を取り込んだ後に実施 | SC-007 |

## Project Structure

### Documentation (this feature)

```text
specs/001-fantasy-coin-escape/
├── spec.md
├── plan.md              # このファイル
├── research.md          # Phase 0
├── data-model.md        # Phase 1
├── quickstart.md        # Phase 1（検証ガイド）
├── contracts/
│   ├── cli-state.md     # unity-cli から読む状態の名前
│   └── input-actions.md # 入力アクションとバインド
├── tasks.md             # /speckit-tasks で作成
├── verification.md      # 実装時に作成（検証記録）
└── evidence/            # 実装時に作成（根拠画像）
```

### Source Code (repository root)

```text
Assets/Game/
├── Scenes/              # すべて Additive で管理（research R1）
│   ├── Boot.unity           # 常駐: GameDirector, GameInput, AudioManager, SceneDirector
│   ├── Title.unity
│   ├── Dungeon.unity        # 迷宮・プレイヤー・敵・ボス・カメラ
│   ├── HUD.unity
│   ├── PauseDialog.unity
│   ├── GameOverDialog.unity
│   └── ClearDialog.unity
├── Scripts/
│   ├── CryptEscape.asmdef
│   ├── Core/            # Unity 非依存のルール: RunState, LivesAndRespawn, RtaTimer, BestRecord, BossBrain(遷移), GameFlowMachine
│   ├── Game/            # GameDirector, SceneDirector, DungeonContext, GameInput, GameConfig, ButtonEdge, AudioManager
│   ├── Player/          # PlayerMotor, PlayerCombat, PlayerHealth, CameraFollow
│   ├── Enemies/         # EnemyBase, Slime, Goblin, BossController, AttackTelegraph
│   ├── World/           # Coin, Potion, SavePoint, BossDoor, ExitZone
│   └── UI/              # HudView, TitleView, PauseView, GameOverView, ClearView
├── Editor/
│   ├── CryptEscape.Editor.asmdef
│   └── DungeonBuilder.cs      # マップ定義からシーンに迷宮を生成
├── Input/
│   └── CryptEscape.inputactions
├── Data/
│   ├── GameConfig.asset
│   └── DungeonLayout.txt      # 文字のマップ定義
├── Prefabs/             # Player, Slime, Goblin, Boss, Coin, Potion, SavePoint, Floor, Wall, Door など（本体＋Visual 子）
├── Materials/           # 仮素材の色
└── Audio/               # 素材が入ったら配置

Assets/Tests/CryptEscape/EditMode/
├── CryptEscape.Tests.EditMode.asmdef
└── *Tests.cs
```

**Structure Decision**: 単一の Unity プロジェクト。ゲームのコードは `Assets/Game/` にまとめ、既存の `Assets/Scenes/SampleScene.unity` とは分ける。シーンは常駐の Boot に、画面ごとのシーンとダイアログのシーンを Additive で重ねる。シーンをまたぐ参照は、各シーンのルートのコンポーネントが読み込み時に GameDirector へ登録する。Unity 非依存のルール（Core）と、MonoBehaviour（Game/Player/Enemies/World/UI）を分け、Core だけを EditMode テストの対象にする。

## Complexity Tracking

違反なし。
