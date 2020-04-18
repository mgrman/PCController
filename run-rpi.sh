export ASPNETCORE_ENVIRONMENT=rpi
dotnet run --project "src/PCController.Local/PCController.Local.csproj" --no-launch-profile -- --urls "http://*:8080"