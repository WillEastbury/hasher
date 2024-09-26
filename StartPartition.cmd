REM Start the partition servers
cd Store
SET FIXED_PARTITION="%1"
SET ARBITER_KEY=thisasdfslkhjkfhkj2142rfKLJHLKHJCZgsdfg.XHFQ435GSLKVHJSLJKGHF
SET ARBITER_ENDPOINT=https://localhost:5000
SET PARTITION_ENDPOINT=https://localhost:%2
SET ENDPOINT=https://localhost:%2
SET ENDPOINT2=http://localhost:1%2
dotnet run --no-build --urls "%ENDPOINT%;%ENDPOINT2%"
