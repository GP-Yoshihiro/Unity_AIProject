. "$PSScriptRoot/mm.ps1"; . "$PSScriptRoot/lib2.ps1"
$know=@{}; function Learn2 { (Up (V)).GetEnumerator() | % { $know[$_.Key]=$_.Value } }
function Sc($v,$p) { [int](($v.Scores -split ' ')[$p]) }
"-- 対CPU 2人"
StartPlay; Menu 1 0; $v=V; Chk2 'B1 対CPU・2人を選択' ($v.Mode -eq 'VsCpu' -and [int]$v.PlayerCount -eq 2) $v; Shot 'B1_menu_vscpu'; FastKey 'space'; Start-Sleep -Milliseconds 800
$v=V; Chk2 'B8 52枚・全て裏' (@(Down $v).Count -eq 52 -and $v.Phase -eq 'PickFirst') $v; Shot 'B8_trump_start'
$blocked=$false;$cpuPairs=0;$shotMiss=$false;$shotTake=$false;$bad=0;$hm=0;$ht=0;$cpuT=0;$maxMiss=0
$sw=[Diagnostics.Stopwatch]::StartNew()
while (($v=V).Phase -notin 'Cleared','GameOver' -and $sw.Elapsed.TotalSeconds -lt 520) {
  if ([int]$v.CurrentPlayer -eq 1) {
    if ($v.Phase -eq 'ShowingMiss' -and -not $shotCpu -and $blocked) { Shot 'B5_cpu_miss'; $shotCpu=$true }; if ($v.Phase -ne 'PickFirst') { (Up $v).GetEnumerator() | % { $know[$_.Key]=$_.Value } }
    if (-not $blocked -and $v.Phase -eq 'ShowingMiss') { $u0=$v.Cards; $c0=$v.Cursor; FastKey 'space'; FastKey 'upArrow'; FastKey 'leftArrow'; $n=V; if ($n.Phase -eq 'ShowingMiss') { Chk2 'B5 CPU手番中は矢印・Spaceが効かない' ($n.Cards -eq $u0 -and $n.Cursor -eq $c0) $n; $blocked=$true } }
    Start-Sleep -Milliseconds 120; continue }
  if ($v.Phase -eq 'ShowingMiss') { FastKey 'space'; Start-Sleep -Milliseconds 150; $n=V; if ([int]$n.CurrentPlayer -ne 1) { $bad++; "   NG 交代しない $(VS $n)" }; $cpuT++; continue }
  Learn2
  $dn=@(Down $v); $pair=$null
  $g=$dn | ? { $know[$_] } | group { Rank $know[$_] } | ? Count -ge 2 | select -First 1
  if ($g) { $a=$g.Group[0] } else { $a=$dn | ? { -not $know[$_] } | select -First 1 }
  $s0=Sc $v 0; $k0=[int]$v.Combo; $p0=[int](($v.Pairs -split ' ')[0])
  Flip2 $a; Learn2
  $c=$dn | ? { $_ -ne $a -and $know[$_] -and (Rank $know[$_]) -eq (Rank $know[$a]) } | select -First 1
  if ($null -eq $c) { $c=$dn | ? { $_ -ne $a -and -not $know[$_] } | select -First 1 }
  if ($null -eq $c) { $c=$dn | ? { $_ -ne $a } | select -First 1 }
  Flip2 $c; Learn2; $n=V
  $same=(Rank $know[$a]) -eq (Rank $know[$c])
  if ($same) { $ht++; $want=$s0+100*[math]::Min($k0+1,5); $ow=($n.Owners -split ' '); $ok=(Sc $n 0) -eq $want -and [int](($n.Pairs -split ' ')[0]) -eq $p0+1 -and $ow[$a] -eq '0' -and $ow[$c] -eq '0' -and [int]$n.CurrentPlayer -eq 0
    if (-not $shotTake) { Chk2 "B2/B8 $($know[$a])+$($know[$c]) を取得し続行 (+$($want-$s0))" $ok $n; Shot 'B2_take'; $shotTake=$true } }
  else { $hm++; $want=[math]::Max(0,$s0-10); $ok=(Sc $n 0) -eq $want -and $n.Phase -eq 'ShowingMiss'
    if (-not $shotMiss) { Chk2 "B3 $($know[$a])/$($know[$c]) ミス" $ok $n; Shot 'B3_miss'; $shotMiss=$true } }
  if (-not $ok) { $bad++; "   NG $($know[$a])/$($know[$c]) want=$want $(VS $n)" }
}
$v=V; "   人間: 取得 $ht ミス $hm ／ CPUへ交代 $cpuT 回 ／ 時間 $([int]$sw.Elapsed.TotalSeconds)s ／ 不一致 $bad"
$sc=$v.Scores -split ' ' | % {[int]$_}; $want=@('P1','CPU2'); 
Chk2 'B2/B3 人間の全手番で得点・持ち主・交代が期待どおり' ($bad -eq 0 -and $hm -ge 1) $v
Chk2 'B4 26ペアで終了し、スコア順の順位' ($v.Phase -eq 'Cleared' -and [int]$v.Matched -eq 26 -and $v.Result -match "1\. $(if($sc[0] -ge $sc[1]){'P1'}else{'CPU2'}) +$(($sc|measure -Max).Maximum)") $v; Shot 'B4_ranking'
Chk2 'B4 ミス5回を超えても終了しない' ([int]$v.Misses -gt 5) $v
Chk2 'B5 入力の遮断を観測できた' $blocked $v
FastKey 'r'; Start-Sleep -Milliseconds 300; $v=V; Chk2 'B7 R: 同じモード・人数で初期化' ($v.Mode -eq 'VsCpu' -and $v.Phase -eq 'PickFirst' -and $v.Scores -eq '0 0' -and $v.Pairs -eq '0 0' -and [int]$v.CurrentPlayer -eq 0 -and @(Down $v).Count -eq 52) $v
FastKey 'escape'; Start-Sleep -Milliseconds 300; $v=V; Chk2 'B7 Esc: モード選択へ戻る' ($v.Phase -eq 'Menu') $v
StopPlay; "held=[" + ((UC get_current_input_state).data.keyboard.pressedKeys -join ',') + "] NG=$($script:ng)"
