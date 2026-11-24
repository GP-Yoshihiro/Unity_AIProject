. "$PSScriptRoot/mm.ps1"; . "$PSScriptRoot/lib2.ps1"
"-- 対CPU 4人: 手番の巡回（1周）"
StartPlay; Menu 1 2; $v=V; Chk2 'B1 対CPU・4人' ($v.Mode -eq 'VsCpu' -and [int]$v.PlayerCount -eq 4) $v; FastKey 'space'; Start-Sleep -Milliseconds 500
$order=@(); $sw=[Diagnostics.Stopwatch]::StartNew(); $i=0
while ($sw.Elapsed.TotalSeconds -lt 60) { $v=V; $c=[int]$v.CurrentPlayer; if ($order.Count -eq 0 -or $order[-1] -ne $c) { $order += $c }
  if ($order.Count -ge 5) { break }
  if ($c -eq 0) { if ($v.Phase -eq 'ShowingMiss') { FastKey 'space' } else { Flip2 $i; Flip2 ($i+1); $i+=2 } } else { Start-Sleep -Milliseconds 150 } }
$v=V; Chk2 "B5/B6 手番の順 $($order -join '→')" (($order -join '') -eq '01230') $v; Shot 'B6_cpu4'; FastKey 'escape'
"-- 対人 3人: 最後まで"
Start-Sleep -Milliseconds 300; FastKey 'downArrow'; FastKey 'leftArrow'; $v=V; Chk2 'B1 対人・3人' ($v.Mode -eq 'VsHuman' -and [int]$v.PlayerCount -eq 3) $v; Shot 'B1_menu_vshuman'; FastKey 'space'; Start-Sleep -Milliseconds 500
$know=@{}; function Learn2($v) { (Up $v).GetEnumerator() | % { $know[$_.Key]=$_.Value } }
$exp=@(0,0,0); $cb=@(0,0,0); $bad=0; $seen=@{}; $sw=[Diagnostics.Stopwatch]::StartNew()
while (($v=V).Phase -ne 'Cleared' -and $sw.Elapsed.TotalSeconds -lt 480) {
  $p=[int]$v.CurrentPlayer; $seen[$p]=1; $dn=@(Down $v)
  $g=$dn | ? { $know[$_] } | group { Rank $know[$_] } | ? Count -ge 2 | select -First 1
  $unk=@($dn | ? { -not $know[$_] })
  # 記憶を集めるあいだは未知の2枚をめくる
  if ($unk.Count -ge 2 -and $know.Count -lt 44) { $a=$unk[0]; $c=$unk[1] } elseif ($g) { $a=$g.Group[0]; $c=$g.Group[1] } else { $a=$unk[0]; $c=$dn | ? { $_ -ne $a } | select -First 1 }
  Flip2 $a; Learn2 (V); Flip2 $c; $n=V; Learn2 $n
  if ((Rank $know[$a]) -eq (Rank $know[$c])) { $cb[$p]++; $exp[$p]+=100*[math]::Min($cb[$p],5); if ($n.Phase -ne 'Cleared' -and [int]$n.CurrentPlayer -ne $p) { $bad++ } }
  else { $cb[$p]=0; $exp[$p]=[math]::Max(0,$exp[$p]-10); FastKey 'space'; Start-Sleep -Milliseconds 100; $n=V; if ([int]$n.CurrentPlayer -ne ($p+1)%3) { $bad++; "   NG 交代 $(VS $n)" } }
  if ($n.Scores -ne ($exp -join ' ')) { $bad++; "   NG 期待 $exp $(VS $n)" }
}
$v=V; "   時間 $([int]$sw.Elapsed.TotalSeconds)s 期待スコア $($exp -join ' ')"
Chk2 'B2/B3/B6 3人の得点と交代が全手番で期待どおり' ($bad -eq 0 -and $seen.Count -eq 3) $v
$rk = 0..2 | % { $s=$exp[$_]; 1 + @($exp | ? { $_ -gt $s }).Count }
$ok = $true; 0..2 | % { if ($v.Result -notmatch "$($rk[$_])\. P$($_+1) +$($exp[$_])(\D|$)") { $ok=$false } }
Chk2 "B4 3人の順位 (期待 $($rk -join ' '))" ($v.Phase -eq 'Cleared' -and $ok) $v; Shot 'B4_ranking_3p'
FastKey 'r'; Start-Sleep -Milliseconds 300; $v=V; Chk2 'B7 終了後のR' ($v.Mode -eq 'VsHuman' -and [int]$v.PlayerCount -eq 3 -and $v.Scores -eq '0 0 0' -and @(Down $v).Count -eq 52) $v
$know2=@{}; Flip2 0; Flip2 1; $v=V; $diff = (Up $v)[0] -ne $know[0] -or (Up $v)[1] -ne $know[1]; Chk2 'B7 新しい配置' $diff $v
StopPlay; "held=[" + ((UC get_current_input_state).data.keyboard.pressedKeys -join ',') + "] NG=$($script:ng)"
