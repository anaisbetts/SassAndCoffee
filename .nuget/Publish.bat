@ECHO OFF

SET PROJECT=%1

ECHO Using project %PROJECT%

SET NUGET=%~dp0NuGet.exe
IF NOT EXIST %NUGET% (
	ECHO Couldn't locate NuGet.exe!
	EXIT /B -1
)
ECHO Using NuGet at %NUGET%

REM Check for changes
CALL git rev-parse --verify HEAD >NUL
IF %ERRORLEVEL% NEQ 0 (
	ECHO Not in a git repo?
	EXIT /B -1
)

CALL git update-index -q --ignore-submodules --refresh
SET ERROR = 0

CALL git diff-files --quiet --ignore-submodules
IF %ERRORLEVEL% NEQ 0 (
	ECHO Abort: You have unstaged changes.
	SET ERROR = 1
)

CALL git diff-index --cached --quiet --ignore-submodules HEAD --
IF %ERRORLEVEL% NEQ 0 (
	IF %ERROR% EQU 0 (
		ECHO Abort: Your index contains uncommitted changes.
	) ELSE (
		ECHO Additionally, your index contains uncommitted changes.
	)
	SET ERROR = 1
)

IF %ERROR% NEQ 0 (
	EXIT /B -1
)

ECHO Everything looks good so far.
CHOICE /M "About to purge. Continue?"
IF %ERRORLEVEL% NEQ 1 (
	Echo Aborting.
	EXIT /B -1
)

CALL git reset --hard >NUL 2>&1
CALL git clean -x -d -f >NUL 2>&1
ECHO Purge complete.

REM Build Package
%NUGET% pack %1 -Properties "Configuration=Release" -verbose -symbols -build
IF %ERRORLEVEL% NEQ 0 (
	ECHO Unable to create package!  Abort!
	EXIT /B -1
)

REM Tag and publish release
FOR %%P IN (*.nupkg) DO (
	REM hg tag "%%~nP" -m "Recording source for NuGet package" -r . > NUL 2>&1
	REM IF %ERRORLEVEL% NEQ 0 (
	REM 	ECHO Unable to tag!  Abort!
	REM 	EXIT /B -1
	REM )
	REM ECHO Created tag %%~nP
	CHOICE /M "Continue pushing package: '%%~nxP'?"
	IF %ERRORLEVEL% NEQ 0 (
		Echo Aborting.
		EXIT /B -1
	)
	%NUGET% push "%%~nxP"
	IF %ERRORLEVEL% NEQ 0 (
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