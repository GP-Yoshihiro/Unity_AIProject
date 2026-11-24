# E1〜E5 の実証。まず EASY を1度遊んで配置を覚え、同じシードで6連続成立させる。
. "$PSScriptRoot/mm.ps1"; . "$PSScriptRoot/lib2.ps1"
function FX { $p = (UC get_component_values '{"gameObjectName":"Game","componentType":"ComboEffects"}').data.properties; [pscustomobject]@{ Level=[int]$p.Level.value; Combo=[int]$p.ComboShown.value; Points=[int]$p.PointsShown.value; Parts=[int]$p.ActiveParticles.value; Playing=[bool]$p.IsPlaying.value; Count=[int]$p.PlayCount.value; Text=($p.PopupText.value -replace "`n",' ') } }
function Fs($f) { "level=$($f.Level) combo=$($f.Combo) +$($f.Points) particles=$($f.Parts) playing=$($f.Playing) count=$($f.Count) text='$($f.Text)'" }
function ChkF($n, $c, $f) { if (-not $c) { $script:ng++ }; "{0} {1} | {2}" -f $(if ($c) {'OK '} else {'NG '}), $n, (Fs $f) }
function FastShot($n) { $r = UC capture_screenshot '{"captureMode":"game","includeUI":true,"width":1280,"height":720}'; if ($r.success) { Copy-Item $r.data.path "$script:root/specs/001-memory-match/evidence/$n.png" } else { "shot failed" } }
function StagePos { $p = (UC get_component_values '{"gameObjectName":"Stage","componentType":"RectTransform"}').data.properties.anchoredPosition.value; "$($p.x),$($p.y)" }
function WaitIdle { for ($i=0;$i -lt 30 -and (FX).Playing;$i++) { Start-Sleep -Milliseconds 150 } }

"-- 配置を覚える"
StartPlay; FastKey 'space'; Start-Sleep 1; $home0 = StagePos; $know=@{}
while (($v=V).Phase -ne 'GameOver') { if ($v.Phase -eq 'ShowingMiss') { FastKey 'space'; $v=V }
  $dn=@(Down $v); $unk=@($dn | ? { -not $know[$_] }); if ($unk.Count -ge 2) { $a=$unk[0];$c=$unk[1] } else { $a=$dn[0]; $c=$dn | ? { $know[$_] -and $know[$_] -ne $know[$a] } | select -First 1 }
  Flip2 $a; (Up (V)).GetEnumerator() | % { $know[$_.Key]=$_.Value }; Flip2 $c; (Up (V)).GetEnumerator() | % { $know[$_.Key]=$_.Value } }
$f=FX; ChkF 'E5 ミスではコンボの演出が出ない' ($f.Count -eq (V).Matched -and -not $f.Playing) $f
StopPlay
# 残りの2枚を覚える
StartPlay; FastKey 'space'; Start-Sleep 1; $rest=@(0..11 | ? { -not $know[$_] }); foreach ($i in $rest) { Flip2 $i; (Up (V)).GetEnumerator() | % { $know[$_.Key]=$_.Value } }
StopPlay
"-- 6連続"
StartPlay; FastKey 'space'; Start-Sleep 1
for ($n=1; $n -le 6; $n++) {
  $dn=@(Down (V)); $a=$dn | ? { $x=$_; $know[$x] -and @($dn | ? { $_ -ne $x -and $know[$_] -eq $know[$x] }).Count } | select -First 1
  if ($null -eq $a) { $a=$dn | ? { $know[$_] } | select -First 1 }
  $c=$dn | ? { $_ -ne $a -and $know[$_] -eq $know[$a] } | select -First 1; if ($null -eq $c) { $c=$dn | ? { $_ -ne $a -and -not $know[$_] } | select -First 1 }
  Flip2 $a; Go2 $c
  $null = UC input_keyboard '{"key":"space","action":"press","holdSeconds":0.05}'; Start-Sleep -Milliseconds 60
  FastShot "E1_combo$n"; $f=FX; $v=V
  $txt = if ($n -ge 2) { "$n COMBO! +$(100*[math]::Min($n,5))" } else { '+100' }
  ChkF "E1/E2 ${n}連続 → 段階$([math]::Min($n,5))・$txt" ($f.Level -eq [math]::Min($n,5) -and $f.Combo -eq $n -and $f.Text -eq $txt -and $f.Count -eq $n -and [int]$v.Combo -eq $n) $f
  if ($n -eq 3) { $cur=$v.Cursor; FastKey $(if ($cur % 4) {'leftArrow'} else {'rightArrow'}); $f=FX; $v=V; ChkF "E4 演出中もカーソルが動く ($cur→$($v.Cursor))" ($f.Playing -and $v.Cursor -ne $cur) $f }
  WaitIdle
}
$f=FX; ChkF "E4 自然に終わり盤面が元の位置 ($(StagePos))" (-not $f.Playing -and $f.Parts -eq 0 -and (StagePos) -eq $home0) $f
FastKey 'r'; Start-Sleep -Milliseconds 300
"-- R / Esc で演出が消える（別の配置なので成立するまで探す）"
foreach ($key in 'r','escape') {
  $kn=@{}; $done=$false
  for ($k=0; $k -lt 30 -and -not $done; $k++) { $v=V; if ($v.Phase -eq 'ShowingMiss') { FastKey 'space'; $v=V }; if ($v.Phase -eq 'GameOver') { FastKey 'r'; $kn=@{}; Start-Sleep -Milliseconds 200; $v=V }
    $dn=@(Down $v); $a=$dn | ? { $x=$_; $kn[$x] -and @($dn | ? { $_ -ne $x -and $kn[$_] -eq $kn[$x] }).Count } | select -First 1; if ($null -eq $a) { $a=$dn | ? { -not $kn[$_] } | select -First 1 }
    Flip2 $a; (Up (V)).GetEnumerator() | % { $kn[$_.Key]=$_.Value }; $c=$dn | ? { $_ -ne $a -and $kn[$_] -eq $kn[$a] } | select -First 1; if ($null -eq $c) { $c=$dn | ? { $_ -ne $a -and -not $kn[$_] } | select -First 1 }
    $m0=[int](V).Matched; Go2 $c; $null = UC input_keyboard '{"key":"space","action":"press","holdSeconds":0.05}'; Start-Sleep -Milliseconds 60; $v=V; (Up $v).GetEnumerator() | % { $kn[$_.Key]=$_.Value }
    if ([int]$v.Matched -gt $m0) { $f0=FX; FastKey $key; $f=FX; $v=V; ChkF "E4 演出中の $key で消える (直前 playing=$($f0.Playing))" ($f0.Playing -and -not $f.Playing -and $f.Text -eq '' -and $(if ($key -eq 'r') { $v.Phase -eq 'PickFirst' -and [int]$v.Matched -eq 0 } else { $v.Phase -eq 'Menu' })) $f; $done=$true } }
  if (-not $done) { $script:ng++; "NG $key 未成立" }
}
"-- 対CPU: CPUの成立でも演出"
Start-Sleep -Milliseconds 300; FastKey 'downArrow'; FastKey 'space'; Start-Sleep -Milliseconds 500; $c0=(FX).Count; $hit=$false; $i=0; $sw=[Diagnostics.Stopwatch]::StartNew()
while (-not $hit -and $sw.Elapsed.TotalSeconds -lt 150) { $v=V; $f=FX
  if ($f.Count -gt $c0 -and [int]$v.CurrentPlayer -eq 1 -and $f.Playing) { FastShot 'E3_cpu_combo'; ChkF 'E3 CPUの成立で演出' ($f.Combo -ge 1 -and $f.Points -eq 100*[math]::Min($f.Combo,5)) $f; $hit=$true; break }
  $c0=[math]::Max($c0,$f.Count)
  if ([int]$v.CurrentPlayer -eq 0) { if ($v.Phase -eq 'ShowingMiss') { FastKey 'space' } else { $dn=@(Down $v); Flip2 ($dn | Get-Random) } } else { Start-Sleep -Milliseconds 80 } }
if (-not $hit) { 'E3 は時間内に観測できず（未確認）' }
StopPlay
"held=[" + ((UC get_current_input_state).data.keyboard.pressedKeys -join ',') + "] NG=$($script:ng)"
