
@echo off

set RELEASEDIR=d:\Users\jbb\release
set ZIP="c:\Program Files\7-zip\7z.exe"

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

set HOMEDIR=D:\Users\jbb\github\EVAParachutesAndEjectionSeats
mkdir %HOMEDIR%\GameData\VanguardTechnologies
mkdir %HOMEDIR%\GameData\VanguardTechnologies\Plugins
mkdir %HOMEDIR%\GameData\VanguardTechnologies\Parts
mkdir %HOMEDIR%\GameData\VanguardTechnologies\Sounds

copy /Y "%~dp0bin\Release\VanguardTechnologies.dll"  %HOMEDIR%\GameData\VanguardTechnologies\Plugins
xcopy /y /s "%~dp0..\..\GameData\VNG\Parts" "%HOMEDIR%\GameData\VanguardTechnologies\Parts"
copy /y ..\..\GameData\VNG\addEjectionToAll.cfg.patch "%HOMEDIR%\GameData\VanguardTechnologies"
copy /y ..\..\GameData\VNG\FAR_patch.cfg "%HOMEDIR%\GameData\VanguardTechnologies"

xcopy /y /s "%~dp0..\..\GameData\VNG\Sounds" "%HOMEDIR%\GameData\VanguardTechnologies\Sounds"

copy /Y "EVAParachutesAndEjectionSeats.version" "%HOMEDIR%\GameData\VanguardTechnologies"


copy /Y "..\..\License.txt" "%HOMEDIR%\GameData\VanguardTechnologies"
copy /Y "README.md" "%HOMEDIR%\GameData\VanguardTechnologies"
copy /Y MiniAVC.dll  "%HOMEDIR%\GameData\VanguardTechnologies"

cd %HOMEDIR%

set FILE="%RELEASEDIR%\VanguardTechnologies-%VERSION%.zip"
IF EXIST %FILE% del /F %FILE%
%ZIP% a -tzip %FILE% Gamedata\VanguardTechnologies
pause