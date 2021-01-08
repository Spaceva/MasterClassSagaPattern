$RootPath = $args[0]
write-host $RootPath
write-host "$RootPath\MasterClassSagaPattern.Choregraphy.OrderService\MasterClassSagaPattern.Choregraphy.OrderService.csproj"
$StartInfo = new-object System.Diagnostics.ProcessStartInfo
$StartInfo.FileName = "$pshome\powershell.exe"
$StartInfo.Arguments = "-NoExit -Command `$Host.UI.RawUI.WindowTitle=`'Orders`';dotnet run --project $RootPath\MasterClassSagaPattern.Choregraphy.OrderService\MasterClassSagaPattern.Choregraphy.OrderService.csproj --no-build"
[System.Diagnostics.Process]::Start($StartInfo)
$StartInfo = new-object System.Diagnostics.ProcessStartInfo
$StartInfo.FileName = "$pshome\powershell.exe"
$StartInfo.Arguments = "-NoExit -Command `$Host.UI.RawUI.WindowTitle=`'Delivery`';dotnet run --project $RootPath\MasterClassSagaPattern.Choregraphy.DeliveryService\MasterClassSagaPattern.Choregraphy.DeliveryService.csproj --no-build"
[System.Diagnostics.Process]::Start($StartInfo)
$StartInfo = new-object System.Diagnostics.ProcessStartInfo
$StartInfo.FileName = "$pshome\powershell.exe"
$StartInfo.Arguments = "-NoExit -Command `$Host.UI.RawUI.WindowTitle=`'Payment`';dotnet run --project $RootPath\MasterClassSagaPattern.Choregraphy.PaymentService\MasterClassSagaPattern.Choregraphy.PaymentService.csproj --no-build"
[System.Diagnostics.Process]::Start($StartInfo)
$StartInfo = new-object System.Diagnostics.ProcessStartInfo
$StartInfo.FileName = "$pshome\powershell.exe"
$StartInfo.Arguments = "-NoExit -Command `$Host.UI.RawUI.WindowTitle=`'Stocks`';dotnet run --project $RootPath\MasterClassSagaPattern.Choregraphy.StockService\MasterClassSagaPattern.Choregraphy.StockService.csproj --no-build"
[System.Diagnostics.Process]::Start($StartInfo)
$StartInfo = new-object System.Diagnostics.ProcessStartInfo
$StartInfo.FileName = "$pshome\powershell.exe"
$StartInfo.Arguments = "-NoExit -Command `$Host.UI.RawUI.WindowTitle=`'Billing`';dotnet run --project $RootPath\MasterClassSagaPattern.Choregraphy.BillingService\MasterClassSagaPattern.Choregraphy.BillingService.csproj --no-build"
[System.Diagnostics.Process]::Start($StartInfo)