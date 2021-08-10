FROM mcr.microsoft.com/dotnet/core/sdk:3.1
WORKDIR /app
COPY ./bin/Release/netcoreapp3.1/publish\ .
ENV ASPNETCORE_URLS http://*:5000
ENV ASPNETCORE_URLS https://*:5001
EXPOSE 5000
EXPOSE 5001
ENTRYPOINT ["dotnet", "M-url.Api.dll"]