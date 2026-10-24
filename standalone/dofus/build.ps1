param([string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path)
$ErrorActionPreference = 'Stop'
$out = Join-Path $ProjectRoot 'dist-compagnon'
New-Item -ItemType Directory -Force -Path $out | Out-Null
& (Join-Path $PSScriptRoot 'restore-portraits.ps1') -ProjectRoot $ProjectRoot -Out $out
$gac = Join-Path $env:WINDIR 'Microsoft.NET/assembly'
$refs = @('System.dll','System.Core.dll','System.Windows.Forms.dll','System.Drawing.dll','System.Web.Extensions.dll','System.Numerics.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll')
foreach ($name in @('WindowsBase','PresentationCore','PresentationFramework','System.Xaml','System.Net.Http')) {
    $found = Get-ChildItem $gac -Filter ($name + '.dll') -Recurse | Select-Object -First 1 -ExpandProperty FullName
    if (!$found) { throw "Assembly missing: $name" }
    $refs += $found
}
$refs += (Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/System.Runtime.WindowsRuntime.dll')
$runtime = Get-ChildItem (Join-Path $gac 'GAC_MSIL/System.Runtime') -Filter 'System.Runtime.dll' -Recurse | Select-Object -First 1 -ExpandProperty FullName
$refs += $runtime
foreach ($name in @('Windows.Media','Windows.Foundation','Windows.Globalization','Windows.Graphics','Windows.Storage')) { $refs += Join-Path $env:WINDIR ('System32/WinMetadata/' + $name + '.winmd') }
$resources = Join-Path $out 'CompagnonDofus.g.resources'
$writer = New-Object System.Resources.ResourceWriter($resources)
try {
 foreach($asset in Get-ChildItem (Join-Path $ProjectRoot 'assets/dofus') -Filter '*.png') { $writer.AddResource(('assets/dofus/' + $asset.Name.ToLowerInvariant()), [IO.File]::OpenRead($asset.FullName), $true) }
 $writer.AddResource('assets/fonts/fontbleach.ttf', [IO.File]::OpenRead((Join-Path $ProjectRoot 'assets/fonts/FontBleach.ttf')), $true)
 $writer.Generate()
} finally { $writer.Dispose() }
$argsList = @('/nologo','/target:winexe','/platform:x64','/optimize+','/codepage:65001','/utf8output',('/out:' + (Join-Path $out 'CompagnonDofus.exe')),('/win32icon:' + (Join-Path $ProjectRoot 'assets/MonEspace.ico')),('/resource:' + (Join-Path $ProjectRoot 'assets/MonEspace.ico') + ',MonEspace.Icon'))
$argsList += '/resource:' + $resources + ',CompagnonDofus.g.resources'
$catalogue = Join-Path $out 'catalogue-bootstrap.json'
if(Test-Path $catalogue) { $argsList += '/resource:' + $catalogue + ',Compagnon.Catalogue' }
$refs += @('SkiaSharp.dll','System.Memory.dll','System.Buffers.dll','System.Runtime.CompilerServices.Unsafe.dll','System.Numerics.Vectors.dll') | ForEach-Object { Join-Path $out $_ }
$argsList += $refs | ForEach-Object { '/reference:' + $_ }
$argsList += @('DofusCompanion.cs','DofusTools.cs','DofusToolsUi.cs','QuestCaptureImport.cs','QuestCaptureOcr.cs','QuestCaptureUi.cs','QuestCaptureAcceptance.cs','TougliReader.cs','AppIdentity.cs') | ForEach-Object { Join-Path $ProjectRoot ('src/' + $_) }
$argsList += Get-ChildItem (Join-Path $ProjectRoot 'standalone/dofus') -Filter '*.cs' | ForEach-Object FullName
$rsp = Join-Path $out 'compilation.rsp'
[IO.File]::WriteAllLines($rsp,[string[]]($argsList | ForEach-Object { '"' + $_ + '"' }), (New-Object Text.UTF8Encoding($false)))
& (Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe') ('@' + $rsp)
if ($LASTEXITCODE -ne 0) { throw 'Standalone compilation failed' }
Write-Output 'CompagnonDofus.exe compiled independently (no MonEspace.exe).'

