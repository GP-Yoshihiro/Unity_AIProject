$script:u = "$HOME\.unity\tools\unity-cli\win-x64\unity-cli.exe"
$script:root = (Resolve-Path "$PSScriptRoot/../../..").Path
Set-Location $script:root
function UC([string]$tool, [string]$json = '{}') { & $script:u --output json raw $tool --json $json 2>$null | ConvertFrom-Json }
function Eval([string]$code) {
    $r = & $script:u --output json editor eval --mode statements $code 2>&1 | Out-String
    try { $j = $r | ConvertFrom-Json } catch { return "RAW: $r" }
    if ($j.data.exception) { "EXC: $($j.data.exception | ConvertTo-Json -Compress -Depth 4)" }
    $e = $j.data.diagnostics | ? severity -eq 'error'; if ($e) { "COMPILE: " + (($e | % { "L$($_.line): $($_.message)" }) -join ' / ') }
    if (-not $j.success) { "FAILED: $r" }
    $j.data.value
}
function Recompile {
    $null = UC refresh_assets; Start-Sleep 2
    for ($i = 0; $i -lt 60; $i++) {
        $s = & $script:u editor status --output json 2>$null | ConvertFrom-Json
        if ($s.data.state -eq 'ready' -and -not $s.data.editor.isCompiling -and -not $s.data.editor.isUpdating) { break }
        Start-Sleep 2
    }
    $c = (UC get_compilation_state).data
    "compile errors=$($c.errorCount)"
    if ($c.errorCount) { (UC read_console '{"count":20,"logTypes":["Error"]}').data.logs | select -First 8 | % { "  " + ($_.message -split "`n")[0] } }
}
function RunTests {
    $st = UC run_tests '{"testMode":"EditMode","filter":"MemoryMatch.Tests"}'
    $id = $st.data.runId
    for ($i = 0; $i -lt 60; $i++) { Start-Sleep 2; $r = UC get_test_status '{"includeTestResults":true}'; if ($r.data.runId -eq $id -and $r.data.status -eq 'completed') { break } }
    "tests: matched=$($id -and $r.data.runId -eq $id) status=$($r.data.status) total=$(@($r.data.tests).Count) passed=$($r.data.passedTests) failed=$($r.data.failedTests) skipped=$($r.data.skippedTests)"
    if (-not $id) { $st | ConvertTo-Json -Depth 5 -Compress }
    $r.data.tests | ? status -ne 'Passed' | % { "  FAIL $($_.fullName): $(($_.message -replace '\s+',' '))" }
}
function Cycle { Recompile; RunTests }
function S {
    $p = (UC get_component_values '{"gameObjectName":"Game","componentType":"MemoryGame"}').data.properties
    [pscustomobject]@{ Phase=$p.Phase.value; M=$p.Matched.value; X=$p.Misses.value; Score=$p.Score.value; Combo=$p.Combo.value; Cur=$p.Cursor.value; Cards=$p.Cards.value }
}
function Show($s) { "$($s.Phase) match=$($s.M) miss=$($s.X) score=$($s.Score) combo=$($s.Combo) cur=$($s.Cur) [$($s.Cards)]" }
function Key([string]$k) {
    $r = UC input_keyboard (@{ key = $k; action = 'press'; holdSeconds = 0.08 } | ConvertTo-Json -Compress)
    if (-not $r.success) { Write-Host "KEY FAIL $k $($r | ConvertTo-Json -Compress -Depth 4)" }
    Start-Sleep -Milliseconds 300
}
function GoTo([int]$i) {
    for ($n = 0; $n -lt 12; $n++) {
        $c = [int](S).Cur; if ($c -eq $i) { return }
        if ([math]::Floor($c/4) -lt [math]::Floor($i/4)) { Key 'downArrow' } elseif ([math]::Floor($c/4) -gt [math]::Floor($i/4)) { Key 'upArrow' }
        elseif ($c % 4 -lt $i % 4) { Key 'rightArrow' } else { Key 'leftArrow' }
    }
    Write-Host "GOTO FAIL $i"
}
function Flip([int]$i) { GoTo $i; Key 'space' }
function StartPlay {
    $null = UC play_game
    for ($i = 0; $i -lt 30; $i++) { Start-Sleep 1; $s = & $script:u editor status --output json 2>$null | ConvertFrom-Json; if ($s.data.editor.isPlaying) { break } }
    $null = UC manage_windows '{"action":"focus","windowType":"GameView"}'; Start-Sleep 2
    "playing=$($s.data.editor.isPlaying)"
}
function StopPlay { $null = UC stop_game; Start-Sleep 3; $s = & $script:u editor status --output json | ConvertFrom-Json; "playing=$($s.data.editor.isPlaying)" }
function Shot([string]$n) {
    $null = UC manage_windows '{"action":"focus","windowType":"GameView"}'; Start-Sleep -Milliseconds 300
    $r = UC capture_screenshot '{"captureMode":"game","includeUI":true,"width":1280,"height":720}'
    if (-not $r.success) { return "shot failed $($r | ConvertTo-Json -Compress -Depth 4)" }
    Copy-Item $r.data.path "$script:root\specs\001-memory-match\evidence\$n.png"; "shot $n"
}

# 検証用のシードを Edit Mode で設定する（シーンは保存しない）
function UseSeed([int]$seed) { Eval ('var g=UnityEngine.GameObject.Find("Game").GetComponent("MemoryGame"); var so=new UnityEditor.SerializedObject(g); so.FindProperty("useFixedSeed").boolValue=true; so.FindProperty("fixedSeed").intValue=' + $seed + '; so.ApplyModifiedPropertiesWithoutUndo(); return "seed (unsaved)";') }
# 保存済みのシーンを読み直し、検証用の設定を捨てる
function ReloadScene { Eval 'UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/MemoryMatch/Scenes/MemoryMatch.unity"); return "reloaded";' }
