# IdentityServer-Extensions

Additions and extensions for [IdentityServer](https://github.com/IdentityServer/IdentityServer4/).

[![NuGet](https://img.shields.io/nuget/v/HansKindberg.IdentityServer.svg?label=NuGet)](https://www.nuget.org/packages/HansKindberg.IdentityServer)

Additions/extensions to be able to setup a **configurable**, **globalizable** and **localizable** implementation of [IdentityServer](https://github.com/IdentityServer/IdentityServer4/).

The idea is to setup an IdentityServer-implementation like the included [sample](/Source/Sample/Application) and configure continous-release with substitution/transforms for Web.config & AppSettings.json or to copy the sample and use it as a template. The copy can then bee changed regarding configuration, style and translations.

IdentityServer:

- Documentation: https://identityserver.io/
- GitHub: https://github.com/IdentityServer/IdentityServer4/
- NuGet: https://www.nuget.org/packages/IdentityServer4/

## 1 Features

- [Feature.cs](/Source/Project/FeatureManagement/Feature.cs)
- [Example](/Source/Sample/Application/appsettings.Development.json#L353)

## 2 Configuration

### 2.1 Features
See above.

### 2.2 Globalization

- [Example](/Source/Sample/Application/appsettings.Development.json#L473)

### 2.3 Localization

- [Path-based localization](https://github.com/RegionOrebroLan/.NET-Localization-Extensions#1-path-based-localization)
- [Example](/Source/Sample/Application/Resources)

### 2.4 Authentication-schemes

#### 2.4.1 Example

- [Authentication](/Source/Sample/Application/appsettings.Development.json#L64)
- [Scheme-registrations](/Source/Sample/Application/appsettings.Development.json#L103)

Solution behind it:

- [.NET-Web-Authentication-Extensions](https://github.com/RegionOrebroLan/.NET-Web-Authentication-Extensions)
- [ActiveLogin-Authentication-Extensions](https://github.com/RegionOrebroLan/ActiveLogin-Authentication-Extensions)

##### 2.4.2 Providers

##### 2.4.2.1 Facebook

- [Facebook external login setup in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/facebook-logins/)

##### 2.4.2.2 Google

- [Google external login setup in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins/)
- https://console.developers.google.com/
- [Example](/Source/Application/Data/Authentication.json#L92)

##### 2.4.2.3 Microsoft

- [Microsoft Account external login setup with ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/microsoft-logins/)
- https://portal.azure.com/?l=en.en-001#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/RegisteredApps
- Examples
  - [Azure AD (single tenant)](/Source/Application/Data/Authentication.json#L62)
  - [Microsoft (multitenant and personal accounts)](/Source/Application/Data/Authentication.json#L116)

##### 2.4.2.4 Twitter

- [Twitter external sign-in setup with ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/twitter-logins/)

##### 2.4.2.5 Other providers

- [External OAuth authentication providers](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/other-logins/)

## 3 Development

### 3.1 Migrations

We might want to create/recreate migrations. If we can accept data-loss we can recreate the migrations otherwhise we will have to update them.

Copy all the commands below and run them in the Package Manager Console for the affected database-context.

If you want more migration-information you can add the -Verbose parameter:

	Add-Migration TheMigration -Context TheDatabaseContext -OutputDir Data/Migrations -Project Project -Verbose;

**Important!** Before running the commands below you need to ensure the "Project"-project is set as startup-project. 

#### 3.1.1 Configuration

##### 3.1.1.1 Create migrations

	Write-Host "Removing migrations...";
	Remove-Migration -Context SqliteConfiguration -Force -Project Project;
	Remove-Migration -Context SqlServerConfiguration -Force -Project Project;
	Write-Host "Removing current migrations-directory...";
	Remove-Item "Project\Data\Migrations\Configuration" -ErrorAction Ignore -Force -Recurse;
	Write-Host "Creating migrations...";
	Add-Migration SqliteConfigurationMigration -Context SqliteConfiguration -OutputDir Data/Migrations/Configuration/Sqlite -Project Project;
	Add-Migration SqlServerConfigurationMigration -Context SqlServerConfiguration -OutputDir Data/Migrations/Configuration/SqlServer -Project Project;
	Write-Host "Finnished";

##### 3.1.1.2 Update migrations

	Write-Host "Updating migrations...";
	Add-Migration SqliteConfigurationMigrationUpdate -Context SqliteConfiguration -OutputDir Data/Migrations/Configuration/Sqlite -Project Project;
	Add-Migration SqlServerConfigurationMigrationUpdate -Context SqlServerConfiguration -OutputDir Data/Migrations/Configuration/SqlServer -Project Project;
	Write-Host "Finnished";

#### 3.1.2 DataProtection

##### 3.1.2.1 Create migrations

	Write-Host "Removing migrations...";
	Remove-Migration -Context SqliteDataProtection -Force -Project Project;
	Remove-Migration -Context SqlServerDataProtection -Force -Project Project;
	Write-Host "Removing current migrations-directory...";
	Remove-Item "Project\DataProtection\Data\Migrations" -ErrorAction Ignore -Force -Recurse;
	Write-Host "Creating migrations...";
	Add-Migration SqliteDataProtectionMigration -Context SqliteDataProtection -OutputDir DataProtection/Data/Migrations/Sqlite -Project Project;
	Add-Migration SqlServerDataProtectionMigration -Context SqlServerDataProtection -OutputDir DataProtection/Data/Migrations/SqlServer -Project Project;
	Write-Host "Finnished";

##### 3.1.2.2 Update migrations

	Write-Host "Updating migrations...";
	Add-Migration SqliteDataProtectionMigrationUpdate -Context SqliteDataProtection -OutputDir DataProtection/Data/Migrations/Sqlite -Project Project;
	Add-Migration SqlServerDataProtectionMigrationUpdate -Context SqlServerDataProtection -OutputDir DataProtection/Data/Migrations/SqlServer -Project Project;
	Write-Host "Finnished";

#### 3.1.3 Identity

##### 3.1.3.1 Create migrations

	Write-Host "Removing migrations...";
	Remove-Migration -Context SqliteIdentity -Force -Project Project;
	Remove-Migration -Context SqlServerIdentity -Force -Project Project;
	Write-Host "Removing current migrations-directory...";
	Remove-Item "Project\Identity\Data\Migrations" -ErrorAction Ignore -Force -Recurse;
	Write-Host "Creating migrations...";
	Add-Migration SqliteIdentityMigration -Context SqliteIdentity -OutputDir Identity/Data/Migrations/Sqlite -Project Project;
	Add-Migration SqlServerIdentityMigration -Context SqlServerIdentity -OutputDir Identity/Data/Migrations/SqlServer -Project Project;
	Write-Host "Finnished";

##### 3.1.3.2 Update migrations

	Write-Host "Updating migrations...";
	Add-Migration SqliteIdentityMigrationUpdate -Context SqliteIdentity -OutputDir Identity/Data/Migrations/Sqlite -Project Project;
	Add-Migration SqlServerIdentityMigrationUpdate -Context SqlServerIdentity -OutputDir Identity/Data/Migrations/SqlServer -Project Project;
	Write-Host "Finnished";

#### 3.1.4 Operational (PersistedGrant)

##### 3.1.4.1 Create migrations

	Write-Host "Removing migrations...";
	Remove-Migration -Context SqliteOperational -Force -Project Project;
	Remove-Migration -Context SqlServerOperational -Force -Project Project;
	Write-Host "Removing current migrations-directory...";
	Remove-Item "Project\Data\Migrations\Operational" -ErrorAction Ignore -Force -Recurse;
	Write-Host "Creating migrations...";
	Add-Migration SqliteOperationalMigration -Context SqliteOperational -OutputDir Data/Migrations/Operational/Sqlite -Project Project;
	Add-Migration SqlServerOperationalMigration -Context SqlServerOperational -OutputDir Data/Migrations/Operational/SqlServer -Project Project;
	Write-Host "Finnished";

##### 3.1.4.2 Update migrations

	Write-Host "Updating migrations...";
	Add-Migration SqliteOperationalMigrationUpdate -Context SqliteOperational -OutputDir Data/Migrations/Operational/Sqlite -Project Project;
	Add-Migration SqlServerOperationalMigrationUpdate -Context SqlServerOperational -OutputDir Data/Migrations/Operational/SqlServer -Project Project;
	Write-Host "Finnished";

#### 3.1.5 Plugins

##### 3.1.5.1 SAML

###### 3.1.5.1.1 Create migrations

	Write-Host "Removing migrations...";
	Remove-Migration -Context SqliteSamlConfiguration -Force -Project Project;
	Remove-Migration -Context SqlServerSamlConfiguration -Force -Project Project;
	Write-Host "Removing current migrations-directory...";
	Remove-Item "Project\Data\Saml\Migrations" -ErrorAction Ignore -Force -Recurse;
	Write-Host "Creating migrations...";
	Add-Migration SqliteSamlConfigurationMigration -Context SqliteSamlConfiguration -OutputDir Data/Saml/Migrations/Sqlite -Project Project;
	Add-Migration SqlServerSamlConfigurationMigration -Context SqlServerSamlConfiguration -OutputDir Data/Saml/Migrations/SqlServer -Project Project;
	Write-Host "Finnished";

###### 3.1.5.1.2 Update migrations

	Write-Host "Updating migrations...";
	Add-Migration SqliteSamlConfigurationMigrationUpdate -Context SqliteSamlConfiguration -OutputDir Data/Saml/Migrations/Sqlite -Project Project;
	Add-Migration SqlServerSamlConfigurationMigrationUpdate -Context SqlServerSamlConfiguration -OutputDir Data/Saml/Migrations/SqlServer -Project Project;
	Write-Host "Finnished";

##### 3.1.5.2 WsFederation

###### 3.1.5.2.1 Create migrations

	Write-Host "Removing migrations...";
	Remove-Migration -Context SqliteWsFederationConfiguration -Force -Project Project;
	Remove-Migration -Context SqlServerWsFederationConfiguration -Force -Project Project;
	Write-Host "Removing current migrations-directory...";
	Remove-Item "Project\Data\WsFederation\Migrations" -ErrorAction Ignore -Force -Recurse;
	Write-Host "Creating migrations...";
	Add-Migration SqliteWsFederationConfigurationMigration -Context SqliteWsFederationConfiguration -OutputDir Data/WsFederation/Migrations/Sqlite -Project Project;
	Add-Migration SqlServerWsFederationConfigurationMigration -Context SqlServerWsFederationConfiguration -OutputDir Data/WsFederation/Migrations/SqlServer -Project Project;
	Write-Host "Finnished";

###### 3.1.5.2.2 Update migrations

	Write-Host "Updating migrations...";
	Add-Migration SqliteWsFederationConfigurationMigrationUpdate -Context SqliteWsFederationConfiguration -OutputDir Data/WsFederation/Migrations/Sqlite -Project Project;
	Add-Migration SqlServerWsFederationConfigurationMigrationUpdate -Context SqlServerWsFederationConfiguration -OutputDir Data/WsFederation/Migrations/SqlServer -Project Project;
	Write-Host "Finnished";

## 4 Technicalities

### 4.1 Development-environment
This solution is built on:

- Visual Studio Enterprise 2019
- Windows 10

### 4.2 NuGet
The solution uses the following NuGet-packages:

- [Project](/Source/Project/Project.csproj#L26)

### 4.3 Documentation

- [IdentityServer4](https://identityserver4.readthedocs.io/)
- [SAML 2.0 Integration with IdentityServer4](https://www.identityserver.com/articles/saml-20-integration-with-identityserver4/)
- [IdentityServer4 Saml 2.0 Component](https://www.identityserver.com/documentation/saml2p/)

### 4.4 Examples

- [IdentityServer4 samples](https://github.com/RockSolidKnowledge/)

### 4.5 Information

- [IdentityServer.com](https://www.identityserver.com/)

#### 4.5.1 Products

- [IdentityServer products](https://www.identityserver.com/products/)

## 5 Notes

Various saved notes that appeared during development.

### 5.1 Mutual TLS (client-certificate-authentication)

- [Mutual TLS ? IdentityServer4 1.0.0 documentation](http://docs.identityserver.io/en/latest/topics/mtls.html)
- [Transport Layer Security (TLS) registry settings](https://docs.microsoft.com/sv-se/windows-server/security/tls/tls-registry-settings/)
- [Overview of TLS - SSL (Schannel SSP)](https://docs.microsoft.com/sv-se/windows-server/security/tls/what-s-new-in-tls-ssl-schannel-ssp-overview/)
- [Answer to: Mutual certificates authentication fails with error 403.16](https://stackoverflow.com/questions/27232340/mutual-certificates-authentication-fails-with-error-403-16#answer-27282889)
- [Testing with client certificate authentication in a development environment on IIS 8.5](https://itq.nl/testing-with-client-certificate-authentication-in-a-development-environment-on-iis-8-5/)
- [What's New in TLS/SSL (Schannel SSP)](https://docs.microsoft.com/en-us/previous-versions/windows/it-pro/windows-server-2012-R2-and-2012/hh831771(v=ws.11))

#### HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SecurityProviders\Schannel\SendTrustedIssuerList

If it does not exist, create it as REG_DWORD.

Values:

0.	Off
1.	On

#### HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SecurityProviders\Schannel\ClientAuthTrustMode

If it does not exist, create it as REG_DWORD.

Values:

0.	Machine Trust (default) - Requires that the client certificate is issued by a certificate in the Trusted Issuers list.
1.	Exclusive Root Trust - Requires that a client certificate chains to a root certificate contained in the caller-specified trusted issuer store. The certificate must also be issued by an issuer in the Trusted Issuers list
2.	Exclusive CA Trust - Requires that a client certificate chain to either an intermediate CA certificate or root certificate in the caller-specified trusted issuer store.

- /connect/mtls/token
- /connect/mtls/revocation
- /connect/mtls/introspect
- /connect/mtls/deviceauthorization

### 5.2 Links

- Example site with multiple authentication schemes: https://auth0.com/auth/login/
- Icons/svg's: https://iconscout.com/
- https://www.scottbrady91.com/OpenID-Connect/ASPNET-Core-using-Proof-Key-for-Code-Exchange-PKCE#pkce
- https://docs.microsoft.com/en-us/dotnet/api/system.identitymodel.tokens.jwt.jwtsecuritytokenhandler.inboundclaimtypemap?view=azure-dotnet
- https://docs.microsoft.com/en-us/dotnet/api/system.identitymodel.tokens.jwt.jwtsecuritytokenhandler.defaultinboundclaimtypemap?view=azure-dotnet
- https://docs.microsoft.com/en-us/dotnet/api/system.identitymodel.tokens.jwt.jwtsecuritytokenhandler.outboundclaimtypemap?view=azure-dotnet
- https://docs.microsoft.com/en-us/dotnet/api/system.identitymodel.tokens.jwt.jwtsecuritytokenhandler.defaultoutboundclaimtypemap?view=azure-dotnet
- https://github.com/aspnet/AuthSamples/tree/master/samples/DynamicSchemes
- https://docs.microsoft.com/en-us/azure/redis-cache/cache-configure
- [SAML 2.0 Integration with IdentityServer4](https://www.identityserver.com/articles/saml-20-integration-with-identityserver4/)
- https://github.com/RockSolidKnowledge/Samples.IdentityServer4.Saml2pIntegration/
- [IdentityServer4 - WS-Federation and SharePoint](https://www.identityserver.com/articles/identityserver4-ws-federation-and-sharepoint/)