# M1〜M6 の実証（シード固定の1人用）。裏の絵柄は読まず、めくって見えたものだけを記憶する。
param([string]$Tag = '')
. "$PSScriptRoot/mm.ps1"
$Menu = $true; $Combo = $true
$script:seen = @{}; $script:ng = 0
function Chk($name, $cond, $s) { if (-not $cond) { $script:ng++ }; "{0} {1} | {2}" -f $(if ($cond) {'OK '} else {'NG '}), $name, (Show $s) }
function Learn($s) { $a = $s.Cards -split ' '; for ($i=0;$i -lt 12;$i++) { if ($a[$i] -ne '--') { $script:seen[$i] = $a[$i].Trim('*').Substring(0,1) } } }
function Init($s) { $s.Phase -eq 'PickFirst' -and $s.M -eq 0 -and $s.X -eq 0 -and $s.Score -eq 0 -and $s.Cur -eq 0 -and $s.Cards -eq ((@('--')*12) -join ' ') }
function Same($a,$b) { (Show $a) -eq (Show $b) }
function Begin { StartPlay; if ($Menu) { Key 'space'; Start-Sleep -Milliseconds 300 } }
"-- run1: 初期状態・めくる・ミス・ゲームオーバー・再開"
Begin
$s = S; Chk 'M1 初期状態' (Init $s) $s; Shot "M1_initial$Tag"
Key 'leftArrow'; Key 'upArrow'; $s = S; Chk 'M2 端で止まる' ($s.Cur -eq 0) $s
Flip 0; $s = S; Learn $s; Chk 'M2 1枚だけ表' ($s.Phase -eq 'PickSecond' -and ($s.Cards -split ' ' | ? {$_ -ne '--'}).Count -eq 1) $s; Shot "M2_flip$Tag"
$b = S; Key 'space'; $s = S; Chk 'M4 同じカードは2枚目にできない' (Same $b $s) $s
Flip 1; $s = S; Learn $s; Chk 'M3 ミス: 即+1・下限0・2枚見える' ($s.Phase -eq 'ShowingMiss' -and $s.X -eq 1 -and $s.Score -eq 0) $s; Shot "M3_miss$Tag"
Flip 2; $s = S; Chk 'M2/M3 3枚目はめくれず戻すだけ' ($s.Phase -eq 'PickFirst' -and $s.Cards -eq ((@('--')*12) -join ' ')) $s
foreach ($p in @(2,3),@(4,5),@(6,7),@(8,9)) { if ((S).Phase -eq 'ShowingMiss') { Key 'space' }; Flip $p[0]; Learn (S); Flip $p[1]; Learn (S) }
$n = 0; while ((S).X -lt 5 -and $n++ -lt 5) { if ((S).Phase -eq 'ShowingMiss') { Key 'space' }
  $down = 0..11 | ? { (($s = S).Cards -split ' ')[$_] -eq '--' }; $a = $down[0]; $c = $down | ? { $script:seen[$_] -and $script:seen[$a] -and $script:seen[$_] -ne $script:seen[$a] } | select -First 1
  Flip $a; Learn (S); Flip $c; Learn (S) }
$b = S; Chk 'M5 ミス5回でゲームオーバー・2枚は表のまま' ($b.Phase -eq 'GameOver' -and $b.X -eq 5) $b; Shot "M5_gameover$Tag"
Key 'space'; Key 'upArrow'; Key 'leftArrow'; $s = S; Chk 'M5 終了後は入力を受けない' (Same $b $s) $s
Key 'r'; $s = S; Chk 'M6 ゲームオーバー後のR' (Init $s) $s; Shot "M6_restart$Tag"
Flip 3; $s = S; Chk 'M6 再び遊べる' ($s.Phase -eq 'PickSecond') $s; Key 'r'; $s = S; Chk 'M6 途中のR' (Init $s) $s
StopPlay
"-- run2: 同じシードの配置（run1 で見た記憶）で成立・クリア"
$old = @{} + $script:seen
Begin
Flip 0; $s = S; Chk 'FR-07 同じシードで同じ絵柄' ((($s.Cards -split ' ')[0]) -eq $old[0]) $s
$mate = 1..11 | ? { $old[$_] -eq $old[0] } | select -First 1
if ($null -eq $mate) { $mate = 10..11 | select -First 1 }
Flip $mate; Learn (S); $s = S
if ($s.M -eq 0) { Key 'space'; "   (未知のカードを探索: ミス $($s.X))"; Flip 0; Flip 11; Learn (S); $s = S }
$m0 = $s.X; Chk 'M3 成立: 表のまま +100' ($s.M -eq 1 -and $s.Score -eq 100) $s; Shot "M3_match$Tag"
GoTo 0; $b = S; Key 'space'; $s = S; Chk 'M4 成立済みは選べない' (Same $b $s) $s
$down = 1..11 | ? { (($s.Cards -split ' ')[$_]) -eq '--' -and $script:seen[$_] }; $a = $down[0]; $c = $down | ? { $script:seen[$_] -ne $script:seen[$a] } | select -First 1
Flip $a; $b = S; GoTo 0; Key 'space'; $s = S; Chk 'M4 成立済みは2枚目にできない' ($s.Phase -eq 'PickSecond' -and $s.X -eq $m0 -and $s.Cards -eq $b.Cards) $s
Flip $c; $s = S; Chk 'M3 ミス: -10' ($s.Score -eq 90 -and $s.X -eq ($m0+1)) $s; Key 'space'
foreach ($i in 0..11) { # 未知のカードを残りから確定
  $s = S; if ((($s.Cards -split ' ')[$i]) -ne '--') { continue }
  if ($s.Phase -eq 'ShowingMiss') { Key 'space' }
  Flip $i; Learn (S); $sy = $script:seen[$i]
  $cards = (S).Cards -split ' '; $mate = 0..11 | ? { $_ -ne $i -and $cards[$_] -eq '--' -and $script:seen[$_] -eq $sy } | select -First 1
  if ($null -eq $mate) { $mate = 0..11 | ? { $_ -ne $i -and $cards[$_] -eq '--' -and -not $script:seen[$_] } | select -First 1 }
  Flip $mate; Learn (S); $s = S; "   ($i,$mate) " + (Show $s)
  if ($s.Phase -eq 'ShowingMiss') { Key 'space' }
}
$s = S; if ($s.Phase -ne 'Cleared') { $left = 0..11 | ? { (($s.Cards -split ' ')[$_]) -eq '--' }; while ((S).Phase -notin 'Cleared','GameOver') { $s = S; $left = 0..11 | ? { (($s.Cards -split ' ')[$_]) -eq '--' }; $m = $left | ? { $_ -ne $left[0] -and $script:seen[$_] -eq $script:seen[$left[0]] } | select -First 1; Flip $left[0]; Flip $m } }
$b = S; $want = 100 - 10*$b.X + 100+200+300+400+500 # 最初の成立→ミス→5連続
Chk "M5 全ペア成立でクリア (スコア期待 $want)" ($b.Phase -eq 'Cleared' -and $b.M -eq 6 -and $b.Score -eq $want) $b; Shot "M5_clear$Tag"
Key 'space'; Key 'upArrow'; $s = S; Chk 'M5 クリア後は入力を受けない' (Same $b $s) $s
Key 'r'; $s = S; Chk 'M6 クリア後のR' (Init $s) $s
$diff = $false; foreach ($p in @(0,1),@(2,3)) { Flip $p[0]; Flip $p[1]; $c = (S).Cards -split ' '; foreach ($i in $p) { if ($c[$i].Trim('*').Substring(0,1) -ne $script:seen[$i]) { $diff = $true } }; if ((S).Phase -eq 'ShowingMiss') { Key 'space' } }
$s = S; Chk 'M6 配置が変わった' $diff $s
StopPlay
"held=[" + ((UC get_current_input_state).data.keyboard.pressedKeys -join ',') + "]  NG=$($script:ng)"
