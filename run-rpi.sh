export ASPNETCORE_ENVIRONMENT=rpi
dotnet run --project "src/PCController.Local/PCController.Local.csproj" -- --urls "http://*:8080"
read -p "Press enter to continue"