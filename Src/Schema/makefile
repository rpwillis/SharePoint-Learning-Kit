#  Copyright (c) Microsoft Corporation. All rights reserved. 

#

all: SchemaDataComponents.cs SchemaDataStorage.cs InitSchema.sql

SchemaDataComponents.cs SchemaDataStorage.cs InitSchema.sql: ..\SchemaCompiler\BaseSchema.xml ..\SchemaCompiler\bin\debug\SchemaCompiler.exe
    ..\SchemaCompiler\bin\debug\SchemaCompiler.exe /OutputBaseInit InitSchema.sql /OutputComponentsHelper SchemaDataComponents.cs /OutputStorageHelper SchemaDataStorage.cs

deb: all

rel: all

ide: all

runtests:

clean: 
	-del schema.vcproj.*.user
	-attrib -h Schema.suo
	-del Schema.suo
    -rmdir /s /q Debug 2> nul
    -rmdir /s /q Release 2> nul
	-del SchemaDataComponents.cs
	-del SchemaDataStorage.cs
	-del InitSchema.sql

cleanall: clean 
