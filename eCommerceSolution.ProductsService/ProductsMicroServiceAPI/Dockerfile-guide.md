# Dockerfile Guide: Products Microservice API

This guide provides a detailed, line-by-line technical breakdown of the `Dockerfile`. It explains exactly what each instruction does in the context of building a .NET Core application using a multi-stage Docker build.

---

### 1. Base Stage (Runtime Environment)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
```
- `FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base`: Pulls the official .NET 10 ASP.NET Core runtime image from Microsoft's container registry. This lightweight image contains only what is necessary to run a compiled .NET app (no SDK/build tools). It is labeled as the `base` stage.
- `USER $APP_UID`: Switches the user context to a non-root user defined by the `$APP_UID` environment variable (a security best practice for running containers).
- `WORKDIR /app`: Sets the working directory inside the container to `/app`. All subsequent commands in this stage will execute in this directory.
- `EXPOSE 8080` / `EXPOSE 8081`: Documents that the container listens on TCP ports 8080 (HTTP) and 8081 (HTTPS). These are the default ports for .NET 8+ applications running as non-root users.

---

### 2. Build Stage (Compiling the Code)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ProductsMicroServiceAPI/ProductsMicroServiceAPI.csproj", "ProductsMicroServiceAPI/"]
RUN dotnet restore "./ProductsMicroServiceAPI/ProductsMicroServiceAPI.csproj"
COPY . .
WORKDIR "/src/ProductsMicroServiceAPI"
RUN dotnet build "./ProductsMicroServiceAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build
```
- `FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build`: Initiates a new build stage using the much larger .NET 10 SDK image, which includes all the tools needed to compile the application. It is labeled as the `build` stage.
- `ARG BUILD_CONFIGURATION=Release`: Defines a build-time variable named `BUILD_CONFIGURATION` with a default value of `Release`.
- `WORKDIR /src`: Sets the working directory to `/src` for the build process.
- `COPY ["ProductsMicroServiceAPI/ProductsMicroServiceAPI.csproj", "ProductsMicroServiceAPI/"]`: Copies only the `.csproj` file into the container first. Doing this before copying the rest of the source code allows Docker to cache the restored NuGet packages, significantly speeding up future builds.
- `RUN dotnet restore ...`: Executes the `dotnet restore` command to download all necessary NuGet packages and dependencies defined in the `.csproj` file.
- `COPY . .`: Copies the remaining source code from the host machine into the container's `/src` directory.
- `WORKDIR "/src/ProductsMicroServiceAPI"`: Changes the working directory to the specific project folder where the source code resides.
- `RUN dotnet build ...`: Compiles the application code.
  - `-c $BUILD_CONFIGURATION`: Uses the configuration defined earlier (Release).
  - `-o /app/build`: Specifies `/app/build` as the output directory for the compiled binaries.

---

### 3. Publish Stage (Packaging for Deployment)

```dockerfile
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ProductsMicroServiceAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
```
- `FROM build AS publish`: Starts a new stage based on the exact state of the previous `build` stage. It is labeled as `publish`.
- `ARG BUILD_CONFIGURATION=Release`: Re-declares the argument so it is available in this stage.
- `RUN dotnet publish ...`: Compiles the application and its dependencies into a folder for deployment.
  - `-c $BUILD_CONFIGURATION`: Builds in Release mode.
  - `-o /app/publish`: Places the final, ready-to-deploy files into the `/app/publish` directory.
  - `/p:UseAppHost=false`: Prevents the generation of a native executable host (like `.exe`), which is unnecessary inside a container that invokes the `.dll` directly via the `dotnet` command.

---

### 4. Final Stage (Creating the Production Image)

```dockerfile
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV MYSQL_HOST=localhost
ENV MYSQL_PASSWORD=admin
ENV MYSQL_DATBASE=ecommerceproductsdatabase
ENV MYSQL_USER=root
ENV MYSQL_PORT=3306
ENTRYPOINT ["dotnet", "ProductsMicroServiceAPI.dll"]
```
- `FROM base AS final`: Starts the final, production-ready stage. It uses the lightweight `base` image (the ASP.NET runtime) defined in Step 1, completely leaving behind the heavy SDK tools from the `build` stage.
- `WORKDIR /app`: Sets the working directory to `/app`.
- `COPY --from=publish /app/publish .`: Copies the finalized, published files from the `/app/publish` directory of the `publish` stage into the current `/app` directory of this `final` stage.
- `ENV ...`: Sets several runtime environment variables (`MYSQL_HOST`, `MYSQL_PASSWORD`, etc.) inside the container. These configure the database connection string parameters that the application will use at runtime.
- `ENTRYPOINT ["dotnet", "ProductsMicroServiceAPI.dll"]`: Configures the container to run as an executable. When the container starts, it will execute the `dotnet` command and pass `ProductsMicroServiceAPI.dll` as the argument, starting the web API.
