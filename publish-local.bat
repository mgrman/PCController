rmdir /s /q "publish-local"
dotnet publish src\PCController.Local\PCController.Local.csproj -c release -o publish-local