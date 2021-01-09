$RootPath = "../Common/MasterClassSagaPattern.MainUI"
$RootPath = (Resolve-Path $RootPath).Path
$BaseName = "MasterClassSagaPattern"
$ServiceName = "MainUI"
$CSProjPath = "$RootPath\$BaseName.$ServiceName.csproj"
Write-Host "Calling $CSProjPath"
$StartInfo = new-object System.Diagnostics.ProcessStartInfo
$StartInfo.FileName = "$pshome\powershell.exe"
$StartInfo.Arguments = "-NoExit -Command `$Host.UI.RawUI.WindowTitle=`'$ServiceName`';dotnet run --project $CSProjPath --BusVirtualHost orchestration --no-build"
[System.Diagnostics.Process]::Start($StartInfo)

$ServiceName = "OrderService"
& ".\launchOne.ps1" $ServiceName
$ServiceName = "DeliveryService"
& ".\launchOne.ps1" $ServiceName
$ServiceName = "PaymentService"
& ".\launchOne.ps1" $ServiceName
$ServiceName = "StockService"
& ".\launchOne.ps1" $ServiceName
$ServiceName = "BillingService"
& ".\launchOne.ps1" $ServiceName