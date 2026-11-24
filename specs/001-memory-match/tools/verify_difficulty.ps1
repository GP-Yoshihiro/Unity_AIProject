# 3つの難易度とコンボの実証。めくって見えた絵柄だけを記憶する。
. "$PSScriptRoot/mm.ps1"; . "$PSScriptRoot/lib2.ps1"
function Play1($opt) { StartPlay; Menu 0 $opt; FastKey 'space'; Start-Sleep 1 }
foreach ($d in @(@('EASY',0,12,4,3,5), @('NORMAL',1,16,4,4,10), @('HARD',2,24,6,4,20))) {
  "-- 難易度 $($d[0])"; Play1 $d[1]
  $v=V; Chk2 "D1 $($d[0]) 枚数・並び・初期" ($v.Difficulty -eq $d[0] -and [int]$v.Columns -eq $d[3] -and [int]$v.Rows -eq $d[4] -and [int]$v.MissLimit -eq $d[5] -and @(Down $v).Count -eq $d[2]) $v; Shot "D1_$($d[0])"
  $know=@{}
  while (($v=V).Phase -notin 'GameOver','Cleared') {
    if ($v.Phase -eq 'ShowingMiss') { FastKey 'space'; $v=V }
    $dn=@(Down $v); $unk=@($dn | ? { -not $know[$_] })
    if ($unk.Count -ge 2) { $a=$unk[0]; $c=$unk[1] } elseif ($unk.Count -eq 1) { $a=$unk[0]; $c=$dn | ? { $_ -ne $a } | select -First 1 } else { $a=$dn[0]; $c=$dn | ? { $know[$_] -ne $know[$a] } | select -First 1 }
    Flip2 $a; (Up (V)).GetEnumerator() | % { $know[$_.Key]=$_.Value }; Flip2 $c; (Up (V)).GetEnumerator() | % { $know[$_.Key]=$_.Value }
  }
  $v=V; Chk2 "D2 $($d[0]) ミス上限でゲームオーバー・最後の2枚は表" ($v.Phase -eq 'GameOver' -and [int]$v.Misses -eq $d[5] -and (Up $v).Count - 2*[int]$v.Matched -eq 2) $v; Shot "D2_$($d[0])_gameover"
  FastKey 'r'; Start-Sleep -Milliseconds 300; $v=V; Chk2 "D3 $($d[0]) Rで同じ難易度の初期状態" ($v.Difficulty -eq $d[0] -and $v.Phase -eq 'PickFirst' -and @(Down $v).Count -eq $d[2] -and [int]$v.Misses -eq 0) $v
  StopPlay; Play1 $d[1]
  while (($v=V).Phase -notin 'Cleared','GameOver') {
    if ($v.Phase -eq 'ShowingMiss') { FastKey 'space'; $v=V }
    $dn=@(Down $v); $a=$dn | ? { $x=$_; $know[$x] -and @($dn | ? { $_ -ne $x -and $know[$_] -eq $know[$x] }).Count } | select -First 1
    if ($null -eq $a) { $a=$dn | ? { -not $know[$_] } | select -First 1 }
    Flip2 $a; (Up (V)).GetEnumerator() | % { $know[$_.Key]=$_.Value }
    $c=$dn | ? { $_ -ne $a -and $know[$_] -eq $know[$a] } | select -First 1
    if ($null -eq $c) { $c=$dn | ? { $_ -ne $a -and -not $know[$_] } | select -First 1 }
    if ($null -eq $c) { $c=$dn | ? { $_ -ne $a } | select -First 1 }
    Flip2 $c; (Up (V)).GetEnumerator() | % { $know[$_.Key]=$_.Value }
  }
  $v=V; $n=$d[2]/2; $want=0; 1..$n | % { $want += 100*[math]::Min($_,5) }
  Chk2 "D2 $($d[0]) 全ペアでクリア" ($v.Phase -eq 'Cleared' -and [int]$v.Matched -eq $n) $v
  Chk2 "A1 $($d[0]) ${n}連続・上限5倍の得点 $want" ([int]$v.Misses -eq 0 -and [int]$v.Score -eq $want) $v; Shot "D2_$($d[0])_clear"
  FastKey 'r'; Start-Sleep -Milliseconds 300; $v=V; Chk2 "A4 Rで連続数0" ([int]$v.Combo -eq 0 -and [int]$v.Score -eq 0) $v
  StopPlay
}
"held=[" + ((UC get_current_input_state).data.keyboard.pressedKeys -join ',') + "] NG=$($script:ng)"
