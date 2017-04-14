
@echo off
set DEFHOMEDRIVE=d:
set DEFHOMEDIR=%DEFHOMEDRIVE%%HOMEPATH%
set HOMEDIR=
set HOMEDRIVE=%CD:~0,2%

set RELEASEDIR=d:\Users\jbb\release
set ZIP="c:\Program Files\7-zip\7z.exe"
echo Default homedir: %DEFHOMEDIR%

rem set /p HOMEDIR= "Enter Home directory, or <CR> for default: "

if "%HOMEDIR%" == "" (
set HOMEDIR=%DEFHOMEDIR%
)
echo %HOMEDIR%

SET _test=%HOMEDIR:~1,1%
if "%_test%" == ":" (
set HOMEDRIVE=%HOMEDIR:~0,2%
)


rem type EVAParachutesAndEjectionSeats.version
rem set /p VERSION= "Enter version: "



copy EVAParachutesAndEjectionSeats.version a.version
set VERSIONFILE=a.version
rem The following requires the JQ program, available here: https://stedolan.github.io/jq/download/
c:\local\jq-win64  ".VERSION.MAJOR" %VERSIONFILE% >tmpfile
set /P major=<tmpfile

c:\local\jq-win64  ".VERSION.MINOR"  %VERSIONFILE% >tmpfile
set /P minor=<tmpfile

c:\local\jq-win64  ".VERSION.PATCH"  %VERSIONFILE% >tmpfile
set /P patch=<tmpfile

c:\local\jq-win64  ".VERSION.BUILD"  %VERSIONFILE% >tmpfile
set /P build=<tmpfile
del tmpfile
del a.version
set VERSION=%major%.%minor%.%patch%
if "%build%" NEQ "0"  set VERSION=%VERSION%.%build%

echo %VERSION%


rd /s %HOMEDIR%\install\GameData\VanguardTechnologies
mkdir %HOMEDIR%\install\GameData\VanguardTechnologies
mkdir %HOMEDIR%\install\GameData\VanguardTechnologies\Plugins
mkdir %HOMEDIR%\install\GameData\VanguardTechnologies\Parts
mkdir %HOMEDIR%\install\GameData\VanguardTechnologies\Sounds

copy /Y "%~dp0bin\Release\VanguardTechnologies.dll"  %HOMEDIR%\install\GameData\VanguardTechnologies\Plugins
xcopy /y /s "%~dp0..\..\GameData\VNG\Parts" "%HOMEDIR%\install\GameData\VanguardTechnologies\Parts"
copy /y ..\..\GameData\VNG\addEjectionToAll.cfg.patch "%HOMEDIR%\install\GameData\VanguardTechnologies"
copy /y ..\..\GameData\VNG\FAR_patch.cfg "%HOMEDIR%\install\GameData\VanguardTechnologies"

xcopy /y /s "%~dp0..\..\GameData\VNG\Sounds" "%HOMEDIR%\install\GameData\VanguardTechnologies\Sounds"

rem rd /Q /s "%HOMEDIR%\install\GameData\VanguardTechnologies\Parts\VNG_Eject"

copy /Y "EVAParachutesAndEjectionSeats.version" "%HOMEDIR%\install\GameData\VanguardTechnologies"


copy /Y "..\..\License.txt" "%HOMEDIR%\install\GameData\VanguardTechnologies"
copy /Y "README.md" "%HOMEDIR%\install\GameData\VanguardTechnologies"
copy /Y MiniAVC.dll  "%HOMEDIR%\install\GameData\VanguardTechnologies"

%HOMEDRIVE%
cd %HOMEDIR%\install

set FILE="%RELEASEDIR%\VanguardTechnologies-%VERSION%.zip"
IF EXIST %FILE% del /F %FILE%
%ZIP% a -tzip %FILE% Gamedata\VanguardTechnologies
pause