param([string]$ProjectRoot = (Split-Path $PSScriptRoot -Parent))
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$assetRoot = Join-Path $ProjectRoot 'assets/exercises'
$photoRoot = Join-Path $assetRoot 'photos'
$manifestFile = Join-Path $assetRoot 'photos-manifest.json'
$manifest = [IO.File]::ReadAllText($manifestFile) | ConvertFrom-Json
if ($manifest.Count -ne 1746 -or @($manifest.path | Select-Object -Unique).Count -ne 1746) { throw 'Manifeste de photos incomplet ou duplique.' }
foreach ($item in $manifest) { if ($item.path -notmatch '^[A-Za-z0-9_().-]+/[01]\.jpg$' -or $item.sha256 -notmatch '^[a-f0-9]{64}$') { throw 'Entree de manifeste invalide.' } }
$pin = 'f00c92c7dcf1216a928a52c3706c7ce8e2f71ed5'
$releaseRoot = Join-Path $ProjectRoot 'release'
New-Item -ItemType Directory -Path $releaseRoot -Force | Out-Null
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
# Only manifest-listed paths are used. Every imported file must match its recorded SHA-256.
$missing = @($manifest | Where-Object { !(Test-Path -LiteralPath (Join-Path $photoRoot $_.path)) })
if ($missing.Count -gt 0) {
 $archivePath = Join-Path $releaseRoot 'exercise-source.zip'
 if (!(Test-Path -LiteralPath $archivePath)) {
  [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
  Write-Output 'Telechargement de la revision source des photos...'
  Invoke-WebRequest -UseBasicParsing -TimeoutSec 180 -Uri ('https://codeload.github.com/yuhonas/free-exercise-db/zip/' + $pin) -OutFile ($archivePath + '.tmp')
  Move-Item -LiteralPath ($archivePath + '.tmp') -Destination $archivePath -Force
 }
 $sourceArchive = [IO.Compression.ZipFile]::OpenRead($archivePath)
 try {
  foreach ($item in $missing) {
   if ($item.path -notmatch '^[A-Za-z0-9_().-]+/[01]\.jpg$') { throw ('Chemin de photo invalide : ' + $item.path) }
   $entry = $sourceArchive.GetEntry('free-exercise-db-' + $pin + '/exercises/' + $item.path)
   if (!$entry) { throw ('Photo source manquante : ' + $item.path) }
   $target = Join-Path $photoRoot $item.path
   New-Item -ItemType Directory -Path (Split-Path $target -Parent) -Force | Out-Null
   [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $target, $true)
  }
 } finally { $sourceArchive.Dispose() }
}
foreach ($item in $manifest) {
 $file = Join-Path $photoRoot $item.path
 if ((Get-Item -LiteralPath $file).Length -ne $item.bytes -or (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant() -ne $item.sha256) { throw ('Photo modifiee ou incomplete : ' + $item.path) }
}
# Fixed entry order and timestamp keep the embedded archive reproducible.
$bundle = Join-Path $releaseRoot 'exercise-photos.zip'
if (Test-Path -LiteralPath ($bundle + '.tmp')) { Remove-Item -LiteralPath ($bundle + '.tmp') }
$zip = [IO.Compression.ZipFile]::Open($bundle + '.tmp', [IO.Compression.ZipArchiveMode]::Create)
try {
 foreach ($item in ($manifest | Sort-Object path -CaseSensitive)) {
  $entry = $zip.CreateEntry($item.path, [IO.Compression.CompressionLevel]::NoCompression)
  $entry.LastWriteTime = [DateTimeOffset]::new(2000,1,1,0,0,0,[TimeSpan]::Zero)
  $inputStream = [IO.File]::OpenRead((Join-Path $photoRoot $item.path))
  $outputStream = $entry.Open()
  try { $inputStream.CopyTo($outputStream) } finally { $outputStream.Dispose(); $inputStream.Dispose() }
 }
} finally { $zip.Dispose() }
Move-Item -LiteralPath ($bundle + '.tmp') -Destination $bundle -Force
Write-Output ('Photos verifiees et embarquees : ' + $manifest.Count)
