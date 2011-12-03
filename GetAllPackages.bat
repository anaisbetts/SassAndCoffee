@ECHO OFF

SET NUGET="%~dp0\NuGet.exe"

FOR /R "%~dp0" %%C IN (*packages.config) DO (
	%NUGET% install "%%~fC" -o "%~dp0packages"
)
