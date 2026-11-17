---
description: "Task list for 古城脱出 ― 五枚の金貨"
---

# Tasks: 古城脱出 ― 五枚の金貨

**Input**: Design documents from `/specs/001-fantasy-coin-escape/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: constitution III により、Unity 非依存のルール（`Assets/Game/Scripts/Core/`）は EditMode テストを先に書く。Play 中の動作は quickstart の Q1〜Q18 を unity-cli で確認し、`verification.md` に記録する。

**Progress (2026-11-17)**: 実装は P1〜P2 まで完了。検証タスク T030・T042 は一部、T057・T065・T070 と Phase 9〜10 は未完了。詳細は handoff.md と verification.md。

**Organization**: spec のユーザーストーリーごとにフェーズを分ける。P1 は US1〜US4、P2 は US5〜US6、P3 は US7。

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 並行して進められる（別ファイル・未完了タスクに依存しない）
- **[Story]**: 対応するユーザーストーリー（US1〜US7）

## Path Conventions

- ゲームのコード: `Assets/Game/Scripts/{Core,Game,Player,Enemies,World,UI}/`
- Editor 拡張: `Assets/Game/Editor/`
- テスト: `Assets/Tests/CryptEscape/EditMode/`
- 検証記録: `specs/001-fantasy-coin-escape/verification.md`、画像は `specs/001-fantasy-coin-escape/evidence/`

## 各タスク共通の完了条件

- compile error 0（`raw get_compilation_state`）
- テストを含むタスクは `run_tests`（`filter: "CryptEscape.Tests"`）で該当テストが Passed。0件・スキップは不合格
- シーンを変更したタスクは保存し、`get_hierarchy` で再取得して確認

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: フォルダ・アセンブリ・設定の準備

- [x] T001 Create folders `Assets/Game/{Scenes,Scripts/{Core,Game,Player,Enemies,World,UI},Editor,Input,Data,Prefabs,Materials,Audio}` and `Assets/Tests/CryptEscape/EditMode/`
- [x] T002 [P] Create runtime assembly `Assets/Game/Scripts/CryptEscape.asmdef`（references: Unity.InputSystem, UnityEngine.UI）
- [x] T003 [P] Create editor assembly `Assets/Game/Editor/CryptEscape.Editor.asmdef`（Editor only, references CryptEscape）
- [x] T004 [P] Create test assembly `Assets/Tests/CryptEscape/EditMode/CryptEscape.Tests.EditMode.asmdef`（Editor only, references CryptEscape + TestRunner, nunit.framework.dll, UNITY_INCLUDE_TESTS）
- [x] T005 Set `PlayerSettings.runInBackground = true` via unity-cli eval and confirm in `ProjectSettings/ProjectSettings.asset`（research R11）
- [x] T006 Create scenes `Boot`, `Title`, `Dungeon`, `HUD`, `PauseDialog`, `GameOverDialog`, `ClearDialog` in `Assets/Game/Scenes/` and register them in Build Settings with Boot first（Additive 管理、research R1）

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: すべてのストーリーが使う入力・設定・ゲーム状態の土台

**⚠️ CRITICAL**: 完了するまでユーザーストーリーに着手しない

- [x] T007 Create Input Actions asset `Assets/Game/Input/CryptEscape.inputactions` with Gameplay and UI maps per `contracts/input-actions.md`, and generate the C# wrapper class `CryptEscapeInput`
- [x] T008 [P] Implement `ButtonEdge`（前フレームとの比較で押した瞬間を判定）in `Assets/Game/Scripts/Game/ButtonEdge.cs`（research R4）
- [x] T009 [P] Implement `GameConfig` ScriptableObject with all numbers from data-model.md in `Assets/Game/Scripts/Game/GameConfig.cs`, and create `Assets/Game/Data/GameConfig.asset`
- [x] T010 [P] Write tests for `GameFlowMachine` transitions（Title→Playing、Pause/Resume、Dying→Playing/GameOver、Restart from any state）in `Assets/Tests/CryptEscape/EditMode/GameFlowMachineTests.cs`
- [x] T011 Implement `GameFlowMachine`（data-model の GameFlow）in `Assets/Game/Scripts/Core/GameFlowMachine.cs` so T010 passes
- [x] T012 Implement `SceneDirector`（Additive の読み込み・破棄を順番に処理、読み込み中フラグ、loadedScenes）in `Assets/Game/Scripts/Game/SceneDirector.cs` and `GameDirector` skeleton（公開フィールドは `contracts/cli-state.md` の全項目、画面遷移ごとに SceneDirector でシーン・ダイアログを読み込み/破棄）in `Assets/Game/Scripts/Game/GameDirector.cs`, attach both to `/Game` in Boot
- [x] T013 [P] Create placeholder materials（player blue, slime green, goblin brown, boss purple, coin gold, altar cyan, potion red, floor, wall）in `Assets/Game/Materials/`
- [x] T014 Verify foundation: Play → `GameDirector.flow` が "Title"、Enter で "Playing" になる（unity-cli で入力・再取得）

**Checkpoint**: 入力と画面状態が動き、unity-cli で読める

---

## Phase 3: User Story 1 - 迷宮を探索して金貨を集め、脱出する (Priority: P1) 🎯 MVP

**Goal**: 迷宮を歩いて金貨5枚を集め、ボスの間の扉が開く。出口の仕組みも用意する（ボス撃破は US4 で接続）

**Independent Test**: quickstart Q1〜Q3。敵なしで金貨5枚 → 扉が開く

### Tests for User Story 1

- [x] T015 [P] [US1] Write tests for `RunState` coins（二重取得で増えない、5枚目で bossDoorOpen、全初期化で0）in `Assets/Tests/CryptEscape/EditMode/RunStateCoinTests.cs`
- [x] T016 [P] [US1] Write tests for exit rule（bossDefeated で exitOpen、未撃破では閉じている）in `Assets/Tests/CryptEscape/EditMode/RunStateExitTests.cs`

### Implementation for User Story 1

- [x] T017 [US1] Implement `RunState`（coins, bossDoorOpen, bossDefeated, exitOpen, FullReset）in `Assets/Game/Scripts/Core/RunState.cs` so T015/T016 pass
- [x] T018 [P] [US1] Write dungeon layout（開始の広間、金貨の部屋5つ前後、台座3、ボスの間、出口。記号表をファイル先頭に記載）in `Assets/Game/Data/DungeonLayout.txt`
- [x] T019 [P] [US1] Create placeholder prefabs with body + `Visual` child（Floor, Wall, Coin, BossDoor, ExitGate, ExitZone）in `Assets/Game/Prefabs/`
- [x] T020 [US1] Implement `DungeonBuilder` editor tool（layout → `/Dungeon` 配下に生成、名前は contracts/cli-state.md に従う、再生成で前回分を消す）in `Assets/Game/Editor/DungeonBuilder.cs`
- [x] T021 [US1] Run DungeonBuilder via unity-cli, save scene, re-fetch hierarchy and capture Scene/Game images to confirm layout
- [x] T022 [P] [US1] Implement `PlayerMotor`（8方向移動、Rigidbody 速度、向き facing）in `Assets/Game/Scripts/Player/PlayerMotor.cs`
- [x] T023 [P] [US1] Implement `CameraFollow`（斜め上から追従）in `Assets/Game/Scripts/Player/CameraFollow.cs`
- [x] T024 [US1] Create Player prefab（capsule body + Visual, PlayerMotor）in `Assets/Game/Prefabs/Player.prefab` and place at start point
- [x] T025 [P] [US1] Implement `Coin`（触れたら GameDirector に通知、取得済みは非アクティブ）in `Assets/Game/Scripts/World/Coin.cs`
- [x] T026 [P] [US1] Implement `BossDoor`（bossDoorOpen で開く、閉じているとき近づくと "COINS 4/5" 表示）in `Assets/Game/Scripts/World/BossDoor.cs`
- [x] T027 [P] [US1] Implement `ExitZone` and `ExitGate`（exitOpen で柵が消え、触れると GameDirector がクリア）in `Assets/Game/Scripts/World/ExitZone.cs`
- [x] T028 [US1] Implement `HudView`（COIN n/5、ハート、残機、タイマーの枠。縦 Overflow）in `Assets/Game/Scripts/UI/HudView.cs` and build `/UI/HUD`
- [x] T029 [US1] Wire RunState into `GameDirector`（Coin 通知、扉、出口、公開フィールド更新）in `Assets/Game/Scripts/Game/GameDirector.cs`
- [ ] T030 [US1] Verify Q1〜Q3 with unity-cli（移動・金貨取得・再通過・4枚で扉・5枚目で開放）and record in `specs/001-fantasy-coin-escape/verification.md` with images in `evidence/`

**Checkpoint**: 金貨5枚で扉が開く迷宮を歩ける（MVP）

---

## Phase 4: User Story 2 - ザコ敵と剣で戦う (Priority: P1)

**Goal**: 剣の攻撃・回避・被弾と無敵、スライムとゴブリン

**Independent Test**: quickstart Q4〜Q6。敵1体の部屋で倒す・被弾する

### Tests for User Story 2

- [x] T031 [P] [US2] Write tests for `PlayerHealthRules`（被弾で減る、無敵中は減らない、同時被弾は1回、0で倒れる、回復は上限5）in `Assets/Tests/CryptEscape/EditMode/PlayerHealthRulesTests.cs`
- [x] T032 [P] [US2] Write tests for `SwordArc`（前方1.6m・±60°の内外判定）in `Assets/Tests/CryptEscape/EditMode/SwordArcTests.cs`

### Implementation for User Story 2

- [x] T033 [P] [US2] Implement `PlayerHealthRules` in `Assets/Game/Scripts/Core/PlayerHealthRules.cs` so T031 passes
- [x] T034 [P] [US2] Implement `SwordArc` in `Assets/Game/Scripts/Core/SwordArc.cs` so T032 passes
- [x] T035 [US2] Implement `PlayerCombat`（攻撃・再使用0.4秒、回避3m/0.3秒無敵/再使用0.8秒）in `Assets/Game/Scripts/Player/PlayerCombat.cs`
- [x] T036 [US2] Implement `PlayerHealth`（PlayerHealthRules を使う、被弾時の点滅）in `Assets/Game/Scripts/Player/PlayerHealth.cs`
- [x] T037 [US2] Implement `EnemyBase`（hp、縄張り、Idle/Chase/Attack/Return/Stagger/Dead、Reset、enemyId）in `Assets/Game/Scripts/Enemies/EnemyBase.cs`
- [x] T038 [P] [US2] Implement `Slime`（追跡して体当たり）in `Assets/Game/Scripts/Enemies/Slime.cs` and prefab `Assets/Game/Prefabs/Slime.prefab`
- [x] T039 [P] [US2] Implement `Goblin`（Waypoint 巡回、発見で予備動作0.4秒→攻撃）in `Assets/Game/Scripts/Enemies/Goblin.cs` and prefab `Assets/Game/Prefabs/Goblin.prefab`
- [x] T040 [US2] Extend DungeonBuilder for `S`/`G` symbols and goblin waypoints, regenerate dungeon, save scene
- [x] T041 [US2] Show hearts in HUD and expose hearts / enemy hp to GameDirector in `Assets/Game/Scripts/Game/GameDirector.cs`
- [ ] T042 [US2] Verify Q4〜Q6 with unity-cli and record in verification.md

**Checkpoint**: 迷宮で敵と戦える

---

## Phase 5: User Story 3 - 倒れても残機とセーブポイントで続けられる (Priority: P1)

**Goal**: 台座での保存、倒れる→復活／ゲームオーバー、回復薬

**Independent Test**: quickstart Q7〜Q10

### Tests for User Story 3

- [x] T043 [P] [US3] Write tests for `LivesAndRespawn`（未保存は開始地点、保存で更新、倒れると lives−1、0でゲームオーバー、復活で金貨・倒した敵・使った回復薬を保持、全初期化）in `Assets/Tests/CryptEscape/EditMode/LivesAndRespawnTests.cs`

### Implementation for User Story 3

- [x] T044 [US3] Implement `LivesAndRespawn`（RunState の lives / respawnPointId / defeatedEnemyIds / usedPotionIds）in `Assets/Game/Scripts/Core/LivesAndRespawn.cs` so T043 passes
- [x] T045 [P] [US3] Implement `SavePoint`（範囲内で "PRESS E TO SAVE"、Save で respawnPointId 更新、体力は変えない）in `Assets/Game/Scripts/World/SavePoint.cs` and prefab
- [x] T046 [P] [US3] Implement `Potion`（ハート5なら取らない、取ると+2して消える）in `Assets/Game/Scripts/World/Potion.cs` and prefab
- [x] T047 [US3] Implement death flow in GameDirector（Dying 1.5秒 → 復活: 位置・ハート5・生きている敵とボスを初期化／GameOver）in `Assets/Game/Scripts/Game/GameDirector.cs`
- [x] T048 [US3] Implement `GameOverView`（GAME OVER、続ける＝全初期化）in `Assets/Game/Scripts/UI/GameOverView.cs` and build `/UI/GameOver`
- [x] T049 [US3] Show lives and save prompt in HUD; extend DungeonBuilder for `P`（回復薬）and `A`（台座）
- [x] T050 [US3] Verify Q7〜Q10 with unity-cli and record in verification.md

**Checkpoint**: 倒れても続けられ、残機0でゲームオーバー

---

## Phase 6: User Story 4 - ボスを倒す (Priority: P1)

**Goal**: 突進と範囲攻撃（予兆あり）を使うボス、撃破で出口が開きクリア

**Independent Test**: quickstart Q11〜Q13

### Tests for User Story 4

- [x] T051 [P] [US4] Write tests for `BossBrain`（Dormant→Idle→ChargeTell→Charge→Recover→AoeTell→Aoe→Recover の順、予兆は0.8秒以上、hp0で Defeated、Reset で満タン・Dormant、撃破と被弾が同時なら撃破優先）in `Assets/Tests/CryptEscape/EditMode/BossBrainTests.cs`

### Implementation for User Story 4

- [x] T052 [US4] Implement `BossBrain` state machine in `Assets/Game/Scripts/Core/BossBrain.cs` so T051 passes
- [x] T053 [US4] Implement `BossController`（BossBrain に従って移動・突進・範囲判定、被弾、Reset）in `Assets/Game/Scripts/Enemies/BossController.cs` and prefab `Assets/Game/Prefabs/Boss.prefab`
- [x] T054 [P] [US4] Implement `AttackTelegraph`（突進の線、範囲攻撃の円）in `Assets/Game/Scripts/Enemies/AttackTelegraph.cs`
- [x] T055 [US4] Add boss HP bar to HUD and boss arena trigger（入ると戦闘開始）; connect Defeated → RunState.bossDefeated / exitOpen in GameDirector
- [x] T056 [US4] Implement `ClearView`（クリアタイムの枠。区間とベストは US5 で追加）in `Assets/Game/Scripts/UI/ClearView.cs` and build `/UI/Clear`
- [ ] T057 [US4] Verify Q11〜Q13 with unity-cli（予兆中に横移動して避ける、撃破、出口でクリア、ボス戦で倒れて満タンに戻る）and record in verification.md with images

**Checkpoint**: 金貨5枚 → ボス撃破 → 脱出 までを通しで遊べる（P1 完成）

---

## Phase 7: User Story 5 - RTA を楽しむ (Priority: P2)

**Goal**: 経過タイマー、区間タイム、ベストタイム保存、即リスタート

**Independent Test**: quickstart Q14〜Q16

### Tests for User Story 5

- [x] T058 [P] [US5] Write tests for `RtaTimer`（Playing/Dying で進み Paused で止まる、区間は1回ずつ7件、Escape で確定、Reset）in `Assets/Tests/CryptEscape/EditMode/RtaTimerTests.cs`
- [x] T059 [P] [US5] Write tests for `BestRecord`（初回は更新、遅ければ更新しない、差の計算、JSON の保存と読み込み）in `Assets/Tests/CryptEscape/EditMode/BestRecordTests.cs`

### Implementation for User Story 5

- [x] T060 [P] [US5] Implement `RtaTimer` in `Assets/Game/Scripts/Core/RtaTimer.cs` so T058 passes
- [x] T061 [P] [US5] Implement `BestRecord`（JSON 変換）and PlayerPrefs 保存 `CryptEscape.BestRun` in `Assets/Game/Scripts/Core/BestRecord.cs` so T059 passes
- [x] T062 [US5] Wire timer and splits into GameDirector（Coin1〜5、BossDefeat、Escape）; show timer in HUD（mm:ss.ff）
- [x] T063 [US5] Implement Restart from any state（全初期化して Playing、1秒以内）in GameDirector
- [x] T064 [US5] Show clear time, split list and diff vs best in ClearView; save best on improvement
- [ ] T065 [US5] Verify Q14〜Q16 with unity-cli and record in verification.md

**Checkpoint**: RTA として遊べる

---

## Phase 8: User Story 6 - タイトル・ポーズ・音 (Priority: P2)

**Goal**: タイトル画面、ポーズ画面、効果音と BGM の仕組み

**Independent Test**: quickstart Q17 と画面遷移

### Implementation for User Story 6

- [x] T066 [P] [US6] Implement `TitleView`（はじめる、ベストタイム、操作説明）in `Assets/Game/Scripts/UI/TitleView.cs` and build `/UI/Title`
- [x] T067 [P] [US6] Implement `PauseView`（再開・最初から・タイトルへ、timeScale=0）in `Assets/Game/Scripts/UI/PauseView.cs` and build `/UI/Pause`
- [x] T068 [P] [US6] Implement `AudioManager`（名前で効果音・BGM を再生、素材なしでもエラーにしない、lastSounds と bgm を公開）in `Assets/Game/Scripts/Game/AudioManager.cs`
- [x] T069 [US6] Call AudioManager at each event in FR-043 and switch BGM at boss arena
- [ ] T070 [US6] Verify title → start → pause → resume → title, and Q17（lastSounds / bgm）with unity-cli; record in verification.md

**Checkpoint**: 1本のゲームとして起動からクリアまで遊べる

---

## Phase 9: User Story 7 - Asset Store 素材への差し替え (Priority: P3)

**Goal**: ユーザーが取り込んだ素材で見た目と音を差し替える

**Independent Test**: 差し替え後に Q1〜Q17 の主要シナリオが変わらない（SC-007）

**⚠️ 前提**: ユーザーが Asset Store で素材を入手し、プロジェクトに取り込んでいること（research R8 のカテゴリ表）

- [ ] T071 [US7] List imported assets via unity-cli, confirm license notes and Built-in RP compatibility, and record the mapping（どの素材を何に使うか）in `specs/001-fantasy-coin-escape/assets.md`
- [ ] T072 [US7] Replace `Visual` children of Player / Slime / Goblin / Boss / Coin / Potion / SavePoint prefabs, keeping colliders on the body
- [ ] T073 [US7] Replace floor / wall / door prefabs used by DungeonBuilder and regenerate the dungeon
- [ ] T074 [US7] Assign sound effects and BGM to AudioManager
- [ ] T075 [US7] Re-run main scenarios（Q1, Q2, Q4, Q5, Q8, Q11, Q12）and record before/after in verification.md

---

## Phase 10: Polish & Cross-Cutting Concerns

- [ ] T076 Verify gamepad input（Q18）with `input_gamepad`（移動・攻撃・回避・保存・ポーズ・リスタート）
- [ ] T077 Full playthrough by AI from title to clear; measure time vs SC-001 / SC-002 and adjust numbers in `Assets/Game/Data/GameConfig.asset`
- [ ] T078 Run all EditMode tests（filter CryptEscape.Tests）and record names / counts / results in verification.md
- [ ] T079 Final check: no keys held, Play stopped, scene saved, no stray objects in hierarchy; update verification.md summary（確認済み・未成立・未確認）

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)** → **Foundational (Phase 2)** → ユーザーストーリー
- **US1 (Phase 3)**: Foundational の後。迷宮とプレイヤーを作るので、以降のストーリーはこれに乗る
- **US2 (Phase 4)**: US1 の迷宮とプレイヤーが必要
- **US3 (Phase 5)**: US2 の被弾（倒れる）が必要
- **US4 (Phase 6)**: US2 の戦闘と US3 の復活（ボスのリセット）が必要
- **US5 (Phase 7)**: US1 の金貨と US4 のボス撃破（区間）が必要
- **US6 (Phase 8)**: GameFlowMachine（Phase 2）があれば着手できる。音のイベントは US1〜US4 の後に接続
- **US7 (Phase 9)**: ユーザーの素材取り込み待ち。P1/P2 完成後
- **Polish (Phase 10)**: 必要なストーリーの完成後

### Within Each User Story

- テスト（Core）を先に書いて失敗を確認 → Core 実装 → MonoBehaviour → シーン配置 → unity-cli で検証 → 記録 → コミット

### Parallel Opportunities

- Phase 1: T002, T003, T004
- Phase 2: T008, T009, T010, T013
- US1: T015/T016、T018/T019、T022/T023、T025/T026/T027
- US2: T031/T032 → T033/T034、T038/T039
- US5: T058/T059 → T060/T061
- US6: T066/T067/T068

---

## Parallel Example: User Story 1

```text
# テストを同時に書く
Task: "T015 RunState coin tests in Assets/Tests/CryptEscape/EditMode/RunStateCoinTests.cs"
Task: "T016 Exit rule tests in Assets/Tests/CryptEscape/EditMode/RunStateExitTests.cs"

# 配置物のスクリプトを同時に書く
Task: "T025 Coin in Assets/Game/Scripts/World/Coin.cs"
Task: "T026 BossDoor in Assets/Game/Scripts/World/BossDoor.cs"
Task: "T027 ExitZone/ExitGate in Assets/Game/Scripts/World/ExitZone.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1)

1. Phase 1〜2 を完了
2. US1（迷宮・移動・金貨・扉）を完了
3. **STOP and VALIDATE**: Q1〜Q3 を unity-cli で確認して記録

### Incremental Delivery

1. US1 → US2（戦闘）→ US3（残機とセーブ）→ US4（ボスとクリア）で P1 完成。ここで通しプレイができる
2. US5（RTA）→ US6（画面と音）で P2 完成。1本のゲームとして完成
3. 素材が届いたら US7 で見た目と音を差し替え
4. 各フェーズの終わりにコミット

---

## Notes

- 検証は constitution I・II に従い、状態の再取得と画像で行う。状態の直接書き換えで「動いた」としない
- 検証前に Game View へフォーカスし、タイマーが進むことを確認してから入力する（research R11）
- テストは `filter: "CryptEscape.Tests"` で実行し、シーンに余計なオブジェクトが残っていないか確認する
- 仕様にない機能を足さない（spec の Out of Scope）
