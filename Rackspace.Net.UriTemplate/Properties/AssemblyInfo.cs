using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// This project currently uses the same assembly name for all target framework
// versions. The only differences between the releases are the referenced versions
// of mscorlib and a few non-public (and thus non-breaking) implementation details.
// In other words, while the builds are not identical, they are interchangeable at
// runtime.
[assembly: AssemblyTitle("Rackspace.Net.UriTemplate")]
[assembly: AssemblyDescription("An implementation of RFC 6570 URI templates for .NET")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Rackspace Inc.")]
[assembly: AssemblyProduct("Rackspace.Net.UriTemplate")]
[assembly: AssemblyCopyright("Copyright © Sam Harwell 2014")]
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
[assembly: AssemblyInformationalVersion("1.0.0.0-dev")]
