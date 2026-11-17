# Research: 古城脱出 ― 五枚の金貨

Phase 0。技術上の未決事項と、その決定理由。

## R1. シーンとダイアログの管理（Additive）

- **Decision**（ユーザー指定）: シーンとダイアログは Additive 読み込みで管理する。
  - `Boot`（常駐、Build Settings の 0 番）: `GameDirector`（進行）、`GameInput`、`AudioManager`、`SceneDirector`（Additive の読み込み・破棄）
  - `Title`: タイトル画面（背景カメラ付き）
  - `Dungeon`: 迷宮・プレイヤー・敵・ボス・メインカメラ・ライト。読み込み後にアクティブシーンにする
  - `HUD`: プレイ中の HUD。Dungeon と一緒に読み込む
  - ダイアログ: `PauseDialog`、`GameOverDialog`、`ClearDialog`。開くときに Additive で読み込み、閉じるときに破棄する
- 画面遷移:
  - 開始: Title を破棄 → Dungeon + HUD を読み込み → 読み込み完了後に Playing（タイマー開始）
  - 即リスタート／ゲームオーバー後の続行: ダイアログを破棄 → Dungeon を破棄して読み直す（=全初期化）。HUD はそのまま
  - タイトルへ: ダイアログ・HUD・Dungeon を破棄 → Title を読み込み
  - 復活（残機あり）: シーンは読み直さず、Dungeon 内でプレイヤー・生きている敵・ボスを初期化（倒した敵・金貨は保持するため）
- シーンをまたぐ参照は Inspector で持てないため、各シーンのルートに置いたコンポーネント（`DungeonContext`、`HudView`、各ダイアログ）が読み込み時に `GameDirector` へ自分を登録する。
- **Rationale**: 画面ごとに独立したシーンになり、ダイアログの追加や差し替えがしやすい。リスタートで Dungeon を読み直すため、初期化漏れが起きない。
- **注意**: 即リスタートは1秒以内（SC-006）。Dungeon の読み直し時間を検証で測る。

## R2. 迷宮の作り方

- **Decision**: 文字で書いたマップ定義（例: `#` 壁、`.` 床、`C` 金貨、`S` スライム）から、Editor 拡張 `DungeonBuilder` で床・壁・配置物を生成する。生成物はシーンに保存する。
- **Rationale**: 部屋の形や金貨の位置を何度も調整できる。床・壁・敵は Prefab から生成するので、Asset Store の素材に差し替えるときは Prefab だけ替えればよい（FR-060）。
- **Alternatives considered**: 手作業で配置 → 調整のたびに大量の操作が必要。実行時の自動生成 → スコープ外（spec の Out of Scope）。

## R3. 敵の移動

- **Decision**: NavMesh は使わない。敵は自分の「縄張り」（初期位置からの半径）の中で、Rigidbody の速度で直接プレイヤーへ向かう。縄張りの外には出ず、プレイヤーを見失うと初期位置へ戻る。ゴブリンの巡回は経路点（Waypoint）を順に回る。
- **Rationale**: 追加パッケージ（AI Navigation）が要らず、部屋単位で敵を置く迷宮では十分。動きが単純なので検証で予測しやすい。
- **Alternatives considered**: AI Navigation パッケージの NavMeshAgent → 通路をまたいで賢く追えるが、パッケージ追加とベイクが必要で、今回の「部屋ごとの戦闘」には過剰。

## R4. 入力

- **Decision**: Input System の Input Actions アセット（`CryptEscape.inputactions`）に、Gameplay（Move/Attack/Dodge/Save/Pause/Restart）と UI（Navigate/Submit/Cancel）のアクションマップを作る。キーボードとゲームパッドの両方をバインドする（spec FR-050 の表）。コードからは生成した C# ラッパーで読む。
- **Rationale**: 1か所の定義で両デバイスに対応できる。
- **注意（BallCollect で判明）**: unity-cli の `input_keyboard` で押したキーは、`wasPressedThisFrame` が立たないことがあった。ボタン系は「前フレームの押下状態との比較」で立ち上がりを判定する共通処理（`ButtonEdge`）を使う。

## R5. 時間の扱い（タイマー・ポーズ）

- **Decision**: ポーズは `Time.timeScale = 0`。RTA タイマーは `Time.unscaledDeltaTime` を、状態が Playing / Dying（倒れて復活するまで）のときだけ加算する。表示は 分:秒.百分の一秒。
- **Rationale**: ポーズ中は止め、倒れてから復活までは含める（FR-031）を、状態で明確に分けられる。
- **Alternatives considered**: `Time.time` の差分 → ポーズ中も進んでしまう。

## R6. ベストタイムの保存

- **Decision**: `PlayerPrefs` にベストタイムと7区間のタイムを JSON 文字列で保存する。キーは `CryptEscape.BestRun`。
- **Rationale**: 1件の記録だけで、追加の仕組みが要らない。
- **Alternatives considered**: `Application.persistentDataPath` へのファイル保存 → 1件なら PlayerPrefs で十分。

## R7. UI と文字

- **Decision**: uGUI の Text（Legacy）を使う。画面の文言は英語にする（COIN 3/5、GAME OVER、PRESS E TO SAVE など）。日本語にする場合は、日本語フォントを Asset Store 素材と一緒に入れてから差し替える。
- **Rationale**: 標準フォント（LegacyRuntime）には日本語の文字がない。TextMeshPro は Essentials の取り込みが必要。BallCollect で Legacy Text の動作を確認済み。
- **注意（BallCollect で判明）**: 文字サイズに対して枠が小さいと表示されない。縦方向は Overflow にする。

## R8. 見た目と素材の差し替え

- **Decision**: キャラクター・敵・ボス・金貨・台座・扉・回復薬は「本体（当たり判定とスクリプト）」の子に「Visual」を置き、見た目はその子だけにする。仮素材は標準図形と色（プレイヤー=青、スライム=緑、ゴブリン=茶、ボス=紫で大きめ、金貨=金、台座=水色、回復薬=赤）。
- **Rationale**: 素材を差し替えても当たり判定と動きが変わらない（SC-007）。

### Asset Store で入手する素材（ユーザー作業）

カテゴリごとに必要なものを挙げる。具体的な候補は、ストアで「無料」「Built-in RP 対応」「ライセンス」を確認してから決める。

| カテゴリ | 用途 | 条件 |
|----------|------|------|
| ファンタジーの剣士キャラクター | プレイヤー | 待機・移動・攻撃・被弾・倒れるのアニメーション付きが望ましい |
| モンスター（スライム系） | ザコ敵1 | 移動・攻撃・被弾・倒れるのアニメーション |
| モンスター（ゴブリン・人型） | ザコ敵2 | 同上 |
| 大型モンスター | ボス | 突進と範囲攻撃に見える動きがあると良い |
| ダンジョンの床・壁・扉・柱 | 迷宮 | モジュール式（グリッドに並べられる） |
| 小物 | 金貨・台座・回復薬・松明 | |
| 効果音 | FR-043 の各場面 | |
| BGM | 探索・ボス戦 | ループ再生できるもの |
| 日本語フォント（任意） | UI を日本語にする場合 | 再配布可のライセンス |

- 共通の条件: 無料、商用利用の可否とクレジット表記の要否を確認、Built-in レンダーパイプラインで表示できること（URP 専用ならマテリアルの変換が必要になる）。
- 素材が入るまでは仮素材のまま完成させる（spec User Story 7）。

## R9. 音

- **Decision**: `AudioManager` に効果音の名前（Attack, Hit, Hurt, Coin, Heal, Save, DoorOpen, BossTell, BossDefeat, Clear, GameOver）と BGM（Explore, Boss）を登録し、名前で鳴らす。素材がない間は音なし（無音でもエラーにしない）。鳴らした履歴を最後の数件だけ保持し、unity-cli から確認できるようにする。
- **Rationale**: 音は画像で確認できないため、鳴らした記録を状態として読めるようにする（constitution V）。

## R10. テスト

- **Decision**: 進行ルールを純粋な C# クラス（`RunState`、`LivesAndRespawn`、`RtaTimer`、`BossBrain` の状態遷移）にして EditMode テストで確かめる。実行は `run_tests` に `filter: "CryptEscape.Tests"` を付けて自分のテストだけ実行する。
- **Rationale**: BallCollect で、フィルタなしで実行すると Bridge パッケージのテストがシーンにオブジェクトを残したため。

## R11. 検証時の Editor 設定

- **Decision**: `PlayerSettings.runInBackground = true`。Play 開始後は Game View にフォーカスし、タイマーが進んでいることを確かめてから入力を送る。
- **Rationale**: BallCollect で、Editor が背面にあるとフレームが進まず、入力の解除やタイマーが止まったため。
