/* Copyright (c) Microsoft Corporation. All rights reserved. */

The schema compiler has several "hidden" options used during builds:

/BaseSchema <path-to-schema-file>:
    The schema compiler will use the specified schema as the base schema.
    
/OutputComponentsHelper <path-to-output-file>:
    The schema compiler will output a .cs file that can be compiled into
    the Microsoft.LearningComponents assembly.
    
/OutputStorageHelper <path-to-output-file>:
    The schema compiler will output a .cs file that can be compiled into
    the Microsoft.LearningComponents.Storage assembly.

/OutputBaseInit <path-to-output-file>:
    The schema compiler will output a .sql file that can initialize the
    database.  The file only contains information from the base schema
    -- it does not contain information from the derived schema.
