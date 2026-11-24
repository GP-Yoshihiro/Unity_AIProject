# 難易度と対戦の実証で使う共通関数。mm.ps1 の後に読み込む。
$script:ng = 0
function V { $p = (UC get_component_values '{"gameObjectName":"Game","componentType":"MemoryGame"}').data.properties; $o = @{}; foreach ($n in 'Phase','Mode','Difficulty','PlayerCount','CurrentPlayer','Matched','Misses','MissLimit','Score','Combo','Cursor','Columns','Rows','Pairs','Scores','Owners','Cards','Result') { $o[$n] = $p.$n.value }; [pscustomobject]$o }
function VS($v) { "$($v.Mode)$($v.Difficulty) $($v.Phase) turn=$($v.CurrentPlayer)/$($v.PlayerCount) match=$($v.Matched) miss=$($v.Misses)/$($v.MissLimit) score=[$($v.Scores)] pairs=[$($v.Pairs)] combo=$($v.Combo) cur=$($v.Cursor) result='$($v.Result -replace "`n",' | ')'" }
function Chk2($name, $cond, $v) { if (-not $cond) { $script:ng++ }; "{0} {1} | {2}" -f $(if ($cond) {'OK '} else {'NG '}), $name, (VS $v) }
function FastKey($k) { $null = UC input_keyboard (@{ key = $k; action = 'press'; holdSeconds = 0.05 } | ConvertTo-Json -Compress); Start-Sleep -Milliseconds 130 }
function Go2([int]$i) { for ($n = 0; $n -lt 6; $n++) { $v = V; $c = [int]$v.Columns; $cur = [int]$v.Cursor; if ($cur -eq $i) { return }
  $dx = $i % $c - $cur % $c; $dy = [math]::Floor($i/$c) - [math]::Floor($cur/$c)
  for ($k=0;$k -lt [math]::Abs($dx);$k++) { FastKey $(if ($dx -gt 0) {'rightArrow'} else {'leftArrow'}) }; for ($k=0;$k -lt [math]::Abs($dy);$k++) { FastKey $(if ($dy -gt 0) {'downArrow'} else {'upArrow'}) } }
  Write-Host "GO2 FAIL $i" }
function Flip2([int]$i) { Go2 $i; $b = (V).Cards; FastKey 'space'; for ($n=0;$n -lt 5;$n++) { if ((V).Cards -ne $b) { return }; Start-Sleep -Milliseconds 100 } ; Write-Host "FLIP2 no change $i" }
function Up($v) { $a = $v.Cards -split ' '; $h = @{}; for ($i=0;$i -lt $a.Count;$i++) { if ($a[$i] -ne '--') { $h[$i] = $a[$i].Trim('*') } }; $h }
function Rank($code) { if ($code.Length -gt 1) { $code.Substring(1) } else { $code } }
function Down($v) { $a = $v.Cards -split ' '; 0..($a.Count-1) | ? { $a[$_] -eq '--' } }
function Menu([int]$mode, [int]$opt) { for ($k=0;$k -lt $mode;$k++) { FastKey 'downArrow' }; for ($k=0;$k -lt $opt;$k++) { FastKey 'rightArrow' } }
