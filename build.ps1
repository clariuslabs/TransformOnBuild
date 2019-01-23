#requires -version 3.0

[CmdletBinding()]
param
(
    [Parameter(Mandatory=$true)]
    [string] $PackageVersion,

    [switch] $AutoPush
)

$script:ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
trap { throw $Error[0] }

function Main
{
    . "$PSScriptRoot\Exec.ps1"

    CompileSolution
    DownloadNuGet
    PackNuGetPackage
    PublishNuGetPackage
}

function CompileSolution
{
    $msbuildPath = 'c:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe'
    $solutionPath = "$PSScriptRoot\Clarius.TransformOnBuild.MSBuild.Task\Clarius.TransformOnBuild.MSBuild.Task.sln"

    exec { & $msbuildPath $solutionPath }
}

function DownloadNuGet
{
    $nugetPath = "$PSScriptRoot\.nuget\nuget.exe"
    $nuGetExeUrl = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"

    if (-not (Test-Path -Path $nugetPath))
    {
        New-Item -Path ($nugetPath | Split-Path) -ItemType Directory -Force
        Invoke-WebRequest -Uri $nuGetExeUrl -OutFile $nugetPath
    }
}

function PackNuGetPackage
{
    $nugetPath = "$PSScriptRoot\.nuget\nuget.exe"
    $nuspecPath = "$PSScriptRoot\nuget\Clarius.TransformOnBuild.nuspec"
    $dropPath = "$PSScriptRoot\drop"

    exec { & $nugetPath pack -NoPackageAnalysis $nuspecPath -Version $PackageVersion -OutputDirectory $dropPath }
}

function PublishNuGetPackage
{
    if (-not $AutoPush)
    {
        return
    }

    $nugetPath = "$PSScriptRoot\.nuget\nuget.exe"
    $dropPath = "$PSScriptRoot\drop"
    $nugetRepositoryUrl = 'https://www.nuget.org/api/v2/package'

    exec { & $nugetPath push $dropPath -Source $nugetRepositoryUrl }
}

Main