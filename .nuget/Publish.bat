@ECHO OFF

REM Project
SET PROJECT=%1
IF NOT EXIST %PROJECT% (
	ECHO Couldn't locate project!
	EXIT /B -1
)
ECHO Using project %PROJECT%

REM Nuget
SET NUGET=%~dp0NuGet.exe
IF NOT EXIST %NUGET% (
	ECHO Couldn't locate NuGet.exe!
	EXIT /B -1
)
ECHO Using NuGet at %NUGET%

REM Check for changes
CALL git rev-parse --verify HEAD >NUL
IF ERRORLEVEL 1 (
	ECHO Not in a git repo?
	EXIT /B -1
)

CALL git update-index -q --ignore-submodules --refresh

CALL git diff-files --quiet --ignore-submodules
IF ERRORLEVEL 1 (
	ECHO Abort: You have unstaged changes.
	EXIT /B -1
)

CALL git diff-index --cached --quiet --ignore-submodules HEAD --
IF ERRORLEVEL 1 (
	ECHO Abort: Your index contains uncommitted changes.
	EXIT /B -1
)

ECHO Everything looks good so far.
CHOICE /M "About to purge. Continue?"
IF ERRORLEVEL 2 (
	ECHO Aborting.
	EXIT /B -1
)

CALL git reset --hard >NUL 2>&1
CALL git clean -x -d -f >NUL 2>&1
ECHO Purge complete.

REM Build Package
%NUGET% pack %1 -Properties "Configuration=Release" -verbose -symbols -build
IF ERRORLEVEL 1 (
	ECHO Unable to create package!  Abort!
	EXIT /B -1
)

REM Publish release
FOR %%P IN (*.nupkg) DO (
	CHOICE /M "Continue pushing package: '%%~nxP'?"
	IF ERRORLEVEL 2 (
		ECHO Aborting.
		EXIT /B -1
	)
	%NUGET% push "%%~nxP"
	IF ERRORLEVEL 1 (
		ECHO Unable to push package %%~nxP!  Abort!
		EXIT /B -1
	)
	REM Don't double tag and publish the source package
	goto BailAfterFirstPackage
)
:BailAfterFirstPackage

EXIT /B 0

:Usage
ECHO Usage:
ECHO %~nx0 [Project File]

EXIT /B -1