rem  Copyright (c) Microsoft Corporation. All rights reserved. 

rem -- Propagates MLC and SLK code to SharePoint
call UnGAC.bat
call GAC.bat noiisreset
pushd Slk
nmake prop
popd
