// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Reflection;
using System.Runtime.InteropServices;

// This project currently uses the same assembly name for all target framework
// versions. The only differences between the releases are the referenced versions
// of mscorlib and a few non-public (and thus non-breaking) implementation details.
// In other words, while the builds are not identical, they are interchangeable at
// runtime.
[assembly: AssemblyTitle("TunnelVisionLabs.Net.UriTemplate")]
[assembly: AssemblyDescription("An implementation of RFC 6570 URI templates for .NET")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Tunnel Vision Laboratories, LLC")]
[assembly: AssemblyProduct("TunnelVisionLabs.Net.UriTemplate")]
[assembly: AssemblyCopyright("Copyright © Sam Harwell 2015")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: CLSCompliant(true)]

#if !PORTABLE
// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]
#endif

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0-beta004")]
