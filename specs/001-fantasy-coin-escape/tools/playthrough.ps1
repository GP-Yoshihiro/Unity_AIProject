# 通しプレイ: Q3 → Q13（-SkipQ13 で省略） → Q11 / Q12
param([switch]$SkipQ13)
. "$PSScriptRoot\ce.ps1"
. "$PSScriptRoot\nav.ps1"
. "$PSScriptRoot\combat.ps1"
Set-Content $script:PlayLog ''

function Run {
    if (-not (Goal 'coin' 26 2)) { return }
    HealIfNeeded 23 8
    SaveAt 26 15
    if (-not (Goal 'coin' 26 34)) { return }
    if (-not (Goal 'coin' 16 34)) { return }
    SaveAt 17 15
    if (-not (Goal 'coin' 16 2)) { return }
    if (-not (Goal 'door@4' 12 18)) { return }
    Note "Q3a coins=$((GD).coinCount.value) door=$((GD).bossDoorOpen.value)"
    Note (Shot '07_door_closed_4coins')
    SaveAt 12 15
    if (-not (Goal 'coin' 4 3)) { return }
    Note "Q3b $(S)"
    HealIfNeeded 5 32 2
    HealIfNeeded 12 23 4
    SaveAt 12 15
    if (-not (Goal 'door' 12 18)) { return }
    Note (Shot '08_door_open_5coins')

    if (-not $SkipQ13 -and (GD).lives.value -ge 2) {
        if (-not (Goal 'arena' 9 18)) { return }
        Note "Q13 before: $(S)"
        BossFight -StopAtHp 18
        for ($i = 0; $i -lt 60 -and (GD).flow.value -eq 'Playing'; $i++) { Start-Sleep -Milliseconds 700 }
        Note "Q13 fallen: $(S)"
        $null = WaitFlow 'Playing' 8
        Note "Q13 respawn: $(S)"
    }

    for ($n = 0; $n -lt 3; $n++) {
        if ((GD).flow.value -eq 'Dying') { $null = WaitFlow 'Playing' 5 }
        if ((GD).flow.value -ne 'Playing') { return }
        $null = Goal 'arena' 9 18
        BossFight
        if ((GD).bossDefeated.value) { break }
        Start-Sleep 2
    }
    if (-not (GD).bossDefeated.value) { return }
    Note "Q12 defeated: $(S)"
    Note (Shot '09_boss_defeated')
    $null = GoToCell 3 18
    $null = GoToCell 1 17
    Start-Sleep 1
    $g = GD
    Note "Q12 clear: $(S) splits=$($g.splitsText.value) best=$($g.bestTime.value)"
    Note (Shot '10_clear')
}

Note (StartPlay)
Key 'enter'
Note "playing=$(WaitFlow 'Playing')"
Run
Note "final $(S)"
Note (Held)
Note (StopPlay)
