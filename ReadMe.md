# IdentityServer-Extensions

Additions and extensions for [IdentityServer](https://github.com/DuendeSoftware/IdentityServer/).

[![NuGet](https://img.shields.io/nuget/v/HansKindberg.IdentityServer.svg?label=NuGet)](https://www.nuget.org/packages/HansKindberg.IdentityServer)

**License required for production**
- https://duendesoftware.com/products/identityserver#pricing

Additions/extensions to be able to setup a **configurable**, **globalizable** and **localizable** implementation of [IdentityServer](https://github.com/DuendeSoftware/IdentityServer/).

The idea is to setup an IdentityServer-implementation like the included [implementation](/Source/Implementations/Identity-Server/Application) and configure continous-release with substitution/transforms for Web.config & appsettings.json or to copy the sample and use it as a template. The copy can then bee changed regarding configuration, style and translations.

IdentityServer:

- Documentation: https://docs.duendesoftware.com/
- GitHub: https://github.com/DuendeSoftware/IdentityServer/
- NuGet: https://www.nuget.org/packages/Duende.IdentityServer/

## 1 Features

- [Feature.cs](/Source/Project/FeatureManagement/Feature.cs)
- [Example](/Source/Implementations/Identity-Server/Application/appsettings.json#L397)

## 2 Configuration

### 2.1 Features
See above.

### 2.2 Globalization

- [Example](/Source/Implementations/Identity-Server/Application/appsettings.json#L495)

### 2.3 Localization

- [Path-based localization](https://github.com/RegionOrebroLan/.NET-Localization-Extensions#1-path-based-localization)
- [Example](/Source/Implementations/Identity-Server/Application/Resources)

### 2.4 Authentication-schemes

#### 2.4.1 Example

- [Authentication](/Source/Implementations/Identity-Server/Application/appsettings.json#L64)
- [Scheme-registrations](/Source/Implementations/Identity-Server/Application/appsettings.json#L113)

Solution behind it:

- [.NET-Web-Authentication-Extensions](https://github.com/RegionOrebroLan/.NET-Web-Authentication-Extensions)
- [ActiveLogin-Authentication-Extensions](https://github.com/RegionOrebroLan/ActiveLogin-Authentication-Extensions)

##### 2.4.2 Providers

##### 2.4.2.1 Facebook

- [Facebook external login setup in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/facebook-logins/)

##### 2.4.2.2 Google

- [Google external login setup in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins/)
- https://console.developers.google.com/
- [Example](/Source/Implementations/Identity-Server/Application/appsettings.json#L171)

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

## 3 Features and configuration depending on Duende.IdentityServer license

Some features/configuration depends on the Duende.IdentityServer license, https://duendesoftware.com/products/identityserver#pricing.

### 3.1 Automatic key management

If your Duende.IdentityServer license doesn't include the **Automatic key management**-feature you should disable it in appsettings.json:

	{
		"IdentityServer": {
			"KeyManagement": {
				"Enabled": false
			},
		}
	}

Configure for signing- and validation-certificates instead:

#### 3.1.1 Production-example

	{
		"IdentityServer": {
			"SigningCertificate": {
				"Options": {
					"Path": "CERT:\\LocalMachine\\My\\CN=IdentityServer-Signing-4"
				},
				"Type": "RegionOrebroLan.Security.Cryptography.Configuration.StoreResolverOptions, RegionOrebroLan"
			},
			"ValidationCertificates": [
				{
					"Options": {
						"Path": "CERT:\\LocalMachine\\My\\CN=IdentityServer-Signing-1"
					},
					"Type": "RegionOrebroLan.Security.Cryptography.Configuration.StoreResolverOptions, RegionOrebroLan"
				},
				{
					"Options": {
						"Path": "CERT:\\LocalMachine\\My\\CN=IdentityServer-Signing-2"
					},
					"Type": "RegionOrebroLan.Security.Cryptography.Configuration.StoreResolverOptions, RegionOrebroLan"
				},
				{
					"Options": {
						"Path": "CERT:\\LocalMachine\\My\\CN=IdentityServer-Signing-3"
					},
					"Type": "RegionOrebroLan.Security.Cryptography.Configuration.StoreResolverOptions, RegionOrebroLan"
				}
			]
		}
	}

#### 3.1.2 Development-example

	{
		"IdentityServer": {
			"SigningCertificate": {
				"Options": {
					"Password": "password",
					"Path": "Data/Development-Signing-Certificate-4.pfx"
				},
				"Type": "RegionOrebroLan.Security.Cryptography.Configuration.FileResolverOptions, RegionOrebroLan"
			},
			"ValidationCertificates": [
				{
					"Options": {
						"Password": "password",
						"Path": "Data/Development-Signing-Certificate-1.pfx"
					},
					"Type": "RegionOrebroLan.Security.Cryptography.Configuration.FileResolverOptions, RegionOrebroLan"
				},
				{
					"Options": {
						"Password": "password",
						"Path": "Data/Development-Signing-Certificate-2.pfx"
					},
					"Type": "RegionOrebroLan.Security.Cryptography.Configuration.FileResolverOptions, RegionOrebroLan"
				},
				{
					"Options": {
						"Password": "password",
						"Path": "Data/Development-Signing-Certificate-3.pfx"
					},
					"Type": "RegionOrebroLan.Security.Cryptography.Configuration.FileResolverOptions, RegionOrebroLan"
				}
			]
		}
	}

### 3.2 Dynamic Authentication Providers 

If your Duende.IdentityServer license doesn't include the **Dynamic Authentication Providers**-feature you should disable it in appsettings.json:

	{
		"FeatureManagement": {
			...
			"DynamicAuthenticationProviders": false,
			...
		}
	}

or remove the line:

	{
		"FeatureManagement": {
			...
			// Remove the line below
			"DynamicAuthenticationProviders": true,
			...
		}
	}

## 4 Development

### 4.1 Migrations

We might want to create/recreate migrations. If we can accept data-loss we can recreate the migrations otherwhise we will have to update them.

Copy all the commands below and run them in the Package Manager Console for the affected database-context.

If you want more migration-information you can add the -Verbose parameter:

	Add-Migration TheMigration -Context TheDatabaseContext -OutputDir Data/Migrations -Project Project -Verbose;

**Important!** Before running the commands below you need to ensure the "Project"-project is set as startup-project. 

#### 4.1.1 Configuration

##### 4.1.1.1 Create migrations

	Write-Host "Removing migrations...";
	Remove-Migration -Context SqliteConfiguration -Force -Project Project;
	Remove-Migration -Context SqlServerConfiguration -Force -Project Project;
	Write-Host "Removing current migrations-directory...";
	Remove-Item "Project\Data\Migrations\Configuration" -ErrorAction Ignore -Force -Recurse;
	Write-Host "Creating migrations...";
	Add-Migration SqliteConfigurationMigration -Context SqliteConfiguration -OutputDir Data/Migrations/Configuration/Sqlite -Project Project;
	Add-Migration SqlServerConfigurationMigration -Context SqlServerConfiguration -OutputDir Data/Migrations/Configuration/SqlServer -Project Project;
	Write-Host "Finnished";

##### 4.1.1.2 Update migrations

	Write-Host "Updating migrations...";
	Add-Migration SqliteConfigurationMigrationUpdate -Context SqliteConfiguration -OutputDir Data/Migrations/Configuration/Sqlite -Project Project;
	Add-Migration SqlServerConfigurationMigrationUpdate -Context SqlServerConfiguration -OutputDir Data/Migrations/Configuration/SqlServer -Project Project;
	Write-Host "Finnished";

#### 4.1.2 Identity

##### 4.1.2.1 Create migrations

	Write-Host "Removing migrations...";
	Remove-Migration -Context SqliteIdentity -Force -Project Project;
	Remove-Migration -Context SqlServerIdentity -Force -Project Project;
	Write-Host "Removing current migrations-directory...";
	Remove-Item "Project\Identity\Data\Migrations" -ErrorAction Ignore -Force -Recurse;
	Write-Host "Creating migrations...";
	Add-Migration SqliteIdentityMigration -Context SqliteIdentity -OutputDir Identity/Data/Migrations/Sqlite -Project Project;
	Add-Migration SqlServerIdentityMigration -Context SqlServerIdentity -OutputDir Identity/Data/Migrations/SqlServer -Project Project;
	Write-Host "Finnished";

##### 4.1.2.2 Update migrations

	Write-Host "Updating migrations...";
	Add-Migration SqliteIdentityMigrationUpdate -Context SqliteIdentity -OutputDir Identity/Data/Migrations/Sqlite -Project Project;
	Add-Migration SqlServerIdentityMigrationUpdate -Context SqlServerIdentity -OutputDir Identity/Data/Migrations/SqlServer -Project Project;
	Write-Host "Finnished";

#### 4.1.3 Operational (PersistedGrant)

##### 4.1.3.1 Create migrations

	Write-Host "Removing migrations...";
	Remove-Migration -Context SqliteOperational -Force -Project Project;
	Remove-Migration -Context SqlServerOperational -Force -Project Project;
	Write-Host "Removing current migrations-directory...";
	Remove-Item "Project\Data\Migrations\Operational" -ErrorAction Ignore -Force -Recurse;
	Write-Host "Creating migrations...";
	Add-Migration SqliteOperationalMigration -Context SqliteOperational -OutputDir Data/Migrations/Operational/Sqlite -Project Project;
	Add-Migration SqlServerOperationalMigration -Context SqlServerOperational -OutputDir Data/Migrations/Operational/SqlServer -Project Project;
	Write-Host "Finnished";

##### 4.1.3.2 Update migrations

	Write-Host "Updating migrations...";
	Add-Migration SqliteOperationalMigrationUpdate -Context SqliteOperational -OutputDir Data/Migrations/Operational/Sqlite -Project Project;
	Add-Migration SqlServerOperationalMigrationUpdate -Context SqlServerOperational -OutputDir Data/Migrations/Operational/SqlServer -Project Project;
	Write-Host "Finnished";

#### 4.1.4 Plugins

##### 4.1.4.1 SAML

###### 4.1.4.1.1 Create migrations

	Write-Host "Removing migrations...";
	Remove-Migration -Context SqliteSamlConfiguration -Force -Project Project;
	Remove-Migration -Context SqlServerSamlConfiguration -Force -Project Project;
	Write-Host "Removing current migrations-directory...";
	Remove-Item "Project\Data\Saml\Migrations" -ErrorAction Ignore -Force -Recurse;
	Write-Host "Creating migrations...";
	Add-Migration SqliteSamlConfigurationMigration -Context SqliteSamlConfiguration -OutputDir Data/Saml/Migrations/Sqlite -Project Project;
	Add-Migration SqlServerSamlConfigurationMigration -Context SqlServerSamlConfiguration -OutputDir Data/Saml/Migrations/SqlServer -Project Project;
	Write-Host "Finnished";

###### 4.1.4.1.2 Update migrations

	Write-Host "Updating migrations...";
	Add-Migration SqliteSamlConfigurationMigrationUpdate -Context SqliteSamlConfiguration -OutputDir Data/Saml/Migrations/Sqlite -Project Project;
	Add-Migration SqlServerSamlConfigurationMigrationUpdate -Context SqlServerSamlConfiguration -OutputDir Data/Saml/Migrations/SqlServer -Project Project;
	Write-Host "Finnished";

##### 4.1.4.2 WsFederation

###### 4.1.4.2.1 Create migrations

	Write-Host "Removing migrations...";
	Remove-Migration -Context SqliteWsFederationConfiguration -Force -Project Project;
	Remove-Migration -Context SqlServerWsFederationConfiguration -Force -Project Project;
	Write-Host "Removing current migrations-directory...";
	Remove-Item "Project\Data\WsFederation\Migrations" -ErrorAction Ignore -Force -Recurse;
	Write-Host "Creating migrations...";
	Add-Migration SqliteWsFederationConfigurationMigration -Context SqliteWsFederationConfiguration -OutputDir Data/WsFederation/Migrations/Sqlite -Project Project;
	Add-Migration SqlServerWsFederationConfigurationMigration -Context SqlServerWsFederationConfiguration -OutputDir Data/WsFederation/Migrations/SqlServer -Project Project;
	Write-Host "Finnished";

###### 4.1.4.2.2 Update migrations

	Write-Host "Updating migrations...";
	Add-Migration SqliteWsFederationConfigurationMigrationUpdate -Context SqliteWsFederationConfiguration -OutputDir Data/WsFederation/Migrations/Sqlite -Project Project;
	Add-Migration SqlServerWsFederationConfigurationMigrationUpdate -Context SqlServerWsFederationConfiguration -OutputDir Data/WsFederation/Migrations/SqlServer -Project Project;
	Write-Host "Finnished";

## 5 Technicalities

### 5.1 Development-environment
This solution is built on:

- Visual Studio Enterprise 2019
- Windows 10

### 5.2 NuGet
The solution uses the following NuGet-packages:

- [Project](/Source/Project/Project.csproj#L26)

### 5.3 Documentation

- [IdentityServer](https://docs.duendesoftware.com/)
- [SAML 2.0 Integration with IdentityServer](https://www.identityserver.com/articles/saml-20-integration-with-identityserver4/)
- [IdentityServer Saml 2.0 Component](https://www.identityserver.com/documentation/saml2p/)

### 5.4 Examples

- [IdentityServer samples](https://github.com/RockSolidKnowledge/)

### 5.5 Information

- [IdentityServer.com](https://www.identityserver.com/)

#### 5.5.1 Products

- [IdentityServer products](https://www.identityserver.com/products/)

## 6 Notes

Various saved notes that appeared during development.

### 6.1 Mutual TLS (client-certificate-authentication)

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

### 6.2 Links

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