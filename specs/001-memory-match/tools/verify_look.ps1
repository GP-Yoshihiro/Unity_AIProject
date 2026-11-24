# V1〜V4 の画面を撮る
. "$PSScriptRoot/mm.ps1"; . "$PSScriptRoot/lib2.ps1"
function Panel($n) { $r=@{}; foreach ($k in 'Title','Tag','Value','Detail') { $r[$k] = (UC get_component_values (@{gameObjectName="StatPanel_${n}_$k";componentType='Text'}|ConvertTo-Json -Compress)).data.properties.text.value }; [pscustomobject]$r }
function ChkPanels($label) { $v=V; $sc=$v.Scores -split ' '; $pr=$v.Pairs -split ' '; $ok=$true; for ($p=0;$p -lt [int]$v.PlayerCount;$p++) { $q=Panel $p; $name=$(if ($v.Mode -eq 'VsCpu' -and $p -gt 0) {"CPU$($p+1)"} else {"P$($p+1)"})
  if ($q.Title -ne $name -or $q.Value -ne ([int]$sc[$p]).ToString('#,0') -or $q.Detail -notmatch "PAIRS\s+$($pr[$p])\n" -or ($q.Tag -eq 'TURN') -ne ($p -eq [int]$v.CurrentPlayer)) { $ok=$false; "   NG 枠$p $($q|ConvertTo-Json -Compress)" } }; Chk2 $label $ok $v }
StartPlay; Shot 'V4_menu'; FastKey 'space'; Start-Sleep 1; Flip2 0; Flip2 1; Start-Sleep 1; Shot 'V2_solo'; $q=(0..3 | % { $z=Panel $_; "$($z.Title)=$($z.Value)" }) -join ', '; Chk2 "V2 1人用の枠 ($q)" ($q -eq 'PAIRS=0 / 6, MISS=1 / 5, SCORE=0, COMBO=0') (V)
FastKey 'escape'; Start-Sleep -Milliseconds 300; FastKey 'downArrow'; FastKey 'downArrow'; FastKey 'rightArrow'; FastKey 'rightArrow'; FastKey 'space'; Start-Sleep 1
$know=@{}; for($i=0;$i -lt 40;$i+=2){ Flip2 $i; Flip2 ($i+1); $v=V; (Up $v).GetEnumerator()|%{$know[$_.Key]=$_.Value}; if($v.Phase -eq 'ShowingMiss'){FastKey 'space'; Start-Sleep -Milliseconds 100} }
$n=0; foreach($g in ($know.Keys | group { Rank $know[$_] })) { $dn=@(Down (V)); $k=@($g.Group | ? { $_ -in $dn }); if($k.Count -ge 2 -and $n -lt 9){ Flip2 $k[0]; Flip2 $k[1]; $n++; if($n % 3 -eq 2){ $d=@(Down (V)); Flip2 $d[0]; $e=$d | ? { $_ -ne $d[0] -and (Rank $know[$_]) -ne (Rank $know[$d[0]]) } | select -First 1; Flip2 $e; FastKey 'space'; Start-Sleep -Milliseconds 100 } } }
Start-Sleep 3; $d=@(Down (V)); Flip2 $d[0]; $e=$d | ? { $_ -ne $d[0] -and $know[$_] -and (Rank $know[$_]) -ne (Rank $know[$d[0]]) } | select -First 1; Flip2 $e; Start-Sleep 1; Shot 'V1_versus4'; ChkPanels 'V1 4人の枠が状態と一致し手番だけTURN'
FastKey 'escape'; Start-Sleep -Milliseconds 300; FastKey 'upArrow'; FastKey 'leftArrow'; FastKey 'leftArrow'; FastKey 'space'; Start-Sleep -Milliseconds 600; Flip2 0; Flip2 1; FastKey 'space'; Start-Sleep -Milliseconds 400; Shot 'V1_vscpu'; ChkPanels 'V1 対CPUの枠'; StopPlay
(UC read_console '{"count":10,"logTypes":["Error"]}').data.logs | % { ($_.message -split "`n")[0] }
"held=[" + ((UC get_current_input_state).data.keyboard.pressedKeys -join ',') + "] NG=$($script:ng)"
