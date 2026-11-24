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
| FR-08 | A1〜A4 | UT-08 ComboTests | T013 |
| FR-10 | B2, B3, B6 | UT-10 VersusTests | T014 |
| FR-11 | B2〜B4 | UT-11 RankTests | T015 → T022 |
| FR-12 | B5 | UT-12 CpuTests | T016 |
| FR-09 | B1, B7 | UT-09 MenuTests | T017 |
| FR-13 | D1〜D3 | UT-13 DifficultyTests, MenuTests | T020, T023 |
| FR-14 | B8 | UT-14 TrumpTests | T021 |
| FR-15 | E1〜E5 | UT-15 EffectLevelTests | T028, T029 |
| FR-16 | V1〜V4 | UT-16 TrumpFaceTests | T032, T033 |
| SC-001〜004 | M1〜M6 | AIプレイ | T010〜T011 |

## Phase 1: 準備

- [X] T001 接続確認（版の一致、シーン、コンパイル状態）
- [X] T002 シーン `Assets/MemoryMatch/Scenes/MemoryMatch.unity` に Card_00〜11、カーソル、成立・ミス・スコア表示を置き、保存して Hierarchy を再取得する（ロジックなし）

## Phase 2: ルール（Red → Green → Refactor、1ルールずつ）

- [X] T003 [US1] FR-01/07: 各絵柄2枚・全て裏・同じシードで同じ配置になる失敗テストと最小実装
- [X] T004 [US1] FR-02: カーソル移動と端、1枚めくる、3枚目はめくれない失敗テストと最小実装
- [X] T005 [US1] FR-03: 同じ絵柄は成立+100、違う絵柄はミス+1・-10・下限0、次のSpaceは戻すだけの失敗テストと最小実装
- [X] T006 [US1] FR-04: 同じカード・成立済みの再選択で状態が変わらない失敗テストと最小実装
- [X] T007 [US2] FR-05: 全成立でクリア、ミス5回でゲームオーバー、以後入力を受けない失敗テストと最小実装
- [X] T008 [US3] FR-06: 途中・終了後のRで初期化・再シャッフルする失敗テストと最小実装

## Phase 3: 接続と実証

- [X] T009 CardView / MemoryGame を実装しシーンへ接続、compile error 0、保存
- [X] T010 Play中に実入力でめくり、前後の状態を取得して Stop する
- [X] T011 M1〜M6 をAIプレイで実証し 画像に残す（verification.md への記録は T025 でまとめる）

## Phase 4: 追加（SPEC改訂 v1.1 のあと）

- [X] T012 コンボ（A1〜A4）と対戦（B1〜B7）を spec.md に追記する
- [X] T013 [US4] FR-08: 倍率・上限5・ミスとRで0に戻る失敗テストと最小実装
- [X] T014 [US5] FR-10: ペアの取得・続行・交代・人数分の巡回の失敗テストと最小実装
- [X] T015 [US5] FR-11: 勝敗・引き分け・ミス上限なしの失敗テストと最小実装
- [X] T016 [US5] FR-12: 直近4枚の記憶と、記憶だけで選ぶCPUの失敗テストと最小実装
- [X] T017 [US5] FR-09: モードと人数の選択の失敗テストと最小実装
- [X] T018 [US4][US5] 連続数・手番・取得数・持ち主・勝敗・モード選択・CPU進行をシーンへ接続し保存する

## Phase 5: 追加（SPEC改訂 v1.2 のあと）

- [X] T019 難易度（D1〜D3）、トランプ（B8）、スコア順位（B4）を spec.md に反映する
- [X] T020 [US6] FR-13: 3つの難易度の枚数・並び・ミス上限の失敗テストと最小実装
- [X] T021 [US5] FR-14: 52枚の配置と、同じ数字だけがペアになる失敗テストと最小実装
- [X] T022 [US5] FR-11: 各自の得点・コンボ・順位・同順位の失敗テストと最小実装（WinnerTests を RankTests に置き換え）
- [X] T023 [US6] FR-13: 難易度選択の失敗テストと最小実装
- [X] T024 可変の盤面・トランプ表示・得点・順位・難易度選択をシーンへ接続し保存する
- [X] T025 A1〜A4、B1〜B8、D1〜D3 をAIプレイで実証し、M1〜M6 の回帰を確認、verification.md に記録する
- [X] T026 終了処理（入力解除・Stop・保存）、コミット、push

## Phase 6: 演出（SPEC改訂 v1.3 のあと）

- [X] T027 演出とコンボ数の条件 E1〜E5 を spec.md に追記する
- [X] T028 [US7] FR-15: 段階ごとに強さが増え5で最大になる失敗テストと最小実装
- [X] T029 [US7] ComboEffects と、めくり・ミスの動き、コンボ数表示を実装しシーンへ接続する
- [X] T030 E1〜E5 をAIプレイで実証し、M1〜M6・D・B の回帰を確認して記録する

## Phase 7: 見た目（SPEC改訂 v1.4 のあと）

- [X] T031 V1〜V4 を spec.md に追記する
- [X] T032 [US8] FR-16: カード面の数字・スート・マーク配置の失敗テストと最小実装
- [X] T033 [US8] テーブル・カード・得点枠を実装しシーンへ接続する
- [X] T034 V1〜V4 をAIプレイと画像で実証し、全条件の回帰を確認して記録する

## Analyze 結果（実装前）

- 全FR（01〜07）に対応Taskとテストがある。対応FRのないTaskは準備・終了処理のみ
- spec と plan の用語・数値に矛盾なし。FR-08以降は T012 で spec に追記するまで実装しない
- 指摘（中）: constitution が未記入。plan の Constitution Check で代替（変更範囲外のため編集しない）
- v1.1: FR-08〜12 に T013〜T017 が対応する。M3・M5・M6 への影響は spec の US4・US5 に明記
- v1.2: FR-13・14 は T020・T021・T023、改訂した FR-11 は T022 が対応する
