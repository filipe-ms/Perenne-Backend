# Stage 1: Build the application
# Use the .NET SDK image corresponding to your project's .NET version (e.g., 9.0)
using Microsoft.EntityFrameworkCore.Infrastructure;

FROM mcr.microsoft.com / dotnet / sdk:9.0 AS build-env
WORKDIR /app

# Copy project files and restore dependencies
# Copy .csproj files first to leverage Docker layer caching for dependencies
# Adjust if your .csproj is not in the root or if you have multiple projects
COPY *.csproj ./
# If your solution has multiple projects in subdirectories, you might need:
# COPY Perenne-Backend.sln ./
# COPY src/Perenne.Backend/Perenne.Backend.csproj ./src/Perenne.Backend/
# (Adjust paths as per your actual structure if more complex)
RUN dotnet restore

# Copy the rest of the application code
COPY . ./

# Publish the application
# Replace 'Perenne.Backend.csproj' with the actual name of your main project file if different
RUN dotnet publish Perenne.Backend.csproj -c Release -o out --no-restore

# Stage 2: Create the runtime image
# Use the ASP.NET runtime image corresponding to your project's .NET version
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR / app

# Copy the published output from the build stage
COPY--from = build - env / app /out .

# Set environment variables (Render will override these if set in its dashboard)
ENV ASPNETCORE_ENVIRONMENT = Production
# The PORT variable will be supplied by Render.
# ASPNETCORE_URLS tells Kestrel to listen on the port specified by the PORT env var.
ENV ASPNETCORE_URLS=http://+:80

# Entry point for the application
# Replace 'Perenne.Backend.dll' with the actual name of your application's entry point DLL
ENTRYPOINT["dotnet", "Perenne.Backend.dll"]