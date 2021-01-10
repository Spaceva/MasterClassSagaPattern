$RootPath = "."
$RootPath = (Resolve-Path $RootPath).Path
$BaseName = "MasterClassSagaPattern.Choregraphy"
$ServiceName = $args[0]
$CSProjPath = "$RootPath\$BaseName.$ServiceName\$BaseName.$ServiceName.csproj"
Write-Host "Calling $CSProjPath"
$StartInfo = new-object System.Diagnostics.ProcessStartInfo
$StartInfo.FileName = "$pshome\powershell.exe"
$StartInfo.Arguments = "-NoExit -Command `$Host.UI.RawUI.WindowTitle=`'$ServiceName`';dotnet run --project $CSProjPath"
[System.Diagnostics.Process]::Start($StartInfo)