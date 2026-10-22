param([string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path)
$ErrorActionPreference = 'Stop'
$out = Join-Path $ProjectRoot 'dist-compagnon'
New-Item -ItemType Directory -Force -Path $out | Out-Null
$gac = Join-Path $env:WINDIR 'Microsoft.NET/assembly'
$refs = @('System.dll','System.Core.dll','System.Windows.Forms.dll','System.Drawing.dll','System.Web.Extensions.dll')
foreach ($name in @('WindowsBase','PresentationCore','PresentationFramework','System.Xaml','System.Net.Http')) {
    $found = Get-ChildItem $gac -Filter ($name + '.dll') -Recurse | Select-Object -First 1 -ExpandProperty FullName
    if (!$found) { throw "Assembly missing: $name" }
    $refs += $found
}
$argsList = @('/nologo','/target:winexe','/platform:x64','/optimize+','/codepage:65001','/utf8output',('/out:' + (Join-Path $out 'CompagnonDofus.exe')),('/win32icon:' + (Join-Path $ProjectRoot 'assets/MonEspace.ico')),('/resource:' + (Join-Path $ProjectRoot 'assets/MonEspace.ico') + ',MonEspace.Icon'))
$argsList += $refs | ForEach-Object { '/reference:' + $_ }
$argsList += @('DofusCompanion.cs','TougliReader.cs','AppIdentity.cs') | ForEach-Object { Join-Path $ProjectRoot ('src/' + $_) }
$argsList += Get-ChildItem (Join-Path $ProjectRoot 'standalone/dofus') -Filter '*.cs' | ForEach-Object FullName
$rsp = Join-Path $out 'compilation.rsp'
[IO.File]::WriteAllLines($rsp,[string[]]($argsList | ForEach-Object { '"' + $_ + '"' }), (New-Object Text.UTF8Encoding($false)))
& (Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe') ('@' + $rsp)
if ($LASTEXITCODE -ne 0) { throw 'Standalone compilation failed' }
Write-Output 'CompagnonDofus.exe compiled independently (no MonEspace.exe).'
