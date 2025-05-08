$project = ".\demo\Message.Avalonia.Demo.Desktop"
$folderPath = Join-Path $project "bin\Release\net9.0\win-x64\publish"

Write-Host "1. Cleanup：$folderPath"
if (Test-Path $folderPath) {
    Get-ChildItem -Path $folderPath -Recurse -Force | Remove-Item -Force -Recurse
}

Write-Host "2. Publish"
dotnet publish $project -r win-x64 -c Release

Write-Host "3. Remove debug files"
Remove-Item (Join-Path $folderPath "*.pdb") -Force -ErrorAction SilentlyContinue
