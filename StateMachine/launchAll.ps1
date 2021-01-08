$RootPath = $args[0]
write-host $RootPath
write-host "$RootPath\MasterClassSagaPattern.Orchestration.OrderService\MasterClassSagaPattern.Orchestration.OrderService.csproj"
$StartInfo = new-object System.Diagnostics.ProcessStartInfo
$StartInfo.FileName = "$pshome\powershell.exe"
$StartInfo.Arguments = "-NoExit -Command `$Host.UI.RawUI.WindowTitle=`'Orders`';dotnet run --project $RootPath\MasterClassSagaPattern.Orchestration.OrderService\MasterClassSagaPattern.Orchestration.OrderService.csproj --no-build"
[System.Diagnostics.Process]::Start($StartInfo)
$StartInfo = new-object System.Diagnostics.ProcessStartInfo
$StartInfo.FileName = "$pshome\powershell.exe"
$StartInfo.Arguments = "-NoExit -Command `$Host.UI.RawUI.WindowTitle=`'Delivery`';dotnet run --project $RootPath\MasterClassSagaPattern.Orchestration.DeliveryService\MasterClassSagaPattern.Orchestration.DeliveryService.csproj --no-build"
[System.Diagnostics.Process]::Start($StartInfo)
$StartInfo = new-object System.Diagnostics.ProcessStartInfo
$StartInfo.FileName = "$pshome\powershell.exe"
$StartInfo.Arguments = "-NoExit -Command `$Host.UI.RawUI.WindowTitle=`'Payment`';dotnet run --project $RootPath\MasterClassSagaPattern.Orchestration.PaymentService\MasterClassSagaPattern.Orchestration.PaymentService.csproj --no-build"
[System.Diagnostics.Process]::Start($StartInfo)
$StartInfo = new-object System.Diagnostics.ProcessStartInfo
$StartInfo.FileName = "$pshome\powershell.exe"
$StartInfo.Arguments = "-NoExit -Command `$Host.UI.RawUI.WindowTitle=`'Stocks`';dotnet run --project $RootPath\MasterClassSagaPattern.Orchestration.StockService\MasterClassSagaPattern.Orchestration.StockService.csproj --no-build"
[System.Diagnostics.Process]::Start($StartInfo)
$StartInfo = new-object System.Diagnostics.ProcessStartInfo
$StartInfo.FileName = "$pshome\powershell.exe"
$StartInfo.Arguments = "-NoExit -Command `$Host.UI.RawUI.WindowTitle=`'Billing`';dotnet run --project $RootPath\MasterClassSagaPattern.Orchestration.BillingService\MasterClassSagaPattern.Orchestration.BillingService.csproj --no-build"
[System.Diagnostics.Process]::Start($StartInfo)