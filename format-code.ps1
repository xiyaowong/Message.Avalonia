if (-not (Get-Command xstyler -ErrorAction SilentlyContinue)) {
  Write-Host "$ dotnet tool install -g XamlStyler.Console"
  dotnet tool install -g XamlStyler.Console
}

Get-ChildItem -Path . -Recurse -Filter *.axaml | ForEach-Object {
  Write-Host "$ xstyler -f $($_.FullName)"
  xstyler -f $($_.FullName) -l None
}

if (-not (Get-Command csharpier -ErrorAction SilentlyContinue)) {
  Write-Host "$ dotnet tool install -g csharpier"
  dotnet tool install -g csharpier
}

Write-Host "$ csharpier format ."
csharpier format .