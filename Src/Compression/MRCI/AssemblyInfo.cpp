/* Copyright (c) Microsoft Corporation. All rights reserved. */

#include "stdafx.h"

using namespace System;
using namespace System::Reflection;
using namespace System::Runtime::CompilerServices;
using namespace System::Runtime::InteropServices;
using namespace System::Security::Permissions;

//
// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly:AssemblyTitleAttribute("MRCI")];
[assembly:AssemblyDescriptionAttribute("")];
[assembly:AssemblyConfigurationAttribute("")];
[assembly:AssemblyCompanyAttribute("MS")];
[assembly:AssemblyProductAttribute("MRCI")];
[assembly:AssemblyCopyrightAttribute("Copyright (c) MS 2007")];
[assembly:AssemblyTrademarkAttribute("")];
[assembly:AssemblyCultureAttribute("")];

//
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the value or you can default the Revision and Build Numbers
// by using the '*' as shown below:

#include "..\..\..\Src\Shared\vernum.h"

// from ..\..\..\SLK\Src\Shared\dllver.rc: define string form of version number...
#define postVer
#define VER_VERSIONNUM  SZVERNUM(rmj,rmm,rup)
// use double macros to force conversion of numbers to strings
#define SZVERNUM(x,y,z)     SZVERNUM2(x,y,z)
#define SZVERNUM2(x,y,z)  #x "." #y postVer "." #z ".0"

[assembly:AssemblyVersionAttribute(VER_VERSIONNUM)];
//[assembly:AssemblyVersionAttribute("1.0.0.0")];

[assembly:ComVisible(false)];

[assembly:CLSCompliantAttribute(true)];

[assembly:SecurityPermission(SecurityAction::RequestMinimum, UnmanagedCode = true)];
