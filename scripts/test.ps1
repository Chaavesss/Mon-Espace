param([string]$ProjectRoot = (Split-Path $PSScriptRoot -Parent))
$ErrorActionPreference = 'Stop'
$exe = Join-Path $ProjectRoot 'release\MonEspace.exe'
$testRoot = Join-Path $ProjectRoot ('test-results\' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testRoot -Force | Out-Null
foreach ($suite in @('self-test','ui-self-test','board-self-test','finance-self-test','music-self-test','widget-self-test','personal-self-test','focus-self-test','capture-self-test','templates-self-test','design-self-test','next-self-test')) {
 $folder = Join-Path $testRoot $suite
 $info = New-Object Diagnostics.ProcessStartInfo
 $info.FileName = $exe
 $info.Arguments = '--' + $suite + ' "' + $folder + '"'
 $info.UseShellExecute = $false
 $info.CreateNoWindow = $true
 $p = [Diagnostics.Process]::Start($info)
 if (!$p.WaitForExit(120000)) { $p.Kill(); throw ('Delai depasse : ' + $suite) }
 if ($p.ExitCode -ne 0) { Get-WinEvent -FilterHashtable @{LogName='Application';StartTime=(Get-Date).AddMinutes(-5)} -ErrorAction SilentlyContinue | Where-Object { $_.Message -match 'MonEspace.exe' } | Select-Object -First 4 -ExpandProperty Message | Write-Output; Get-ChildItem -LiteralPath $folder -Recurse -File | Select-Object FullName,Length | Out-String | Write-Output; Get-ChildItem -LiteralPath $folder -Filter '*.txt' | ForEach-Object { Get-Content -LiteralPath $_.FullName -Encoding UTF8 | Write-Output }; throw ('Echec : ' + $suite + ', code ' + $p.ExitCode + '. Consulter ' + $folder) }
 Write-Output ('OK ' + $suite)
}
