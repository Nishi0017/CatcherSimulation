param(
    [string]$UnityEditor = 'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor',
    [string]$DotnetRoot = 'C:/Program Files/dotnet'
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path (Split-Path $PSScriptRoot)
$output = Join-Path $projectRoot 'Temp/PhysicsValidation'
New-Item -ItemType Directory -Path $output -Force | Out-Null
$sdk = Get-ChildItem "$DotnetRoot/sdk" -Directory | Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1
$pack = Get-ChildItem "$DotnetRoot/packs/Microsoft.NETCore.App.Ref" -Directory | Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1
$framework = Get-ChildItem "$($pack.FullName)/ref" -Directory | Select-Object -First 1
$core = Join-Path $UnityEditor 'Data/Managed/UnityEngine/UnityEngine.CoreModule.dll'
$source = Get-Content (Join-Path $projectRoot 'Assets/Scripts/BallManager.Physics.cs') -Raw
$solver = 'using UnityEngine;' + [Environment]::NewLine + $source.Substring($source.IndexOf('public static class BaseballTrajectory'))
Set-Content (Join-Path $output 'Solver.cs') $solver -Encoding utf8
$response = @('-nologo', '-target:exe', "-out:`"$output/TrajectoryChecks.dll`"", "-r:`"$core`"",
    "`"$output/Solver.cs`"", "`"$PSScriptRoot/TrajectoryChecks.cs`"")
$response += Get-ChildItem "$($framework.FullName)/*.dll" | ForEach-Object { '-r:"' + $_.FullName + '"' }
Set-Content (Join-Path $output 'solver-tests.rsp') $response
& "$DotnetRoot/dotnet.exe" "$($sdk.FullName)/Roslyn/bincore/csc.dll" "@$output/solver-tests.rsp"
if ($LASTEXITCODE -ne 0) { throw 'Trajectory checks did not compile.' }
Copy-Item $core $output -Force
$runtime = @{ runtimeOptions = @{ tfm = $framework.Name; framework = @{ name = 'Microsoft.NETCore.App'; version = $pack.Name } } }
$runtime | ConvertTo-Json -Depth 4 | Set-Content (Join-Path $output 'TrajectoryChecks.runtimeconfig.json')
& "$DotnetRoot/dotnet.exe" "$output/TrajectoryChecks.dll"
if ($LASTEXITCODE -ne 0) { throw 'Trajectory checks failed.' }
