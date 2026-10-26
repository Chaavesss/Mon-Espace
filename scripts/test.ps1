param([string]$ProjectRoot = (Split-Path $PSScriptRoot -Parent), [switch]$SkipNativePdf)
$ErrorActionPreference = 'Stop'
$exe = Join-Path $ProjectRoot 'release\MonEspace.exe'
$testRoot = Join-Path $ProjectRoot ('test-results\' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testRoot -Force | Out-Null
foreach ($suite in @('self-test','ui-self-test','board-self-test','finance-self-test','music-self-test','widget-self-test','personal-self-test','focus-self-test','capture-self-test','templates-self-test','design-self-test','next-self-test','sport-self-test')) {
 $folder = Join-Path $testRoot $suite
 $info = New-Object Diagnostics.ProcessStartInfo
 $info.FileName = $exe
 $testFlag = $suite
 if ($SkipNativePdf -and $suite -eq 'next-self-test') { $testFlag = 'next-self-test-no-pdf'; Write-Warning 'PDF natif exclu sur Windows Server : verifier le test complet sur Windows 10/11 avant distribution.' }
 $info.Arguments = '--' + $testFlag + ' "' + $folder + '"'
 $info.UseShellExecute = $false
 $info.CreateNoWindow = $true
 $p = [Diagnostics.Process]::Start($info)
 if (!$p.WaitForExit(120000)) { $p.Kill(); throw ('Delai depasse : ' + $suite) }
 if ($p.ExitCode -ne 0) { Get-WinEvent -FilterHashtable @{LogName='Application';StartTime=(Get-Date).AddMinutes(-5)} -ErrorAction SilentlyContinue | Where-Object { $_.Message -match 'MonEspace.exe' } | Select-Object -First 4 -ExpandProperty Message | Write-Output; Get-ChildItem -LiteralPath $folder -Recurse -File | Select-Object FullName,Length | Out-String | Write-Output; Get-ChildItem -LiteralPath $folder -Filter '*.txt' | ForEach-Object { Get-Content -LiteralPath $_.FullName -Encoding UTF8 | Write-Output }; throw ('Echec : ' + $suite + ', code ' + $p.ExitCode + '. Consulter ' + $folder) }
 Write-Output ('OK ' + $suite)
}

if (!$SkipNativePdf) {
 $fingerprint = & (Join-Path $ProjectRoot 'scripts/source-fingerprint.ps1') -ProjectRoot $ProjectRoot
 $validation = [ordered]@{sourceSha256=$fingerprint;version=[Reflection.AssemblyName]::GetAssemblyName($exe).Version.ToString();nativePdf=$true;suites=13;verifiedAt=[DateTime]::UtcNow.ToString('o')}
 [IO.File]::WriteAllText((Join-Path $ProjectRoot 'desktop-validation.json'), ($validation | ConvertTo-Json), (New-Object Text.UTF8Encoding($false)))
 Write-Output 'Validation desktop complete enregistree.'
}

else {
 $fingerprint = & (Join-Path $ProjectRoot 'scripts/source-fingerprint.ps1') -ProjectRoot $ProjectRoot
 $validation = [ordered]@{sourceSha256=$fingerprint;version=[Reflection.AssemblyName]::GetAssemblyName($exe).Version.ToString();nativePdf=$false;suites=13;scope='windows-server-native-pdf-excluded';verifiedAt=[DateTime]::UtcNow.ToString('o')}
 [IO.File]::WriteAllText((Join-Path $ProjectRoot 'ci-validation.json'), ($validation | ConvertTo-Json), (New-Object Text.UTF8Encoding($false)))
 Write-Warning 'Validation CI enregistree : les 13 suites sont passees, le moteur PDF natif reste non reteste sur cette version.'
}
