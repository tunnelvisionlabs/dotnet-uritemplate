# Note: these values may only change during major release

If ($Version.Contains('-')) {

	# Use the development keys
	$Keys = @{
		'net20' = 'e01bb0fde71f8e91'
		'portable-net40' = 'e01bb0fde71f8e91'
	}

} Else {

	# Use the final release keys
	$Keys = @{
		'net20' = '7ede881141368179'
		'portable-net40' = '7ede881141368179'
	}

}

function Resolve-FullPath() {
	param([string]$Path)
	[System.IO.Path]::GetFullPath((Join-Path (pwd) $Path))
}
