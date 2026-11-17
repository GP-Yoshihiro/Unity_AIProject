# Data Model: 古城脱出 ― 五枚の金貨

Phase 1。ゲーム内の状態と、その変化のルール。数値の初期値は spec の Assumptions に従い、`GameConfig`（ScriptableObject）で調整できるようにする。

## GameFlow（画面の状態）

| 状態 | 説明 | タイマー |
|------|------|----------|
| Title | タイトル画面 | 停止（00:00.00） |
| Playing | 操作中 | 進む |
| Paused | ポーズ中（timeScale=0） | 停止 |
| Dying | 倒れてから復活/ゲームオーバーまでの演出（約1.5秒） | 進む |
| GameOver | ゲームオーバー画面 | 停止 |
| Cleared | クリア画面 | 停止（確定） |

```
Title --Start--> Playing
Playing --Pause--> Paused --Resume--> Playing
Paused --Restart--> Playing（全初期化） / Paused --ToTitle--> Title
Playing --hearts=0--> Dying --lives>0--> Playing（復活） / Dying --lives=0--> GameOver
GameOver --Restart/Submit--> Playing（全初期化）
Playing --出口に触れる(ボス撃破済み)--> Cleared
Playing/Dying/GameOver/Paused --Restart key--> Playing（全初期化）
Cleared --Submit--> Title / Cleared --Restart--> Playing（全初期化）
```

## RunState（1回のプレイの進行）

| フィールド | 型 | 初期値 | ルール |
|------------|----|--------|--------|
| collectedCoinIds | set<int> | 空 | 同じ ID は二重に加えない。CoinCount = 要素数（0〜5） |
| lives | int | 3 | 倒れると −1。0 でゲームオーバー |
| respawnPointId | int | -1（開始地点） | 保存で更新 |
| defeatedEnemyIds | set<int> | 空 | 倒したザコ敵。復活時も保持 |
| usedPotionIds | set<int> | 空 | 使った回復薬。復活時も保持 |
| bossDoorOpen | bool | false | CoinCount が 5 になった時点で true。以後 false に戻らない（全初期化を除く） |
| bossDefeated | bool | false | ボスの体力が 0 で true |
| exitOpen | bool | false | bossDefeated と同時に true |

- **全初期化**（ゲームオーバー後・即リスタート）: すべて初期値へ。RtaTimer も初期化。
- **復活**（lives > 0 で倒れた）: lives 以外の RunState はそのまま。プレイヤーのハートは5、位置は復活地点。生きているザコ敵とボスは初期位置・満タン。

## Player

| フィールド | 型 | 初期値 | ルール |
|------------|----|--------|--------|
| hearts | int | 5 | 0〜5。被弾で減り、回復薬で増える（上限5） |
| invincibleTime | float | 0 | 被弾後 1.0 秒、回避中 0.3 秒は無敵。無敵中の被弾は無視 |
| attackCooldown | float | 0 | 攻撃後 0.4 秒は攻撃できない |
| dodgeCooldown | float | 0 | 回避後 0.8 秒は回避できない |
| facing | Vector3 | +Z | 最後に動いた方向。攻撃と回避の向き |

- 剣の攻撃範囲: 前方 1.6m、角度 ±60° の扇形。範囲内の敵すべてに攻撃力1。
- 同じフレームで複数の敵から攻撃された場合、1回分だけ受ける。

## Coin

| フィールド | 型 | ルール |
|------------|----|--------|
| coinId | int (0〜4) | 固有 |
| collected | bool | RunState.collectedCoinIds に含まれるか |

取得時: RunState に加え、RtaTimer に区間 `Coin{n}`（n = 取得後の枚数）を記録。5枚目で bossDoorOpen = true。

## Enemy（ザコ敵）

| フィールド | 型 | スライム | ゴブリン |
|------------|----|---------|---------|
| enemyId | int | 固有 | 固有 |
| maxHp / hp | int | 2 | 3 |
| moveSpeed | float | 2.5 | 3.0 |
| detectRange | float | 5 | 6 |
| leashRange（縄張り） | float | 8 | 10 |
| contactDamage / attackDamage | int | 1（体当たり） | 1（武器、予備動作0.4秒） |

状態遷移:

```
Idle/Patrol --プレイヤーが detectRange 内--> Chase
Chase --攻撃距離内--> Attack --終了--> Chase
Chase --プレイヤーが leashRange 外 or 縄張りの外へ出そう--> Return --初期位置--> Idle/Patrol
任意 --被弾--> Stagger(0.3秒) --> Chase
任意 --hp=0--> Dead（RunState.defeatedEnemyIds に追加。消える）
```

## Boss

| フィールド | 型 | 初期値 |
|------------|----|--------|
| maxHp / hp | int | 20 |
| state | enum | Dormant |
| arenaEntered | bool | false |

状態遷移（予兆→攻撃→隙を交互に繰り返す）:

```
Dormant --プレイヤーがボスの間に入る--> Idle(1.0秒)
Idle --> ChargeTell(0.8秒: 向きを示す線) --> Charge(直進、壁か最大距離で停止) --> Recover(1.5秒: 隙)
Recover --> AoeTell(1.0秒: 地面に半径3.5mの円) --> Aoe(瞬間判定) --> Recover(1.2秒: 隙)
Recover --> ChargeTell ...（交互）
任意 --hp=0--> Defeated（bossDefeated, exitOpen = true。区間 BossDefeat を記録）
```

- 突進のダメージ: ハート2。範囲攻撃: ハート1。
- 予兆から攻撃までは 0.8 秒以上（SC-004 の 0.6 秒以上を満たす）。
- プレイヤーが倒れて復活: hp=20、state=Dormant、初期位置。

## SavePoint（祈りの台座）

| フィールド | 型 | ルール |
|------------|----|--------|
| savePointId | int (0〜2) | 固有 |
| interactRange | float | 1.8m。範囲内で "PRESS E TO SAVE" を表示 |

保存: RunState.respawnPointId = savePointId。体力は変えない。

## Potion（回復薬）

| フィールド | 型 | ルール |
|------------|----|--------|
| potionId | int | 固有 |
| healAmount | int | 2 |

ハートが5のときは取らない。取ると usedPotionIds に追加して消える。

## RtaTimer

| フィールド | 型 | ルール |
|------------|----|--------|
| elapsed | float（秒） | Playing / Dying の間だけ unscaled time で加算 |
| splits | list<(name, time)> | Coin1〜Coin5, BossDefeat, Escape の順。同じ区間は1回だけ |
| finished | bool | Escape 記録で true。以後加算しない |

## BestRecord（PC に保存）

| フィールド | 型 | ルール |
|------------|----|--------|
| bestTime | float | 未記録は無し |
| bestSplits | list<(name, time)> | ベスト更新時の区間 |

クリア時: elapsed < bestTime（または未記録）なら更新して保存。CLEAR 画面に bestTime との差（−なら更新）を表示。
