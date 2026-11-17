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
