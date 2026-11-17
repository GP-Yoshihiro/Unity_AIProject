# Asset Store 素材リスト

調査日 2026-11-17。すべて Asset Store のページで **無料** を確認済み。ライセンスはいずれも Standard Unity Asset Store EULA（Extension Asset）。

入手（「マイアセットに追加」）と取り込みは、ユーザーの Unity アカウントで行う（下の手順）。取り込み後、AI が tasks.md の Phase 9（US7）で差し替える。

## 候補

| 用途 | アセット | 作者 | サイズ | 描画設定 | メモ |
|------|----------|------|--------|----------|------|
| プレイヤー（剣士） | [Meshtint Free Knight](https://assetstore.unity.com/packages/3d/characters/humanoids/fantasy/meshtint-free-knight-133286) | Meshtint Studio | 7.0 MB | 記載なし（2017年対応の古い素材のため Built-in の標準シェーダーの可能性が高い） | アニメーションの内容は取り込み後に確認 |
| ザコ敵1（スライム） | [RPG Monster Couple PBR Polyart](https://assetstore.unity.com/packages/3d/characters/creatures/rpg-monster-couple-pbr-polyart-355988) | Dungeon Mason | 9.4 MB | Built-in / URP / HDRP 対応 | 収録モンスターは取り込み後に確認。スライムでなければ近い見た目のものを当てる |
| ザコ敵2（ゴブリン） | [Goblins - characters with animations](https://assetstore.unity.com/packages/3d/characters/goblins-characters-with-animations-129599) | Mixall | 87.6 MB | Built-in / URP 対応 | アニメーション付き |
| ボス | [Dragon for Boss Monster : PBR](https://assetstore.unity.com/packages/3d/characters/creatures/dragon-for-boss-monster-pbr-78923) | Dungeon Mason | 414.4 MB | 記載なし（2018年対応のため Built-in の可能性が高い） | 容量が大きい。不要なテクスチャは取り込み時に外してよい |
| 迷宮（床・壁・扉・小物） | [Low Poly Dungeons Lite](https://assetstore.unity.com/packages/3d/environments/dungeons/low-poly-dungeons-lite-177937) | JustCreate | 1.6 MB | 記載なし | モジュール式。台座・金貨・回復薬に使える小物があれば流用 |
| 効果音 | [RPG Essentials Sound Effects - FREE!](https://assetstore.unity.com/packages/audio/sound-fx/rpg-essentials-sound-effects-free-227708) | leohpaz | 16.1 MB | — | 剣・戦闘・回復・メニューなど |
| BGM | [The Fantasy Music Library FREE](https://assetstore.unity.com/packages/audio/music/the-fantasy-music-library-free-90287) | Ethan Richey | 39.9 MB | — | 曲の内容は取り込み後に確認し、探索用とボス戦用を選ぶ |

### 予備の候補

- 迷宮: [Lite Dungeon Pack - Low Poly 3D Art by Gridness](https://assetstore.unity.com/packages/3d/environments/dungeons/lite-dungeon-pack-low-poly-3d-art-by-gridness-242692)、[Ultimate Low Poly Dungeon](https://assetstore.unity.com/packages/3d/environments/dungeons/ultimate-low-poly-dungeon-143535)
- ゴブリン: [LOWPO: Low-Poly Goblin Pack Free Sample](https://assetstore.unity.com/packages/3d/characters/creatures/lowpo-low-poly-goblin-pack-free-sample-332200)
- BGM: [FREE Adventure RPG Fantasy Game Music Pack](https://assetstore.unity.com/packages/audio/music/orchestral/free-adventure-rpg-fantasy-game-music-pack-322978)

予備の候補は価格と描画設定を未確認。使う場合は先に確認する。

## 入手と取り込みの手順（ユーザー作業）

1. 上の表の各リンクを開き、Unity アカウントでサインインして **「マイアセットに追加」**（Add to My Assets）を押す
2. Unity Editor で **Window > Package Manager** を開き、左上のドロップダウンで **My Assets** を選ぶ
3. 各アセットを選んで **Download** → **Import**。取り込みダイアログはすべて選択のまま Import
   - 「プロジェクト設定を上書きする」旨の確認が出た場合は、上書きしない（No / Cancel）を選ぶ
4. 取り込みが終わったら AI に伝える。AI が `Assets/` の中身を確認し、表示できるか（ピンク色になっていないか）を確かめてから差し替える

## 取り込み後に AI が確認すること

- マテリアルが Built-in で表示できるか（ピンク色のマテリアルがないか）
- アニメーションの有無と種類（待機・移動・攻撃・被弾・倒れる）
- 取り込みで compile error や設定の変更が起きていないか
- どのモデル・音を何に使ったかを、この表の下に追記する
