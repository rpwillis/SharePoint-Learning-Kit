rem  Copyright (c) Microsoft Corporation. All rights reserved. 

slkadm\bin\Debug\slkadm -o configuresite -url http://localhost/districts/bellevue -uploadslksettings SlkSettings.xml
if errorlevel 1 goto exit
iisreset
:exit
