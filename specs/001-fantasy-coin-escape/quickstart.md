# Quickstart: 検証ガイド

実装した機能が spec のとおり動くことを、unity-cli で確かめる手順。状態の名前は [contracts/cli-state.md](contracts/cli-state.md)、入力は [contracts/input-actions.md](contracts/input-actions.md) を参照。

## 前提

```bash
unity-cli --output json system ping          # versionCheck.status が match
unity-cli doctor --output json               # healthy
```

- `PlayerSettings.runInBackground` が true
- シーン `Assets/Game/Scenes/CryptEscape.unity` を開いている
- compile error 0（`raw get_compilation_state`）

## 自動テスト

```bash
unity-cli --output json raw run_tests --json '{"testMode":"EditMode","filter":"CryptEscape.Tests"}'
unity-cli --output json raw get_test_status --json '{"includeTestResults":true}'
```

期待: すべて Passed、件数が tasks.md の想定と一致。0件・スキップありは不合格。

## プレイ検証の基本ループ

1. `raw play_game` → `editor status` で isPlaying=true
2. `raw manage_windows {"action":"focus","windowType":"GameView"}`
3. タイトルで Submit（Enter）を送る → `GameDirector.flow` が Playing、`elapsed` が増えていることを確認
4. 入力を送る（例: `raw input_keyboard {"key":"w","action":"press","holdSeconds":1.0}`）→ 解除の分まで待つ
5. `GameDirector` と対象の GameObject を再取得して比べる
6. 必要なら `raw capture_screenshot {"captureMode":"game","includeUI":true}` → 画像を開いて確認
7. 終了時: 押されたままのキーがない（`get_current_input_state`）→ `raw stop_game` → isPlaying=false

## シナリオ（spec の受け入れシナリオとの対応）

| # | シナリオ | 主な確認 | spec |
|---|----------|----------|------|
| Q1 | 開始直後の HUD | coinCount=0, hearts=5, lives=3, elapsed が増える、HUD 画像 | US1-1 |
| Q2 | 金貨を1枚取る／同じ場所を再通過 | coinCount 0→1→1、Coin_n 非アクティブ、splits に Coin1 | US1-2, US5-4 |
| Q3 | 4枚で扉に近づく → 5枚目を取る | bossDoorOpen false→true、lastSounds に DoorOpen | US1-3, US1-4 |
| Q4 | ザコ敵を攻撃する／空振り | 敵 hp 減少→0で非アクティブ、範囲外では不変 | US2-1, US2-2 |
| Q5 | 被弾と無敵時間 | hearts 減少、無敵中の連続被弾で減らない | US2-3 |
| Q6 | スライムの追跡・ゴブリンの巡回→発見 | 敵の座標の変化、状態名 | US2-4, US2-5 |
| Q7 | 未保存で倒れる | lives 3→2、開始地点で hearts=5 | US3-1 |
| Q8 | 台座で保存→金貨を取る→倒れる | respawnPointId 更新、hearts 不変、復活後 coinCount 保持 | US3-2, US3-6 |
| Q9 | 倒した敵／生きている敵の復活時 | 倒した敵は非アクティブのまま、生きている敵は初期位置・満タン | US3-3 |
| Q10 | 残機0でゲームオーバー→続ける | flow=GameOver、続けると全初期化 | US3-5 |
| Q11 | ボスの予兆と回避 | bossState の遷移、予兆中に横へ移動して hearts 不変 | US4-1〜4-3 |
| Q12 | ボス撃破→出口→クリア | bossDefeated, exitOpen, flow=Cleared, splits 7件, CLEAR 画像 | US1-5, US4-5, US5-5 |
| Q13 | ボス戦中に倒れる | bossHp が満タンに戻る | US3-4 |
| Q14 | ポーズ中のタイマー停止 | Paused の間 elapsed 不変 | US5-2 |
| Q15 | 即リスタート | R で flow=Playing、elapsed≈0、全初期化、1秒以内 | US5-7 |
| Q16 | ベストタイムの保存 | クリア→Play 停止→再 Play でタイトルに bestTime | US5-6 |
| Q17 | 効果音と BGM | lastSounds / bgm の値（素材が入るまでは名前の記録のみ） | US6-3, US6-4 |
| Q18 | ゲームパッド入力 | `input_gamepad` で移動・攻撃が効く | FR-050 |

結果は `specs/001-fantasy-coin-escape/verification.md` に「操作・期待・実際・根拠・判定」で記録し、画像は `specs/001-fantasy-coin-escape/evidence/` に置く。
