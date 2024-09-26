rem Start the frontends 
cd FrontendRouter 
SET ARBITER_ENDPOINT=https://localhost:5000
SET FRONTEND_ENDPOINT=https://localhost:5500
SET ENDPOINT=https://localhost:5500
dotnet run --urls %ENDPOINT%