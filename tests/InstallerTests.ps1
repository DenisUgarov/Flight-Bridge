$ErrorActionPreference = 'Stop'
$project = Split-Path $PSScriptRoot -Parent
$installer = [Reflection.Assembly]::LoadFile((Join-Path $project 'dist\FlightBridge-Setup.exe'))
$setupType = $installer.GetType('Setup', $true)
$flags = [Reflection.BindingFlags]'NonPublic,Static'
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ('FlightBridge-installer-tests-' + [Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($testRoot) | Out-Null
$setupType.GetField('Folder', $flags).SetValue($null, $testRoot)
$writePayload = $setupType.GetMethod('WritePayload', $flags)
foreach ($name in @('FlightBridge.exe', 'README.md')) {
 $writePayload.Invoke($null, @($name)) | Out-Null
 $expected = if ($name -eq 'FlightBridge.exe') { Join-Path $project "dist\$name" } else { Join-Path $project $name }
 $extracted = Join-Path $testRoot $name
 if ((Get-FileHash -LiteralPath $extracted).Hash -ne (Get-FileHash -LiteralPath $expected).Hash) { throw "Installer payload mismatch: $name" }
 Write-Output "PASS: installer embeds current $name"
 [IO.File]::WriteAllText($extracted, 'old version')
 $writePayload.Invoke($null, @($name)) | Out-Null
 if ((Get-FileHash -LiteralPath $extracted).Hash -ne (Get-FileHash -LiteralPath $expected).Hash) { throw "Installer update mismatch: $name" }
 if (Test-Path -LiteralPath ($extracted + '.new')) { throw 'Unfinished update file remains' }
 Write-Output "PASS: installer replaces existing $name completely"
}
$localization = $installer.GetType('SetupLocalization', $true)
$all = $localization.GetField('All').GetValue($null)
if ($all.Count -ne 13) { throw 'Expected 13 installer languages' }
$keys = @($all[0].Strings.Keys | Sort-Object)
foreach ($language in $all) {
 if (Compare-Object $keys @($language.Strings.Keys | Sort-Object)) { throw "Missing translation keys: $($language.Code)" }
 foreach ($key in $keys) { if ([string]::IsNullOrWhiteSpace($language.Strings[$key])) { throw "Empty translation: $($language.Code)/$key" } }
 $readme = Get-Content -LiteralPath (Join-Path $project "docs/README.$($language.Code).md") -Raw
 if (-not $readme.Contains($language.Strings['Guide'].Replace("`r`n","`n"))) { throw "Guide and README differ: $($language.Code)" }
 Write-Output "PASS: complete installer and README translation: $($language.Code)"
}
$resolve = $localization.GetMethod('Resolve')
foreach ($pair in @(@('en-US','en'),@('ru-RU','ru'),@('zh-CN','zh'),@('pt-BR','pt'),@('de-AT','de'),@('xx-XX','en'))) {
 if ($resolve.Invoke($null,@($pair[0])).Code -ne $pair[1]) { throw 'Language fallback failed' }
}
Write-Output 'PASS: regional language selection and English fallback'
Write-Output '4 installer payload/update checks + 13 translation checks + language fallback passed. No app installed or registry changed.'
