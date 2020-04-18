export ASPNETCORE_ENVIRONMENT=surface
dotnet run --project "src/PCController.Local/PCController.Local.csproj" --no-launch-profile -- --urls "http://*:8080"