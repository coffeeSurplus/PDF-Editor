$versions = @(
	@("win-x64", $false, "x64", "framework-dependent"),
	@("win-x64", $true, "x64", "self-contained"),
	@("win-x86", $false, "x86", "framework-dependent"),
	@("win-x86", $true, "x86", "self-contained")
)

$output = "C:\Users\Lewis\Downloads\"

foreach ($version in $versions)
{
	$runtime = $version[0]
	$selfContained = $version[1]
	$runtimeText = $version[2]
	$selfContainedText = $version[3]

	$fileName = "PDFEditor_${runtimeText}_${selfContainedText}"

	Write-Host "Publishing $fileName..." -ForegroundColor Cyan

	dotnet publish ".\PDF Editor.csproj" `
		--output $output `
		--configuration release `
		--runtime $runtime `
		--framework net8.0-windows `
		--self-contained $selfContained

	if ($LASTEXITCODE -ne 0)
	{
		Write-Host "Publish failed for $fileName" -ForegroundColor Red
		exit $LASTEXITCODE
	}

	Rename-Item `
		"$output\PDFEditor.exe" `
		"$output\$fileName.exe"

	Write-Host "Published $fileName.exe" -ForegroundColor Green
}