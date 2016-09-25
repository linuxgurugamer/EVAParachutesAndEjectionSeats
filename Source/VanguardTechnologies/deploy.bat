

set H=R:\KSP_1.1.4_dev
echo %H%

set d=%H%
if exist %d% goto one
mkdir %d%
:one
set d=%H%\Gamedata
if exist %d% goto two
mkdir %d%
:two
set d=%H%\Gamedata\VanguardTechnologies
if exist %d% goto three
mkdir %d%
:three
set d=%H%\Gamedata\VanguardTechnologies\Plugins
if exist %d% goto four
mkdir %d%
:four
set d=%H%\Gamedata\VanguardTechnologies\Parts
if exist %d% goto five
mkdir %d%
:five

set d=%H%\Gamedata\VanguardTechnologies\Sounds
if exist %d% goto six
mkdir %d%
:six

rem copy /Y "%~dp0..\FrementGUILib\bin\Debug\FrementGUILib.dll" "%H%\GameData\VNG\Plugins"
copy /Y "%~dp0bin\Debug\VanguardTechnologies.dll" "%H%\GameData\VanguardTechnologies\Plugins"
copy /Y MiniAVC.dll  "%H%\GameData\VanguardTechnologies"
xcopy /y /s "%~dp0..\..\GameData\VNG\Parts" "%H%\GameData\VanguardTechnologies\Parts"
xcopy /y /s "%~dp0..\..\GameData\VNG\Sounds" "%H%\GameData\VanguardTechnologies\Sounds"

copy /Y "EVAParachutesAndEjectionSeats.version" "%H%\GameData\VanguardTechnologies"

copy /Y "%~dp0..\..\License.txt" "%H%\GameData\VanguardTechnologies"
rem copy /Y "%~dp0bin\Debug\README.md" "%H%\GameData\VanguardTechnologies"
