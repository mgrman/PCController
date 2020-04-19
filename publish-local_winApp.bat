rmdir /s /q "publish-local_winApp"
dotnet build src\PCController.Local.WinApp\PCController.Local.WinApp.csproj -c release -o publish-local_winApp