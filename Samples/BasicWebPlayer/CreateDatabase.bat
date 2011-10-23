rem  Copyright (c) Microsoft Corporation. All rights reserved. 

rem -- see ReadMe.txt
sqlcmd -Q "create database Training"
sqlcmd -b -d Training -i Schema.sql
mkdir c:\BasicWebPlayerPackages
