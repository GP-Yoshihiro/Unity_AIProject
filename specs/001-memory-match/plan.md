# Implementation Plan: 神経衰弱（Memory Match）

**Branch**: `20261124-memory-match-sdd` | **Date**: 2026-11-24 | **Spec**: [spec.md](spec.md)

## Summary

ルールを純粋なC#に置いてEditModeでTDDし、Unity層は表示・カーソル・入力だけを担う。unity-cliで組み立て、実入力で実証する。

## Technical Context

**Language/Version**: C# / Unity 6000.6.4f1（Built-in）

**Primary Dependencies**: 既存のもののみ（Input System、uGUI、unity-cli bridge 0.19.1）。パッケージ追加なし

**Storage**: N/A

**Testing**: Unity Test Framework（EditMode）＋ unity-cli によるAIプレイ

**Target Platform**: Unity Editor の Game View

**Project Type**: 単一シーンのゲーム

**Constraints**: 外部アセットなし。対象ファイルとシーンのみ変更

**Scale/Scope**: 1シーン、カード12枚

## Constitution Check

`main` の constitution は未記入のテンプレートのため、スライド6の作業条件を代わりの基準とする。

- テストファースト（Redを見てからGreen）／1回1Task／SPECにないルールを足さない
- 変更は対象ファイルとシーンのみ。パッケージ・外部アセットを足さない
- 状態を書き換えて動いたことにしない。裏の絵柄を覗かない。失敗を成功と報告しない
- 区切りで保存し、入力とPlayを残さない

## Project Structure

```text
Assets/MemoryMatch/
├── Scenes/MemoryMatch.unity
├── Scripts/Core/   MemoryMatch.Core.asmdef（noEngineReferences）, MemoryLogic.cs, DeckShuffler.cs
├── Scripts/Unity/  MemoryMatch.asmdef, MemoryGame.cs（入力・表示）, CardView.cs
└── Tests/EditMode/ MemoryMatch.Tests.EditMode.asmdef, *Tests.cs
specs/001-memory-match/  interview, spec, plan, tasks, verification, evidence/
```

**Structure Decision**: Core は asmdef で Unity 参照を禁じ、分離をコンパイル時に保証する。

## 設計

- **MemoryLogic**: 配置、向き、カーソル、判定、スコア、状態遷移（PickFirst → PickSecond → ShowingMiss／PickFirst、Cleared、GameOver）
- **DeckShuffler**: 乱数は実行中1本。シード固定なら開始とRの配置列が再現される
- **MemoryGame**: GameObject `Game` に付く。入力を Logic に渡し、結果を表示する
- **CLIで読む名前**（`Game` / `MemoryGame` の公開プロパティ）: `Phase` `Matched` `Misses` `Score` `Cursor` `Cards`
- `Cards` は裏のカードを `--` とし、絵柄を出さない。CardView も裏の間は絵柄を持たない（覗き防止）
- **対戦（v1.1）**: MemoryLogic を人数つきにする（1なら1人用のルール、2以上なら対戦）。`CurrentPlayer` `OwnerAt` `PairsOf`
- **CpuMemory / CpuPlayer**: 公開されたカードの観測だけを受け取る。MemoryLogic の配置は参照せず、`VisibleSymbolAt`（裏なら-1）から得る
- **BoardSpec（v1.2）**: 盤面の種類（Easy / Normal / Hard / Trump）。列・行・ミス上限・カード一覧・ペアの判定キーを持ち、MemoryLogic と DeckShuffler に渡す。CPUが観測するのも判定キー
- 順位は `RankOf`、スコアは `ScoreOf` / `ComboOf`（プレイヤー別）。旧 `Winners` は置き換え
- カードは Card_00〜Card_51 をシーンに置き、使う枚数だけ表示して並べる
- **MenuState**: モードと人数の選択（純粋なC#）
- **CLIで読む名前（追加）**: `Combo` `Mode` `PlayerCount` `CurrentPlayer` `Pairs` `Scores` `Owners` `Result` `Columns`
- 表示: uGUI。`Card_00`から行優先。記号と色で区別
- 裏で Editor が止まらないよう `Application.runInBackground` を実行時に設定（ProjectSettings は変えない）
