# BallCollect 検証記録

- 環境: Unity 6000.6.4f1 / unity-cli 0.19.1 / Bridge 0.19.1（versionCheck: match）
- 方法: AI（Claude Code）が unity-cli で Play → `input_keyboard` で入力 → 待機 → `get_component_values` / `get_hierarchy` で状態を再取得。画面は `capture_screenshot`（captureMode=game, includeUI=true、Game View にフォーカス）で撮影し、画像を開いて目視確認した。
- 状態は直接書き換えていない。すべて実際のキー入力による結果。
- 判定: 確認済み（期待どおり）／未成立（検証して期待と異なる）／未確認（まだ検証していない）

## 基本仕様（第2コマ前半）

| 条件 | 操作 | 期待 | 実際 | 根拠 | 判定 |
|------|------|------|------|------|------|
| B1 | Play直後 | 回収数0、アイテム5個、CLEAR非表示 | count=0、items=11111、clearUI=false、球 (0, 0.5, −7) | 状態の再取得 / `02_play_initial.png` | 確認済み |
| B2 | D 0.5秒 → A 0.5秒 → S 0.3秒 → W 0.3秒 | D:+X、A:−X、S:−Z、W:+Z | x 0.00→2.54→0.10、z −7.00→−8.56→−7.10 | 各入力前後のTransform | 確認済み |
| B3 | W 0.8秒（Item_1 を通過） | Item_1 が消え 1/5 | count=1、items=01111、UI「1/5」 | 状態 / `03_collect_1of5.png` | 確認済み |
| B3 二重加算 | S 0.8秒で Item_1 の位置を戻り、W 0.8秒で再通過 | 回収数1のまま | count=1 のまま | 状態の再取得 | 確認済み |
| B4 途中 | 1個回収時 | CLEARを出さない | clearUI=false、画像にもCLEARなし | 状態 / `03_collect_1of5.png` | 確認済み |
| B4 | W 3.3秒で5個すべて回収 | 5/5 で CLEAR 表示 | count=5、cleared=true、items=00000、画像に「5/5」「CLEAR」 | 状態 / `04_clear.png` | 確認済み（修正後） |
| B5 | 1個回収後、D 2.5秒で床の右端から落下 | 位置・速度・回収数・アイテム・UIが初期化 | x=12.26, y=−0.69 まで落下を観測した後、count=0、items=11111、pos (0, 0.5, −7)、vel (0, 0, 0)、clearUI=false | 落下中と落下後のTransform・状態 | 確認済み |
| B6 途中 | 1個回収後に R | 初期状態へ戻る | count 1→0、items=11111、pos (0, 0.5, −7) | 状態の再取得 | 確認済み（修正後） |
| B6 クリア後 | CLEAR 後に R | 初期状態へ戻る | count=0、cleared=false、items=11111、clearUI=false | 状態 / `05_after_clear_reset.png` | 確認済み |

### 検証中に見つけて直した不具合

1. **B6: R が効かない（未成立 → 修正）**
   `input_keyboard` で R を押すと押下状態（pressedKeys=[r]）は届いていたが、`Keyboard.rKey.wasPressedThisFrame` が立たず、初期化されなかった。前フレームの押下状態と比べて立ち上がりを検出する方式に変更し、途中R・クリア後Rの両方で再確認した。
2. **B4: CLEAR が画面に出ない（未成立 → 修正）**
   状態は `cleared=true`、ClearText も active だったが、UI込みの画像に文字が写っていなかった。原因は文字サイズ 180 に対して RectTransform の高さが 160 で、Text の縦方向が Truncate になっていたこと。縦方向を Overflow にして枠を広げ、画像で表示を再確認した。

### 補足

- 落下のサンプリングの時刻表記は、CLI 呼び出しの時間を含まないため実時間より短い。落下が起きたことと、その後に初期化されたことの証拠として扱う。

## 追加ルール: 30秒の時間制限（第2コマ後半）

制限時間は短縮せず、実際の30秒で確認した。Play開始直後に Game View へフォーカスし、残り時間が減っている（フレームが進んでいる）ことを確かめてから操作した。

| 条件 | 操作 | 期待 | 実際 | 根拠 | 判定 |
|------|------|------|------|------|------|
| A1 | Play後、入力なしで5秒待つ | 残り時間が表示され、減る | time 26.32 → 20.35、画面右上「TIME 18.8」 | 状態 / `06_timer_counting.png` | 確認済み |
| A2 | 入力なしで時間切れまで待ち、その後 W 1秒 | TIME UP 表示、移動しない | time=0.00、timeUp=true、画面に「TIME UP」。W 1秒後も pos (0, 0.5, −7) のまま | 状態 / `07_time_up.png` | 確認済み |
| A2 + R | TIME UP 中に R | 時間切れでもRが効く | timeUp=false、time 29.22、count=0、items=11111 | 状態の再取得 | 確認済み |
| A3 | W 1秒×3回で5個すべて回収（残り約19秒） | CLEAR、時計が止まる | cleared=true、time 19.26 のまま3秒後も変化なし、画面に「CLEAR」「TIME 19.3」 | 状態 / `08_clear_in_time.png` | 確認済み |
| A4 | CLEAR後、さらに31秒待つ（開始から30秒を超える） | TIME UP に変わらない | cleared=true、timeUp=false、time 19.26 のまま | 状態 / `09_clear_no_timeup.png` | 確認済み |
| A5 R | 1個回収して約5秒後に R | 残り時間も30秒に戻る | time 24.89 → 29.22（Rの処理後に少し進んだ値）、count 1→0 | 状態の再取得 | 確認済み |
| A5 落下 | 1個回収後に D で落下 | 残り時間も30秒に戻る | count 1→0、time 24.89 → 28.45、pos (0, 0.5, −7) | 状態の再取得 | 確認済み |
| 優先順位 | 5個目の回収と時間切れが同じ更新 | 回収（CLEAR）を優先 | EditMode テストで確認（下記）。実プレイでのフレーム単位の再現はしていない | テスト結果 | 確認済み（テストのみ） |
| CLEAR後R | CLEAR後に R | 初期状態へ戻る | count=0、cleared=false、time 29.22 | 状態の再取得 | 確認済み |

### 基本仕様の再確認（30秒ルール追加後）

| 条件 | 実際 | 判定 |
|------|------|------|
| B2 | D:+X（0→2.44）、A:−X（→0.00）、S:−Z（−7.00→−8.46）、W:+Z | 確認済み |
| B3 | 1個回収で count=1、S 1秒で戻り W 1秒で再通過しても count=1 | 確認済み |
| B4 | 1〜4個では CLEAR なし、5個で CLEAR（A3 と同じ実行） | 確認済み |
| B5 | 1個回収後の落下で count・items・pos・vel・time がすべて初期化 | 確認済み |
| B6 | 途中・CLEAR後・TIME UP後のいずれも R で初期化 | 確認済み |

## EditMode テスト

`unity-cli raw run_tests`（testMode=EditMode、filter=`BallCollect.Tests`）で実行。**5件中5件合格、失敗0、スキップ0、未確定0**。

| テスト | 内容 | 結果 |
|--------|------|------|
| `SameItemTwice_CountsOnce` | 同じアイテムを2回回収しても回収数は1 | Passed |
| `CollectAllFive_Clears` | 4個ではクリアせず、5個でクリア | Passed |
| `Reset_ReturnsToZeroAndNotCleared` | 初期化で回収数0・未クリア・TIME UP解除・残り30秒 | Passed |
| `TimeRunsOut_TimeUpAndNoMoreCollect` | 30秒経過でTIME UP、その後は回収しない | Passed |
| `FifthCollectAndTimeOutInSameUpdate_ClearWins` | 同じ更新で5個目の回収と時間切れが重なるとCLEARが優先され、時計が止まる | Passed |

## 検証の進め方で見つかった問題（ゲームの不具合ではない）

1. **テストがシーンにオブジェクトを残した**: フィルタなしで EditMode テストを実行すると、Bridge パッケージ自身のテストも走り、開いているシーンに `TestObject` が2つ残った（保存はされていない）。これが球の進路をふさぎ、z=−1.00 で止まった。保存済みシーンを開き直して除去し、以降は `filter=BallCollect.Tests` で自分のテストだけを実行した。これより前のタイマー検証結果は破棄して、すべてやり直した。
2. **Editor が背面にあるとフレームが進まない**: `PlayerSettings.runInBackground` が false だったため、キーの解除予約や残り時間が止まることがあった。true に変更した（`ProjectSettings.asset`）。あわせて、Play 開始時に Game View へフォーカスし、残り時間が減っていることを確かめてから操作する手順にした。
3. **フィルタの指定方法**: `namespace` を指定すると0件実行になった（0件は合格ではないため `filter` で再実行した）。

## 未確認

- 実機キーボードでの操作（今回の入力はすべて unity-cli の仮想キーボード）
- 5個目の回収と時間切れが同じフレームで重なるケースの実プレイでの再現（ロジックはテストで確認済み）
