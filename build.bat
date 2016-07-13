@echo On
set config=%1
if "%config%" == "" (
   set config=Release
)

set version=-Version 1.0.0
if not "%PackageVersion%" == "" (
   set version=-Version %PackageVersion%
)

REM Restore packages
"%nuget%" restore "Source\PackStream.NET.sln"
if not "%errorlevel%"=="0" goto failure

REM Build
"%MsBuildExe%" "Source\PackStream.NET.sln" /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false
if not "%errorlevel%"=="0" goto failure

REM Unit tests
"%nuget%" install NUnit.Runners -Version 2.6.4 -OutputDirectory packages
if not "%errorlevel%"=="0" goto failure

packages\NUnit.Runners.2.6.4\tools\nunit-console.exe /config:%config% /framework:net-4.5 "Source\PackStream.NET.Tests\bin\%config%\PackStream.NET.Tests.dll"
if not "%errorlevel%"=="0" goto failure

REM Package
mkdir Artifacts
"%nuget%" pack "PackStream.NET.nuspec" -o Artifacts -p Configuration=%config% %version%
if not "%errorlevel%"=="0" goto failure

:success
exit 0

:failure
exit -1