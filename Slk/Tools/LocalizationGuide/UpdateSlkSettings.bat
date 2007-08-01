rem  Copyright (c) Microsoft Corporation. All rights reserved. 

echo -- WARNING: This batch file overwrites the current SlkSettings.xml
pause
slkadm -o configuresite -url http://localhost -uploadslksettings SlkSettings.xml
if errorlevel 1 goto exit
iisreset
:exit
