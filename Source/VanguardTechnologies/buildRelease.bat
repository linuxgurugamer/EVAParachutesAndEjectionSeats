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


type EVAParachutesAndEjectionSeats.version
set /p VERSION= "Enter version: "

rd /s %HOMEDIR%\install\GameData\VanguardTechnologies
mkdir %HOMEDIR%\install\GameData\VanguardTechnologies
mkdir %HOMEDIR%\install\GameData\VanguardTechnologies\Plugins
mkdir %HOMEDIR%\install\GameData\VanguardTechnologies\Parts
mkdir %HOMEDIR%\install\GameData\VanguardTechnologies\Sounds

copy /Y "%~dp0bin\Debug\VanguardTechnologies.dll"  %HOMEDIR%\install\GameData\VanguardTechnologies\Plugins
xcopy /y /s "%~dp0..\..\GameData\VNG\Parts" "%HOMEDIR%\install\GameData\VanguardTechnologies\Parts"

xcopy /y /s "%~dp0..\..\GameData\VNG\Sounds" "%HOMEDIR%\install\GameData\VanguardTechnologies\Sounds"

rd /Q /s "%HOMEDIR%\install\GameData\VanguardTechnologies\Parts\VNG_Eject"

copy /Y "EVAParachutesAndEjectionSeats.version" "%HOMEDIR%\install\GameData\VanguardTechnologies"


copy /Y "..\..\License.txt" "%HOMEDIR%\install\GameData\VanguardTechnologies"
copy /Y "README.md" "%HOMEDIR%\install\GameData\VanguardTechnologies"
copy /Y MiniAVC.dll  "%HOMEDIR%\install\GameData\VanguardTechnologies"

%HOMEDRIVE%
cd %HOMEDIR%\install

set FILE="%RELEASEDIR%\VanguardTechnologies-%VERSION%.zip"
IF EXIST %FILE% del /F %FILE%
%ZIP% a -tzip %FILE% Gamedata\VanguardTechnologies