# Unity_AIProject Constitution

AI エージェントが unity-cli 経由で Unity Editor を操作してゲームを作る、授業「ゲームAI開発」のプロジェクト。

## Core Principles

### I. 実物で確認する（Evidence over Claims）
完了の判断は、Editor の実際の状態とゲーム画面で行う。「C# を書いた」「コマンドが成功した」は完了ではない。状態（座標・数値・フラグ）を unity-cli で再取得し、画面は UI 込みの画像を撮って開いて確認する。確認済み・未成立・未確認を区別して記録し、未確認を隠さない。

### II. 操作して確かめる（Play, Don't Poke）
動作確認は Play 中に実際の入力（unity-cli の入力シミュレーション）を送って行う。座標や数値を直接書き換えて「動いた」としない。入力を送ったら、フレームが進むのを待ってから再取得する。検証の最後には入力を解除し、Play を止める。

### III. ロジックは Unity から分けてテストする（Testable Rules）
進行ルール（回収・クリア条件・残機・タイマーなど）は MonoBehaviour から分けた純粋な C# クラスに置き、EditMode テストで確かめる。テストは名前・件数・合否を照合し、0件・全スキップ・未完了は合格と扱わない。自分のテストだけを絞り込んで実行する。

### IV. 小さく段階的に（Incremental Slices）
1段階ずつ実装し、実物を確認してから次に進む。各段階はそれ単体で遊べる・確かめられる単位にする。仕様にない機能を「おまけ」として足さない。

### V. 状態を読めるようにする（Observable State）
CLI で確認したい状態は、名前の分かる公開フィールドや GameObject として取得できるようにする。見た目（モデル・音）とロジック（当たり判定・状態）は分け、素材を差し替えても挙動が変わらないようにする。

## 技術上の制約

- Unity 6000.6.4f1、Built-in レンダーパイプライン、C#
- 入力は Input System に統一する（旧 Input Manager の API を使わない）
- Editor 操作は unity-cli（CLI と Bridge の版が一致していること）
- 外部素材は、ユーザーが入手・許諾を確認したものだけを使う
- API キー・認証情報・個人設定をリポジトリに含めない

## 開発の流れ

- ブランチは gitルールのプレフィックス（feature/, fix/, docs/ など）で、作業内容が分かる名前にする
- Spec Kit の流れ（spec → plan → tasks → implement）で進め、仕様・計画・検証記録を `specs/` に残す
- 検証結果は条件ごとに「操作・期待・実際・根拠・判定」で記録する

## Governance

この constitution は計画と実装の判断より優先する。原則から外れる場合は、plan.md の Complexity Tracking に理由を書く。改訂時はバージョンと日付を更新する。

**Version**: 1.0.0 | **Ratified**: 2026-11-17 | **Last Amended**: 2026-11-17
