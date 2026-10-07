$ErrorActionPreference = "Stop"

$pythonLauncher = Get-Command python -ErrorAction SilentlyContinue
if ($pythonLauncher) {
    $pythonCommand = $pythonLauncher.Source
    $pythonArgs = @("-m")
} else {
    $pythonCommand = (Get-Command py -ErrorAction Stop).Source
    $pythonArgs = @("-3", "-m")
}

Push-Location $PSScriptRoot
try {
    & $pythonCommand @pythonArgs pip install -r requirements-build.txt
    if ($LASTEXITCODE -ne 0) { throw "Falha ao instalar as dependências de build." }

    & $pythonCommand @pythonArgs PyInstaller --noconfirm --clean --onefile --name executar_ia `
        --add-data "IA.joblib;." --add-data "dados_psa_clinica.csv;." executar_ia.py
    if ($LASTEXITCODE -ne 0) { throw "PyInstaller não conseguiu gerar o executável." }

    Write-Host "Executável criado em: $PSScriptRoot\dist\executar_ia.exe"
} finally {
    Pop-Location
}
