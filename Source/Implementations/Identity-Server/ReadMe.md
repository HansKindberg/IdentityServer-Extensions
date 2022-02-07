# Identity-Server

![.github/workflows/Identity-Server-Docker-Deploy.yml](https://github.com/HansKindberg/IdentityServer-Extensions/actions/workflows/Identity-Server-Docker-Deploy.yml/badge.svg)

IdentityServer instance, without configuration, pushed to Docker Hub: https://hub.docker.com/r/hanskindberg/identity-server

## 1 Development

During local development we can run the [Identity-Server implementation](/Source/Implementations/Identity-Server/Application) and it then has a project-reference to the [IdentityServer-Extensions project](/Source/Project):

- [Application.csproj line 17](/Source/Implementations/Identity-Server/Application/Application.csproj#L17)

## 2 Docker deployment

During Docker deployment another project-file is used that has a NuGet-reference to [HansKindberg.IdentityServer](https://www.nuget.org/packages/HansKindberg.IdentityServer). So before Docker deployment we need to publish the latest release of [HansKindberg.IdentityServer](https://www.nuget.org/packages/HansKindberg.IdentityServer) to NuGet and update the package-reference version in that file. Otherwise the application will not work. The project-file used for Docker deployment:

- [Application.csproj.deploy](/Source/Implementations/Identity-Server/Application/Application.csproj.deploy)