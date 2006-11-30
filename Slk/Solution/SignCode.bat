rem  Copyright (c) Microsoft Corporation. All rights reserved. 

if not exist ..\..\..\SLK.Internal\Tools\SignCodeAcct.bat goto exit
..\..\..\SLK.Internal\Tools\SignCodeAcct.bat ..\..\..\SLK.Internal %1
:exit
