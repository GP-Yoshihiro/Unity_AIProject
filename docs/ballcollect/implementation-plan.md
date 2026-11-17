# BallCollect 実装計画

1段階ずつ実物を確認してから次へ進む。確認は unity-cli で取得した状態とGame画面で行う。

## 段階1: シーン（B1）

- `Assets/Scenes/BallCollect.unity` を作成し、床・Player・アイテム5個・固定カメラ・ライトを配置
- 既存のカメラとライトを重複させない
- 確認: 保存後に Hierarchy を再取得、Game画面で配置を確認

## 段階2: 移動（B2）

- `PlayerController`（Input System、Rigidbody の速度で移動）を作成し Player に追加
- 確認: compile error 0 → シーン保存 → Play（isPlaying を確認）→ W を1秒入力 → 入力前後の Transform で +Z を確認 → 入力解除・Stop

## 段階3: ルール（B3〜B6）

- `BallCollectRules`（回収数・クリア・初期化のロジック、Unity 非依存）
- `BallCollectGame`（GameManager、UI更新、R入力、落下判定、初期化）
- `CollectibleItem`（トリガーで GameManager に通知）
- UI: 回収数 `0/5`、`CLEAR`
- 確認: 1個回収で 1/5、途中R、落下、全回収で CLEAR、クリア後R

## 検証（第2コマ前半）

- AIが入力して、移動・回収・途中R・落下・全回収・クリア後Rを確認
- 結果を `verification.md` に記録し、根拠画像を `evidence/` に保存

## 追加: 30秒ルール（A1〜A5）

- `BallCollectRules` に残り時間と TIME UP を追加（同時発生時は回収優先）
- UI: 残り時間、`TIME UP`
- 確認: 時間切れ・時間内クリア・途中R/落下、B3〜B6 の再確認
- EditMode テスト3件: 二重回収で1、5個でクリア、初期化で0と未クリア
