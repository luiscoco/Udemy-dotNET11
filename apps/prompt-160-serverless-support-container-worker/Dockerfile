FROM mcr.microsoft.com/dotnet/runtime:11.0
WORKDIR /app
COPY artifacts/publish/ ./
USER $APP_UID
ENTRYPOINT ["dotnet", "Worker.dll"]
