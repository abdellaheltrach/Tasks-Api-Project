# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["LoginApp.Api/LoginApp.Api.csproj", "LoginApp.Api/"]
COPY ["LoginApp.Business/LoginApp.Business.csproj", "LoginApp.Business/"]
COPY ["LoginApp.DataAccess/LoginApp.DataAccess.csproj", "LoginApp.DataAccess/"]

RUN dotnet restore "LoginApp.Api/LoginApp.Api.csproj"

# Copy all source code
COPY . .

# Build and publish
WORKDIR "/src/LoginApp.Api"
RUN dotnet build "LoginApp.Api.csproj" -c Release -o /app/build
RUN dotnet publish "LoginApp.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy published files from build stage
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Set entry point
ENTRYPOINT ["dotnet", "LoginApp.Api.dll"]
