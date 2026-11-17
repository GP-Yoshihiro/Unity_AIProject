# Contract: 入力アクション

Input Actions アセット `Assets/Game/Input/CryptEscape.inputactions`。

## Gameplay マップ

| アクション | 種類 | キーボード | ゲームパッド |
|------------|------|-----------|-------------|
| Move | Value (Vector2) | WASD / 矢印キー | 左スティック / 十字キー |
| Attack | Button | J / Space | buttonSouth |
| Dodge | Button | K / Left Shift | buttonEast |
| Save | Button | E | buttonNorth |
| Pause | Button | Escape | start |
| Restart | Button | R | select |

## UI マップ

| アクション | 種類 | キーボード | ゲームパッド |
|------------|------|-----------|-------------|
| Navigate | Value (Vector2) | WASD / 矢印キー | 左スティック / 十字キー |
| Submit | Button | Enter / Space | buttonSouth |
| Cancel | Button | Escape | buttonEast |

## 判定のルール

- ボタンは「前フレームは離れていて、今フレームは押されている」で1回だけ反応する（research R4）。
- Restart はどの画面でも有効（タイトルを除く）。
- Move はゲームパッドのスティックに 0.2 のデッドゾーン。
