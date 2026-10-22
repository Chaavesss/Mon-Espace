param([string]$ProjectRoot = (Split-Path $PSScriptRoot -Parent))
$ErrorActionPreference = 'Stop'
$files = @('build.ps1','standalone/dofus/GanymedeReader.cs','standalone/dofus/QuestInstructions.cs')
foreach ($dir in @('src','assets','scripts')) { Get-ChildItem -LiteralPath (Join-Path $ProjectRoot $dir) -File -Recurse | ForEach-Object { $files += $_.FullName.Substring($ProjectRoot.Length + 1).Replace('\','/') } }
$lines = foreach ($name in ($files | Sort-Object -CaseSensitive)) { $name + ':' + (Get-FileHash -LiteralPath (Join-Path $ProjectRoot $name) -Algorithm SHA256).Hash.ToLowerInvariant() }
$bytes = [Text.Encoding]::UTF8.GetBytes(($lines -join [char]10))
$sha = [Security.Cryptography.SHA256]::Create()
try { ([BitConverter]::ToString($sha.ComputeHash($bytes))).Replace('-','').ToLowerInvariant() } finally { $sha.Dispose() }
