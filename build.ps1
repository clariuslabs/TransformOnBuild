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

& 'C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe' "$PSScriptRoot\build.proj" /verbosity:normal /nr:false /p:PackageVersion=$PackageVersion /p:AutoPush=$($AutoPush.ToString().ToLower())