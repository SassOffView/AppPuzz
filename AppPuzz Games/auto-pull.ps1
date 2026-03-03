# Auto-pull script - avvialo una volta sola, gira in background
# Controlla ogni 15 secondi se ci sono nuovi commit sul branch remoto e fa il pull automaticamente

$branch = "claude/digital-game-development-qTQf4"
$interval = 15  # secondi tra ogni controllo

Write-Host "=== Auto-pull avviato ===" -ForegroundColor Green
Write-Host "Branch: $branch" -ForegroundColor Cyan
Write-Host "Controllo ogni $interval secondi. Tieni questa finestra aperta." -ForegroundColor Yellow
Write-Host ""

while ($true) {
    # Scarica info remote senza fare merge
    git fetch origin $branch 2>$null

    # Conta quanti commit ci sono sul remote che non hai in locale
    $behind = git rev-list HEAD..origin/$branch --count 2>$null

    if ($behind -gt 0) {
        Write-Host "[$([datetime]::Now.ToString('HH:mm:ss'))] Trovati $behind nuovi commit - pulling..." -ForegroundColor Yellow
        git pull origin $branch
        Write-Host "[$([datetime]::Now.ToString('HH:mm:ss'))] Pull completato! Unity rileverà le modifiche automaticamente." -ForegroundColor Green
        Write-Host ""
    }

    Start-Sleep -Seconds $interval
}
