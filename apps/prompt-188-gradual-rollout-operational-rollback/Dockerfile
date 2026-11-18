FROM mcr.microsoft.com/dotnet/aspnet@sha256:bc0d6653979efba491ab3d6f589d691f96c309c3c3ef81325efa6978e7b7f242
WORKDIR /app
COPY . .
ENV ASPNETCORE_HTTP_PORTS=8080
USER 1654
EXPOSE 8080
HEALTHCHECK --interval=2s --timeout=3s --start-period=5s --retries=3 CMD ["dotnet","Web.dll","--health"]
ENTRYPOINT ["dotnet","Web.dll"]
