# Tasks: 神経衰弱（Memory Match）

**Input**: spec.md, plan.md。Tests は必須（スライド12）。番号は実行順

## 対応表

| FR | 受け入れ | 単体テスト | Task |
|----|---------|-----------|------|
| FR-01 | AS-01 / M1 | UT-01 DealTests | T003 |
| FR-07 | 検証の前提 | UT-07 DealTests | T003 |
| FR-02 | AS-02 / M2 | UT-02 FlipTests | T004 |
| FR-03 | AS-03a, AS-03b / M3 | UT-03 JudgeTests | T005 |
| FR-04 | AS-04 / M4 | UT-04 DuplicateTests | T006 |
| FR-05 | AS-05a, AS-05b / M5 | UT-05 EndTests | T007 |
| FR-06 | AS-06 / M6 | UT-06 RestartTests | T008 |
| SC-001〜004 | M1〜M6 | AIプレイ | T010〜T011 |

## Phase 1: 準備

- [ ] T001 接続確認（版の一致、シーン、コンパイル状態）
- [ ] T002 シーン `Assets/MemoryMatch/Scenes/MemoryMatch.unity` に Card_00〜11、カーソル、成立・ミス・スコア表示を置き、保存して Hierarchy を再取得する（ロジックなし）

## Phase 2: ルール（Red → Green → Refactor、1ルールずつ）

- [ ] T003 [US1] FR-01/07: 各絵柄2枚・全て裏・同じシードで同じ配置になる失敗テストと最小実装
- [ ] T004 [US1] FR-02: カーソル移動と端、1枚めくる、3枚目はめくれない失敗テストと最小実装
- [ ] T005 [US1] FR-03: 同じ絵柄は成立+100、違う絵柄はミス+1・-10・下限0、次のSpaceは戻すだけの失敗テストと最小実装
- [ ] T006 [US1] FR-04: 同じカード・成立済みの再選択で状態が変わらない失敗テストと最小実装
- [ ] T007 [US2] FR-05: 全成立でクリア、ミス5回でゲームオーバー、以後入力を受けない失敗テストと最小実装
- [ ] T008 [US3] FR-06: 途中・終了後のRで初期化・再シャッフルする失敗テストと最小実装

## Phase 3: 接続と実証

- [ ] T009 CardView / MemoryGame を実装しシーンへ接続、compile error 0、保存
- [ ] T010 Play中に実入力でめくり、前後の状態を取得して Stop する
- [ ] T011 M1〜M6 をAIプレイで実証し verification.md と画像に記録する

## Phase 4: 追加（SPEC改訂後）

- [ ] T012 [US4] コンボを spec.md に追記する（A1〜A4）
- [ ] T013 [US4] FR-08: 倍率の失敗テストと最小実装
- [ ] T014 [US4] 連続数の表示を接続し保存する
- [ ] T015 A1〜A4 を実証し、M1〜M6 の回帰を確認する
- [ ] T016 終了処理（入力解除・Stop・保存）、コミット、push

## Analyze 結果（実装前）

- 全FR（01〜07）に対応Taskとテストがある。対応FRのないTaskは準備・終了処理のみ
- spec と plan の用語・数値に矛盾なし。FR-08 は T012 で spec に追記するまで実装しない
- 指摘（中）: constitution が未記入。plan の Constitution Check で代替（変更範囲外のため編集しない）
