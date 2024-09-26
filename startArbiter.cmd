rem Start the Arbiter (no params as everything connects TO the arbiter)
cd configArbiter
SET ARBITER_KEY=thisasdfslkhjkfhkj2142rfKLJHLKHJCZgsdfg.XHFQ435GSLKVHJSLJKGHF
SET ENDPOINT=https://localhost:5000
dotnet run --urls %ENDPOINT%
