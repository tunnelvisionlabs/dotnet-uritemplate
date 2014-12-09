# Note: these values may only change during minor release
$Keys = @{
	'net35-client' = '5d67c22833d5111b'
	'portable-net40' = 'd56958160335d686'
}

function Resolve-FullPath() {
	param([string]$Path)
	[System.IO.Path]::GetFullPath((Join-Path (pwd) $Path))
}
