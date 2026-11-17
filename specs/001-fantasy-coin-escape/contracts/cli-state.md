# Contract: unity-cli から読む状態

検証で `get_component_values` / `get_hierarchy` を使って読む名前の約束。実装で名前を変える場合は、この文書も更新する。

## GameObject

| パス | 内容 |
|------|------|
| `/Game` | `GameDirector` コンポーネント（下記の状態をまとめて公開） |
| `/Player` | プレイヤー本体（Transform、`PlayerHealth`、`PlayerCombat`） |
| `/Dungeon/Coins/Coin_0` 〜 `Coin_4` | 金貨。取得済みは非アクティブ |
| `/Dungeon/Enemies/Slime_n`, `Goblin_n` | ザコ敵。倒したものは非アクティブ |
| `/Dungeon/Boss` | ボス本体（`BossBrain`） |
| `/Dungeon/SavePoints/SavePoint_0` 〜 `_2` | 祈りの台座 |
| `/Dungeon/Potions/Potion_n` | 回復薬。使ったものは非アクティブ |
| `/Dungeon/BossDoor` | ボスの間の扉（開くと非アクティブ） |
| `/Dungeon/ExitGate` | 出口の柵（開くと非アクティブ） |
| `/Dungeon/ExitZone` | 出口の判定 |
| `/UI/Title`, `/UI/HUD`, `/UI/Pause`, `/UI/GameOver`, `/UI/Clear` | 各画面。表示中のものだけアクティブ |

## `GameDirector` の公開フィールド（読み取り専用として扱う）

| フィールド | 型 | 例 |
|------------|----|----|
| `flow` | string | `"Playing"`（data-model の GameFlow） |
| `coinCount` | int | 3 |
| `hearts` | int | 4 |
| `lives` | int | 2 |
| `respawnPointId` | int | 1（-1 は開始地点） |
| `bossDoorOpen` | bool | false |
| `bossHp` | int | 20 |
| `bossState` | string | `"ChargeTell"` |
| `bossDefeated` | bool | false |
| `exitOpen` | bool | false |
| `elapsed` | float | 83.42 |
| `splitsText` | string | `"Coin1 00:21.30\|Coin2 00:40.12"` |
| `bestTime` | float | -1（未記録） |
| `lastSounds` | string | `"Coin\|DoorOpen"`（直近に鳴らした効果音の名前、最大8件） |
| `bgm` | string | `"Explore"` / `"Boss"` |
| `savePromptVisible` | bool | 台座の範囲内で true |

## 検証専用の操作

- 状態を直接書き換える操作は用意しない（constitution II）。
- 例外: ベストタイムの初期化は、検証前の準備として `PlayerPrefs.DeleteKey("CryptEscape.BestRun")` を unity-cli の eval で実行してよい（プレイ結果ではなく、前提条件の準備のため）。
