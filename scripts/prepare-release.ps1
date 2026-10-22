param([string]$Repository = 'Chaavesss/Mon-Espace', [string]$ProjectRoot = (Split-Path $PSScriptRoot -Parent), [switch]$SkipBuild)
$ErrorActionPreference = 'Stop'
if ($Repository -notmatch '^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$') { throw 'Depot GitHub invalide : utiliser compte/depot.' }
if (!$SkipBuild) { & (Join-Path $ProjectRoot 'build.ps1') -ProjectRoot $ProjectRoot }
$exe = Join-Path $ProjectRoot 'release\MonEspace.exe'
$version = [Reflection.AssemblyName]::GetAssemblyName($exe).Version.ToString()
$tag = 'v' + $version
$out = Join-Path $ProjectRoot 'dist'
New-Item -ItemType Directory -Path $out -Force | Out-Null
$stage = Join-Path $out ('package-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $stage | Out-Null
try {
 Copy-Item -LiteralPath $exe -Destination (Join-Path $stage 'MonEspace.exe')
 $cmd = @('@echo off', 'start "" "%~dp0MonEspace.exe" --portable', '') -join [Environment]::NewLine
 [IO.File]::WriteAllText((Join-Path $stage 'Mode_portable.cmd'), $cmd, [Text.Encoding]::ASCII)
 Copy-Item -LiteralPath (Join-Path $ProjectRoot 'docs\INSTALLATION.txt') -Destination (Join-Path $stage 'LIRE_MOI.txt')
 $zip = Join-Path $out 'MonEspace.zip'
 if (Test-Path -LiteralPath $zip) { Remove-Item -LiteralPath $zip }
 Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zip -CompressionLevel Optimal
 $hash = (Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash.ToLowerInvariant()
 $notes = [IO.File]::ReadAllText((Join-Path $ProjectRoot 'RELEASE_NOTES.md'))
 if ($notes.Length -gt 10000) { throw 'Notes de version trop longues.' }
 $manifest = [ordered]@{version=$version; downloadUrl=('https://github.com/' + $Repository + '/releases/download/' + $tag + '/MonEspace.zip'); sha256=$hash; notes=$notes}
 [IO.File]::WriteAllText((Join-Path $out 'version.json'), ($manifest | ConvertTo-Json), (New-Object Text.UTF8Encoding($false)))
 [IO.File]::WriteAllText((Join-Path $out 'tag.txt'), $tag, (New-Object Text.UTF8Encoding($false)))
 [IO.File]::WriteAllText((Join-Path $out 'RELEASE_NOTES.md'), $notes, (New-Object Text.UTF8Encoding($false)))
 Add-Type -AssemblyName System.IO.Compression.FileSystem
 $archive = [IO.Compression.ZipFile]::OpenRead($zip)
 try { if (($archive.Entries | Where-Object FullName -eq 'MonEspace.exe').Count -ne 1 -or $archive.Entries.Count -ne 3) { throw 'Contenu du paquet inattendu.' } } finally { $archive.Dispose() }
 Write-Output ('Paquet valide : ' + $zip + ' / ' + $version + ' / SHA256 ' + $hash)
} finally {
 $resolvedStage = [IO.Path]::GetFullPath($stage)
 $resolvedOut = [IO.Path]::GetFullPath($out) + [IO.Path]::DirectorySeparatorChar
 if ($resolvedStage.StartsWith($resolvedOut, [StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $resolvedStage)) { Remove-Item -LiteralPath $resolvedStage -Recurse -Force }
}
