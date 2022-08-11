#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Presentation/Nop.Web/Nop.Web.csproj", "Presentation/Nop.Web/"]
COPY ["Libraries/Nop.Services/Nop.Services.csproj", "Libraries/Nop.Services/"]
COPY ["Libraries/Nop.Core/Nop.Core.csproj", "Libraries/Nop.Core/"]
COPY ["Libraries/Nop.Data/Nop.Data.csproj", "Libraries/Nop.Data/"]
COPY ["Presentation/Nop.Web.Framework/Nop.Web.Framework.csproj", "Presentation/Nop.Web.Framework/"]
RUN dotnet restore "Presentation/Nop.Web/Nop.Web.csproj"
COPY . .
WORKDIR "/src/Presentation/Nop.Web"
RUN dotnet build "Nop.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Nop.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Nop.Web.dll"]