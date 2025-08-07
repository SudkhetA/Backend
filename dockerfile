FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
RUN rm /etc/localtime && \
    ln -s /usr/share/zoneinfo/Asia/Bangkok /etc/localtime
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ./Backend ./Backend
RUN dotnet build "./Backend/Backend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./Backend/Backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Backend.dll"]
