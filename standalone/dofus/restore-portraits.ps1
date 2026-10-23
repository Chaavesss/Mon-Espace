param([string]$ProjectRoot,[string]$Out)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
[Net.ServicePointManager]::SecurityProtocol=[Net.SecurityProtocolType]::Tls12
$cache=Join-Path $ProjectRoot 'dist-compagnon/packages'
New-Item -ItemType Directory -Force $cache | Out-Null
$notices=New-Object Text.StringBuilder
foreach($item in (Get-Content (Join-Path $PSScriptRoot 'portrait-packages.json') -Raw | ConvertFrom-Json)) {
 $zip=Join-Path $cache ($item.id+'.zip')
 if(!(Test-Path $zip) -or (Get-FileHash $zip -Algorithm SHA256).Hash.ToLowerInvariant() -ne $item.sha256) {
  Invoke-WebRequest -Uri ('https://api.nuget.org/v3-flatcontainer/'+$item.id+'/'+$item.version+'/'+$item.id+'.'+$item.version+'.nupkg') -UseBasicParsing -OutFile $zip
 }
 if((Get-FileHash $zip -Algorithm SHA256).Hash.ToLowerInvariant() -ne $item.sha256) { throw ('Package hash mismatch: '+$item.id) }
 $archive=[IO.Compression.ZipFile]::OpenRead($zip)
 try {
  $entry=$archive.GetEntry($item.entry)
  if(!$entry) { throw ('Missing library: '+$item.entry) }
  [IO.Compression.ZipFileExtensions]::ExtractToFile($entry,(Join-Path $Out $item.file),$true)
  [void]$notices.AppendLine(($item.id+' '+$item.version+' - https://www.nuget.org/packages/'+$item.id+'/'+$item.version))
  foreach($name in @('LICENSE.txt','LICENSE.TXT','THIRD-PARTY-NOTICES.TXT','THIRD-PARTY-NOTICES.txt')) {
   $entry=$archive.GetEntry($name)
   if($entry) { $reader=New-Object IO.StreamReader($entry.Open());try{[void]$notices.AppendLine($reader.ReadToEnd())}finally{$reader.Dispose()} }
  }
 }finally{$archive.Dispose()}
}
[IO.File]::WriteAllText((Join-Path $Out 'THIRD_PARTY_NOTICES.txt'),$notices.ToString(),(New-Object Text.UTF8Encoding($false)))
