REM Start the partition servers
cd Store
SET FIXED_PARTITION=128:255
SET ARBITER_KEY=thisasdfslkhjkfhkj2142rfKLJHLKHJCZgsdfg.XHFQ435GSLKVHJSLJKGHF
SET ARBITER_ENDPOINT=https://localhost:5000
SET PARTITION_ENDPOINT=https://localhost:7901
SET ENDPOINT=https://localhost:7901
SET ENDPOINT2=http://localhost:17901"

dotnet run --urls "%ENDPOINT%;%ENDPOINT2%"
