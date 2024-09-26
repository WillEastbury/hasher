REM Start the partition servers
cd Store
SET FIXED_PARTITION=00:127
SET ARBITER_KEY=thisasdfslkhjkfhkj2142rfKLJHLKHJCZgsdfg.XHFQ435GSLKVHJSLJKGHF
SET ARBITER_ENDPOINT=https://localhost:5000
SET PARTITION_ENDPOINT=https://localhost:7500
SET ENDPOINT=https://localhost:7500
SET ENDPOINT2=http://localhost:17500;https://localhost:7500"

dotnet run --urls "%ENDPOINT%;%ENDPOINT2%"
