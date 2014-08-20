param (
	[switch]$Debug,
	[string]$VisualStudioVersion = "12.0",
	[switch]$SkipKeyCheck
)

# build the solution
$SolutionPath = "..\UriTemplate.sln"

# make sure the script was run from the expected path
if (!(Test-Path $SolutionPath)) {
	$host.ui.WriteErrorLine('The script was run from an invalid working directory.')
	exit 1
}

. .\version.ps1

If ($Debug) {
	$BuildConfig = 'Debug'
} Else {
	$BuildConfig = 'Release'
}

# build the main project
$nuget = '..\.nuget\NuGet.exe'
$msbuild = "$env:windir\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe"

&$nuget 'restore' $SolutionPath
&$msbuild '/nologo' '/m' '/nr:false' '/t:rebuild' "/p:Configuration=$BuildConfig" "/p:VisualStudioVersion=$VisualStudioVersion" $SolutionPath
if ($LASTEXITCODE -ne 0) {
	$host.ui.WriteErrorLine('Build failed, aborting!')
	exit $p.ExitCode
}

# By default, do not create a NuGet package unless the expected strong name key files were used
if (-not $SkipKeyCheck) {
	. .\keys.ps1

	foreach ($pair in $Keys.GetEnumerator()) {
		$assembly = Resolve-FullPath -Path "..\Rackspace.Net.UriTemplate\bin\$($pair.Key)\$BuildConfig\Rackspace.Net.UriTemplate.dll"
		# Run the actual check in a separate process or the current process will keep the assembly file locked
		powershell -Command ".\check-key.ps1 -Assembly '$assembly' -ExpectedKey '$($pair.Value)' -Build '$($pair.Key)'"
		if ($LASTEXITCODE -ne 0) {
			Exit $p.ExitCode
		}
	}
}

if (-not (Test-Path 'nuget')) {
	mkdir "nuget"
}

&$nuget 'pack' '..\Rackspace.Net.UriTemplate\Rackspace.Net.UriTemplate.nuspec' '-OutputDirectory' 'nuget' '-Prop' "Configuration=$BuildConfig" '-Version' "$Version" '-Symbols'
