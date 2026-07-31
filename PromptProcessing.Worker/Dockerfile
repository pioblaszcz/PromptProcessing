FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src
COPY . .

RUN dotnet restore PromptProcessing.Worker/PromptProcessing.Worker.csproj
RUN dotnet publish PromptProcessing.Worker/PromptProcessing.Worker.csproj --configuration Release --no-restore --output /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime

WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "PromptProcessing.Worker.dll"]
