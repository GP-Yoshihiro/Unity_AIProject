# Combat helpers for CryptEscape play tests (dot-source after ce.ps1 and nav.ps1)
# 入力はすべて unity-cli の入力シミュレーション。FightNearby / BossFight は nav.ps1 のものを置き換える。
$script:PlayLog = "$env:TEMP\cryptescape-play.log"

function Note([string]$m) {
    $line = "$(Get-Date -Format HH:mm:ss) $m"
    Add-Content $script:PlayLog $line
    Write-Host $line
}

# 複数の状態を並列に読む（約0.2秒）
function Snap([switch]$Boss, [switch]$Enemies) {
    $cli = $script:u
    $q = @(
        @{ k = 'g'; t = 'get_component_values'; j = '{"gameObjectName":"Game","componentType":"GameDirector"}' }
        @{ k = 'p'; t = 'get_component_values'; j = '{"gameObjectName":"Player","componentType":"Transform"}' }
    )
    if ($Boss) { $q += @{ k = 'b'; t = 'get_component_values'; j = '{"gameObjectName":"Boss","componentType":"Transform"}' } }
    if ($Enemies) { $q += @{ k = 'e'; t = 'get_hierarchy'; j = '{"rootPath":"/Dungeon/Enemies","maxDepth":1,"includeTransform":true}' } }
    $o = @{}
    $q | ForEach-Object -Parallel {
        $r = & $using:cli --output json raw $_.t --json $_.j 2>$null | ConvertFrom-Json
        [pscustomobject]@{ k = $_.k; d = $r.data }
    } -ThrottleLimit 5 | ForEach-Object { $o[$_.k] = $_.d }
    $g = $o.g.properties
    $p = $o.p.properties.position.value
    $s = [ordered]@{
        flow = $g.flow.value; hearts = $g.hearts.value; lives = $g.lives.value; coins = $g.coinCount.value
        state = $g.bossState.value; hp = $g.bossHp.value; x = $p.x; z = $p.z; dx = 0; dz = 0; d = 0; enemies = @()
    }
    if ($Boss) {
        $b = $o.b.properties.position.value
        $s.dx = $b.x - $p.x; $s.dz = $b.z - $p.z
        $s.d = [math]::Sqrt($s.dx * $s.dx + $s.dz * $s.dz)
    }
    if ($Enemies) {
        $s.enemies = @($o.e.hierarchy | Where-Object isActive | ForEach-Object {
                $dx = $_.transform.position.x - $p.x; $dz = $_.transform.position.z - $p.z
                [pscustomobject]@{ name = $_.name; dx = $dx; dz = $dz; d = [math]::Sqrt($dx * $dx + $dz * $dz) }
            } | Sort-Object d)
    }
    [pscustomobject]$s
}

function Stick([double]$dx, [double]$dz, [double]$sec, [double]$mag = 1.0) {
    $l = [math]::Sqrt($dx * $dx + $dz * $dz)
    if ($l -lt 0.001) { return }
    $null = UC input_gamepad (@{ action = 'stick'; stick = 'left'; x = [math]::Round($dx / $l * $mag, 3); y = [math]::Round($dz / $l * $mag, 3); holdSeconds = [math]::Round($sec, 3) } | ConvertTo-Json -Compress)
}

# ほぼその場で向きだけ変える
function Aim([double]$dx, [double]$dz) { Stick $dx $dz 0.06 0.4; Start-Sleep -Milliseconds 60 }

function Hit { $null = UC input_gamepad '{"action":"button","button":"a","buttonAction":"press","holdSeconds":0.05}' }

# 壁越しの敵を無視する
function ClearLine($s, $e) {
    $n = [math]::Max(1, [int]($e.d / 0.5))
    for ($i = 1; $i -le $n; $i++) {
        $c = PosCell @{ x = $s.x + $e.dx * $i / $n; z = $s.z + $e.dz * $i / $n }
        if ((CellAt $c.r $c.c) -eq '#') { return $false }
    }
    return $true
}

function FightNearby([double]$radius = 5.0, [int]$maxRounds = 30) {
    for ($i = 0; $i -lt $maxRounds; $i++) {
        $s = Snap -Enemies
        if ($s.flow -ne 'Playing') { return }
        $e = $s.enemies | Where-Object { $_.d -le $radius -and (ClearLine $s $_) } | Select-Object -First 1
        if (-not $e) { return }
        if ($e.d -gt 2.6) {
            Stick $e.dx $e.dz ([math]::Min(0.3, ($e.d - 2.2) / 6))
            Start-Sleep -Milliseconds 100
        }
        else {
            Aim $e.dx $e.dz
            Hit
            Start-Sleep -Milliseconds 100
            Hit
        }
    }
}

# ボスの間（x 22..50, z 36..50）の内側に収める
function ArenaClamp($x, $z) { @{ x = [math]::Min(49, [math]::Max(23, $x)); z = [math]::Min(49, [math]::Max(37, $z)) } }

function BossFight([int]$maxSteps = 250, [int]$StopAtHp = 0) {
    $hearts = -1
    $prev = ''
    for ($i = 0; $i -lt $maxSteps; $i++) {
        $s = Snap -Boss
        if ($s.flow -ne 'Playing' -or $s.state -eq 'Defeated') { Note "boss end flow=$($s.flow) state=$($s.state) hp=$($s.hp) hearts=$($s.hearts)"; return }
        if ($s.hp -le $StopAtHp) { Note "boss stop at hp=$($s.hp)"; return }
        $first = $s.state -ne $prev
        if ($first -or $s.hearts -ne $hearts) { Note ("boss {0} hp={1} hearts={2} d={3:0.0} pos=({4:0.0},{5:0.0})" -f $s.state, $s.hp, $s.hearts, $s.d, $s.x, $s.z) }
        $hearts = $s.hearts
        $prev = $s.state
        switch ($s.state) {
            'ChargeTell' {
                if ($first) {
                    # 突進の線に垂直な2方向のうち、広く動ける方へ
                    $best = $null
                    foreach ($sg in 1, -1) {
                        $nx = -$s.dz / $s.d * $sg; $nz = $s.dx / $s.d * $sg
                        $c = ArenaClamp ($s.x + $nx * 3.6) ($s.z + $nz * 3.6)
                        $room = [math]::Abs(($c.x - $s.x) * $nx + ($c.z - $s.z) * $nz)
                        if (-not $best -or $room -gt $best.room) { $best = @{ x = $nx; z = $nz; room = $room } }
                    }
                    Stick $best.x $best.z 0.6
                    Start-Sleep -Milliseconds 450
                }
            }
            'AoeTell' {
                if ($s.d -lt 4.3) {
                    $best = $null
                    for ($a = 0; $a -lt 16; $a++) {
                        $nx = [math]::Cos($a * [math]::PI / 8); $nz = [math]::Sin($a * [math]::PI / 8)
                        $c = ArenaClamp ($s.x + $nx * 2.6) ($s.z + $nz * 2.6)
                        $d = [math]::Sqrt([math]::Pow($c.x - $s.x - $s.dx, 2) + [math]::Pow($c.z - $s.z - $s.dz, 2))
                        if (-not $best -or $d -gt $best.d) { $best = @{ x = $nx; z = $nz; d = $d } }
                    }
                    Stick $best.x $best.z 0.43
                    Start-Sleep -Milliseconds 350
                }
            }
            'Charge' { }
            default {
                if ($s.d -gt 2.7) {
                    $t = [math]::Min(0.7, ($s.d - 2.2) / 6)
                    Stick $s.dx $s.dz $t
                    Start-Sleep -Milliseconds ([int]($t * 1000))
                    Hit
                }
                else {
                    Aim $s.dx $s.dz
                    Hit
                }
            }
        }
    }
    Note "boss timeout"
}

# 目的地まで。倒れたら復活を待ってやり直す
function Goal([string]$name, [int]$r, [int]$c) {
    for ($try = 0; $try -lt 8; $try++) {
        $f = (GD).flow.value
        if ($f -eq 'Dying') { $null = WaitFlow 'Playing' 5; $f = (GD).flow.value }
        if ($f -ne 'Playing') { Note "goal $name aborted flow=$f"; return $false }
        $m = GoToCell $r $c -Fight
        Note "$name : $m | $(S)"
        if ($m.StartsWith("at ($r,$c)")) { return $true }
    }
    return $false
}

function SaveAt([int]$r, [int]$c) {
    if (Goal "save($r,$c)" $r $c) { Key 'e'; Note "saved resp=$((GD).respawnPointId.value) hearts=$((GD).hearts.value)" }
}

function HealIfNeeded([int]$r, [int]$c, [int]$under = 3) {
    if ((GD).hearts.value -le $under) { $null = Goal "potion($r,$c)" $r $c }
}
