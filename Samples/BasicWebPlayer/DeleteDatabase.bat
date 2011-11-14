rem  Copyright (c) Microsoft Corporation. All rights reserved. 

rem -- see ReadMe.txt -- note that you may need to exit Visual Studio first
iisreset
sqlcmd -Q "drop database Training"
rmdir /s /q c:\BasicWebPlayerPackages
