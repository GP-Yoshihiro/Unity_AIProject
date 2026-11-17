# Navigation helpers for CryptEscape play tests (dot-source after ce.ps1)
$script:Layout = Get-Content C:\Users\koyo\Desktop\AI\Assets\Game\Data\DungeonLayout.txt | Where-Object { $_.Length -gt 0 -and -not $_.StartsWith('# ') }
$script:H = $script:Layout.Count
$script:W = ($script:Layout | Measure-Object -Property Length -Maximum).Maximum
$script:Speed = 6.0
$script:Log = New-Object System.Collections.Generic.List[string]

function CellAt([int]$r, [int]$c) {
    if ($r -lt 0 -or $r -ge $script:H -or $c -lt 0 -or $c -ge $script:Layout[$r].Length) { return '#' }
    return $script:Layout[$r][$c]
}
function CellPos([int]$r, [int]$c) { @{ x = $c * 2.0; z = ($script:H - 1 - $r) * 2.0 } }
function PosCell($p) { @{ r = [int][math]::Round($script:H - 1 - $p.z / 2.0); c = [int][math]::Round($p.x / 2.0) } }

function PlayerPos {
    (UC get_component_values '{"gameObjectName":"Player","componentType":"Transform"}').data.properties.position.value
}

function Bfs($from, $to, [string]$blocked = '#') {
    $prev = @{}
    $q = New-Object System.Collections.Generic.Queue[object]
    $startKey = "$($from.r),$($from.c)"
    $prev[$startKey] = $null
    $q.Enqueue($from)
    while ($q.Count -gt 0) {
        $cur = $q.Dequeue()
        if ($cur.r -eq $to.r -and $cur.c -eq $to.c) { break }
        foreach ($d in @(@(1, 0), @(-1, 0), @(0, 1), @(0, -1))) {
            $nr = $cur.r + $d[0]; $nc = $cur.c + $d[1]
            $k = "$nr,$nc"
            if ($prev.ContainsKey($k)) { continue }
            $ch = CellAt $nr $nc
            if ($blocked.Contains([string]$ch)) { continue }
            $prev[$k] = $cur
            $q.Enqueue(@{ r = $nr; c = $nc })
        }
    }
    $path = New-Object System.Collections.Generic.List[object]
    $k = "$($to.r),$($to.c)"
    if (-not $prev.ContainsKey($k)) { return $null }
    $node = @{ r = $to.r; c = $to.c }
    while ($node -ne $null) {
        $path.Insert(0, $node)
        $node = $prev["$($node.r),$($node.c)"]
    }
    return $path
}

function Segments($path) {
    # compress cell path into straight segments (list of corner cells)
    $corners = New-Object System.Collections.Generic.List[object]
    for ($i = 1; $i -lt $path.Count; $i++) {
        $isLast = $i -eq $path.Count - 1
        if ($isLast) { $corners.Add($path[$i]); break }
        $d1r = $path[$i].r - $path[$i - 1].r; $d1c = $path[$i].c - $path[$i - 1].c
        $d2r = $path[$i + 1].r - $path[$i].r; $d2c = $path[$i + 1].c - $path[$i].c
        if ($d1r -ne $d2r -or $d1c -ne $d2c) { $corners.Add($path[$i]) }
    }
    return $corners
}

function HoldKey([string]$key, [double]$sec) {
    if ($sec -lt 0.04) { $sec = 0.04 }
    $null = UC input_keyboard (@{ key = $key; action = 'press'; holdSeconds = [math]::Round($sec, 3) } | ConvertTo-Json -Compress)
    Start-Sleep -Milliseconds ([int]($sec * 1000) + 150)
}

function MoveAxis([string]$axis, [double]$target) {
    for ($attempt = 0; $attempt -lt 4; $attempt++) {
        $p = PlayerPos
        $cur = if ($axis -eq 'x') { $p.x } else { $p.z }
        $delta = $target - $cur
        if ([math]::Abs($delta) -lt 0.35) { return $true }
        $key = if ($axis -eq 'x') { if ($delta -gt 0) { 'd' } else { 'a' } } else { if ($delta -gt 0) { 'w' } else { 's' } }
        HoldKey $key ([math]::Abs($delta) / $script:Speed)
        $f = (GD).flow.value
        if ($f -ne 'Playing') { return $false }
    }
    return $true
}

function EnemiesNear([double]$radius) {
    $p = PlayerPos
    $list = @()
    foreach ($root in '/Dungeon/Enemies') {
        $h = (UC get_hierarchy (@{ rootPath = $root; maxDepth = 1; includeTransform = $true } | ConvertTo-Json -Compress)).data.hierarchy
        foreach ($e in $h) {
            if (-not $e.isActive) { continue }
            $dx = $e.transform.position.x - $p.x; $dz = $e.transform.position.z - $p.z
            $d = [math]::Sqrt($dx * $dx + $dz * $dz)
            if ($d -le $radius) { $list += [pscustomobject]@{ name = $e.name; dx = $dx; dz = $dz; d = $d } }
        }
    }
    $list | Sort-Object d
}

function FacingKey($dx, $dz) {
    if ([math]::Abs($dx) -gt [math]::Abs($dz)) { if ($dx -gt 0) { 'd' } else { 'a' } } else { if ($dz -gt 0) { 'w' } else { 's' } }
}

function Swing([int]$times = 3) {
    for ($k = 0; $k -lt $times; $k++) {
        $null = UC input_keyboard '{"key":"j","action":"press","holdSeconds":0.05}'
        Start-Sleep -Milliseconds 240
    }
}

function FightNearby([double]$radius = 6.0, [int]$maxRounds = 10) {
    for ($i = 0; $i -lt $maxRounds; $i++) {
        if ((GD).flow.value -ne 'Playing') { return }
        $e = EnemiesNear $radius | Select-Object -First 1
        if (-not $e) { return }
        if ($e.d -gt 1.6) {
            # 近づく（主軸方向に、敵の手前まで）
            $key = FacingKey $e.dx $e.dz
            $dist = [math]::Max([math]::Abs($e.dx), [math]::Abs($e.dz)) - 1.1
            if ($dist -gt 0.1) { HoldKey $key ($dist / $script:Speed) }
            $e = EnemiesNear $radius | Select-Object -First 1
            if (-not $e) { return }
        }
        HoldKey (FacingKey $e.dx $e.dz) 0.1
        Swing 3
        $script:Log.Add("fight $($e.name) d=$('{0:0.0}' -f $e.d) hearts=$((GD).hearts.value)")
    }
}

function BossInfo {
    $g = GD
    $b = (UC get_component_values '{"gameObjectName":"Boss","componentType":"Transform"}').data.properties.position.value
    $p = PlayerPos
    [pscustomobject]@{ state = $g.bossState.value; hp = $g.bossHp.value; dx = $b.x - $p.x; dz = $b.z - $p.z; d = [math]::Sqrt(($b.x - $p.x) * ($b.x - $p.x) + ($b.z - $p.z) * ($b.z - $p.z)); hearts = $g.hearts.value; flow = $g.flow.value; px = $p.x; pz = $p.z }
}

function BossFight([int]$maxSteps = 120) {
    for ($i = 0; $i -lt $maxSteps; $i++) {
        $b = BossInfo
        if ($b.flow -ne 'Playing' -or $b.state -eq 'Defeated') { return "boss fight end: flow=$($b.flow) state=$($b.state) hp=$($b.hp)" }
        switch ($b.state) {
            'ChargeTell' {
                # 突進の線から横へ逃げる（ボス→自分の向きに垂直な方向へ回避）
                $key = if ([math]::Abs($b.dx) -gt [math]::Abs($b.dz)) { if ($b.pz -lt 43) { 'w' } else { 's' } } else { if ($b.px -lt 36) { 'd' } else { 'a' } }
                $null = UC input_keyboard (@{ key = $key; action = 'press'; holdSeconds = 0.3 } | ConvertTo-Json -Compress)
                Start-Sleep -Milliseconds 80
                $null = UC input_keyboard '{"key":"k","action":"press","holdSeconds":0.05}'
                Start-Sleep -Milliseconds 700
                $script:Log.Add("dodge charge ($key) hearts=$($b.hearts)")
            }
            'AoeTell' {
                if ($b.d -lt 4.6) {
                    $key = FacingKey (-$b.dx) (-$b.dz)
                    HoldKey $key ((4.8 - $b.d) / $script:Speed + 0.1)
                    $script:Log.Add("leave aoe ($key) d=$('{0:0.0}' -f $b.d) hearts=$($b.hearts)")
                }
                else { Start-Sleep -Milliseconds 200 }
            }
            default {
                # Idle / Recover / Charge: 近づいて攻撃
                if ($b.state -eq 'Charge') { Start-Sleep -Milliseconds 150; break }
                if ($b.d -gt 2.6) {
                    $key = FacingKey $b.dx $b.dz
                    HoldKey $key ([math]::Min(0.6, ($b.d - 2.2) / $script:Speed))
                }
                else {
                    HoldKey (FacingKey $b.dx $b.dz) 0.1
                    Swing 2
                    $script:Log.Add("hit boss hp=$($b.hp) state=$($b.state) hearts=$($b.hearts)")
                }
            }
        }
    }
    $b = BossInfo
    return "boss fight timeout: state=$($b.state) hp=$($b.hp) hearts=$($b.hearts)"
}

function GoToCell([int]$tr, [int]$tc, [switch]$Fight) {
    # 1区間ずつ進み、毎回いまの位置から経路を引き直す（戦闘やノックバックでずれても戻れる）
    for ($step = 0; $step -lt 25; $step++) {
        if ($Fight) { FightNearby }
        $flow = (GD).flow.value
        if ($flow -ne 'Playing') { return "stopped: flow=$flow" }

        $p = PlayerPos
        $from = PosCell $p
        if ($from.r -eq $tr -and $from.c -eq $tc) { break }
        # 台座(A)・扉(D)・柵(E)・ボス(B) は当たり判定があるので通らない
        $path = Bfs $from @{ r = $tr; c = $tc } '#ADEB'
        if (-not $path) { $path = Bfs $from @{ r = $tr; c = $tc } '#' }
        if (-not $path) { return "no path from ($($from.r),$($from.c)) to ($tr,$tc)" }

        $corner = (Segments $path)[0]
        $t = CellPos $corner.r $corner.c
        $start = CellPos $from.r $from.c
        if ($corner.r -eq $from.r) {
            $null = MoveAxis 'z' $start.z   # 行の中心にそろえてから横に動く
            $null = MoveAxis 'x' $t.x
        }
        else {
            $null = MoveAxis 'x' $start.x   # 列の中心にそろえてから縦に動く
            $null = MoveAxis 'z' $t.z
        }
    }
    if ($Fight) { FightNearby }
    $p = PlayerPos
    $c2 = PosCell $p
    return "at ($($c2.r),$($c2.c)) target ($tr,$tc)"
}


