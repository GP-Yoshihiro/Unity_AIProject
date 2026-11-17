# CryptEscape helpers (dot-source)
$script:u = "$HOME\.unity\tools\unity-cli\win-x64\unity-cli.exe"
Set-Location C:\Users\koyo\Desktop\AI

function UC([string]$tool, [string]$json = '{}') {
    & $script:u --output json raw $tool --json $json 2>$null | ConvertFrom-Json
}

function Eval([string]$code) {
    $r = & $script:u --output json editor eval --mode statements $code 2>$null | ConvertFrom-Json
    if ($r.data.exception) { "EXCEPTION: $($r.data.exception | ConvertTo-Json -Compress -Depth 4)" }
    $errs = $r.data.diagnostics | Where-Object severity -eq 'error'
    if ($errs) { "COMPILE: " + (($errs | ForEach-Object { "L$($_.line): $($_.message)" }) -join ' / ') }
    $r.data.value
}

function WaitReady {
    for ($i = 0; $i -lt 80; $i++) {
        Start-Sleep 2
        $s = & $script:u editor status --output json 2>$null | ConvertFrom-Json
        if ($s.data.state -eq 'ready' -and -not $s.data.editor.isCompiling -and -not $s.data.editor.isUpdating) { return }
    }
    "WARN: editor not ready"
}

function Recompile {
    $null = UC refresh_assets
    Start-Sleep 2
    WaitReady
    $c = (UC get_compilation_state).data
    "compile errors=$($c.errorCount) warnings=$($c.warningCount)"
    if ($c.errorCount -gt 0) {
        $logs = (UC read_console '{"count":30,"logTypes":["Error"]}').data.logs
        $logs | Select-Object -First 15 | ForEach-Object { "  " + ($_.message -split "`n")[0] }
    }
}

function GD {
    (UC get_component_values '{"gameObjectName":"Game","componentType":"GameDirector"}').data.properties
}

function S {
    $g = GD
    $p = (UC get_component_values '{"gameObjectName":"Player","componentType":"Transform"}').data.properties.position.value
    $pos = if ($p) { '({0:0.0},{1:0.0},{2:0.0})' -f $p.x, $p.y, $p.z } else { '-' }
    "flow=$($g.flow.value) scenes=$($g.loadedScenes.value) dlg=$($g.topDialog.value)/$($g.dialogSelection.value) coins=$($g.coinCount.value) hearts=$($g.hearts.value) lives=$($g.lives.value) resp=$($g.respawnPointId.value) door=$($g.bossDoorOpen.value) boss=$($g.bossState.value):$($g.bossHp.value) exit=$($g.exitOpen.value) t=$('{0:0.00}' -f $g.elapsed.value) pos=$pos prompt='$($g.prompt.value)' bgm=$($g.bgm.value) snd=$($g.lastSounds.value)"
}

function Key([string]$key, [double]$hold = 0.1) {
    $null = UC input_keyboard (@{ key = $key; action = 'press'; holdSeconds = $hold } | ConvertTo-Json -Compress)
    Start-Sleep -Milliseconds ([int](($hold + 0.4) * 1000))
}

function WaitFlow([string]$flow, [int]$timeoutSec = 15) {
    for ($i = 0; $i -lt $timeoutSec * 4; $i++) {
        $g = GD
        if ($g.flow.value -eq $flow -and -not $g.sceneBusy.value) { return $true }
        Start-Sleep -Milliseconds 250
    }
    return $false
}

function StartPlay {
    $null = UC play_game
    for ($i = 0; $i -lt 40; $i++) {
        Start-Sleep 1
        $s = & $script:u editor status --output json 2>$null | ConvertFrom-Json
        if ($s.data.editor.isPlaying) { break }
    }
    $null = UC manage_windows '{"action":"focus","windowType":"GameView"}'
    Start-Sleep 2
    "isPlaying=$($s.data.editor.isPlaying)"
}

function StopPlay {
    $null = UC stop_game
    Start-Sleep 2
    $s = & $script:u editor status --output json 2>$null | ConvertFrom-Json
    "isPlaying=$($s.data.editor.isPlaying)"
}

function Shot([string]$name) {
    $dir = "specs\001-fantasy-coin-escape\evidence"
    New-Item -ItemType Directory -Force $dir | Out-Null
    $null = UC manage_windows '{"action":"focus","windowType":"GameView"}'
    Start-Sleep -Milliseconds 400
    $r = UC capture_screenshot '{"captureMode":"game","includeUI":true,"width":1280,"height":720}'
    if (-not $r.success) { return "shot failed: $($r.errors[0].message)" }
    Copy-Item $r.data.path "$dir\$name.png"
    "shot: $name.png"
}

function Held { "held=[" + ((UC get_current_input_state).data.keyboard.pressedKeys -join ',') + "]" }

function RunTests {
    $start = & $script:u --output json raw run_tests --json '{"testMode":"EditMode","filter":"CryptEscape.Tests"}' 2>$null | ConvertFrom-Json
    $runId = $start.data.runId
    for ($i = 0; $i -lt 80; $i++) {
        Start-Sleep 3
        $r = UC get_test_status '{"includeTestResults":true}'
        if ($r.data.runId -eq $runId -and $r.data.status -eq 'completed') { break }
    }
    "tests: runId=$runId matched=$($r.data.runId -eq $runId) status=$($r.data.status) passed=$($r.data.passedTests) failed=$($r.data.failedTests) skipped=$($r.data.skippedTests) inconclusive=$($r.data.inconclusiveTests)"
    $script:LastTests = $r.data.tests
    $r.data.tests | Where-Object status -ne 'Passed' | ForEach-Object { "  FAIL $($_.fullName): $($_.message)" }
}
