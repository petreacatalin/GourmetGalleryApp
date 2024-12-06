# Use the official .NET 8 SDK image as the build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /app

# Copy the project file(s) and restore dependencies
COPY *.csproj ./
RUN dotnet restore "./GourmeyGalleryApp.csproj"

# Copy the entire solution and build the application
COPY . ./
RUN dotnet publish "./GourmeyGalleryApp.csproj" -c Release -o /app/publish

# Use the official .NET 8 runtime image for the runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Expose the port the app runs on
EXPOSE 80

# Set the entry point for the application
ENTRYPOINT ["dotnet", "GourmeyGalleryApp.dll"]
