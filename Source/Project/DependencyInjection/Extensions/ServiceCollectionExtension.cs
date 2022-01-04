using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Duende.IdentityServer;
using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.EntityFramework.Interfaces;
using Duende.IdentityServer.Stores;
using HansKindberg.IdentityServer.Builder;
using HansKindberg.IdentityServer.ComponentModel;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.Configuration.Extensions;
using HansKindberg.IdentityServer.Data;
using HansKindberg.IdentityServer.Data.Configuration;
using HansKindberg.IdentityServer.Data.Saml;
using HansKindberg.IdentityServer.Data.Transferring;
using HansKindberg.IdentityServer.Data.WsFederation;
using HansKindberg.IdentityServer.Development;
using HansKindberg.IdentityServer.Extensions;
using HansKindberg.IdentityServer.FeatureManagement;
using HansKindberg.IdentityServer.FeatureManagement.Extensions;
using HansKindberg.IdentityServer.Identity;
using HansKindberg.IdentityServer.Identity.Data;
using HansKindberg.IdentityServer.Json;
using HansKindberg.IdentityServer.Saml.Configuration;
using HansKindberg.IdentityServer.Saml.Configuration.Extensions;
using HansKindberg.IdentityServer.Saml.Generators;
using HansKindberg.IdentityServer.Saml.Routing;
using HansKindberg.IdentityServer.Saml.Routing.Configuration;
using HansKindberg.IdentityServer.Saml.Services;
using HansKindberg.IdentityServer.Validation;
using HansKindberg.IdentityServer.Web.Authentication;
using HansKindberg.IdentityServer.Web.Authentication.Cookies.Extensions;
using HansKindberg.IdentityServer.Web.Configuration;
using HansKindberg.IdentityServer.Web.Mvc.Filters;
using HansKindberg.IdentityServer.Web.Mvc.Filters.Configuration;
using HansKindberg.IdentityServer.WsFederation.Configuration.Extensions;
using HansKindberg.Web.Authorization.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using Newtonsoft.Json;
using RegionOrebroLan;
using RegionOrebroLan.Caching.Distributed.DependencyInjection.Extensions;
using RegionOrebroLan.ComponentModel;
using RegionOrebroLan.DataProtection.DependencyInjection.Extensions;
using RegionOrebroLan.Localization.DependencyInjection.Extensions;
using RegionOrebroLan.Web.Authentication.Cookies.DependencyInjection.Extensions;
using RegionOrebroLan.Web.Authentication.DependencyInjection.Extensions;
using RegionOrebroLan.Web.Authentication.OpenIdConnect.DependencyInjection.Extensions;
using Rsk.Saml.Configuration;
using Rsk.Saml.Generators;
using Rsk.Saml.IdentityProvider.Storage.EntityFramework.Interfaces;
using Rsk.Saml.IdentityProvider.Storage.EntityFramework.Stores;
using Rsk.Saml.Services;
using Rsk.Saml.Validation;
using Rsk.WsFederation.Configuration;
using Rsk.WsFederation.EntityFramework.DbContexts;
using Rsk.WsFederation.EntityFramework.Stores;
using Saml2SingleSignOnRequestValidator = HansKindberg.IdentityServer.Saml.Validation.Saml2SingleSignOnRequestValidator;
using SamlPersistedGrantService = HansKindberg.IdentityServer.Saml.Services.SamlPersistedGrantService;

namespace HansKindberg.IdentityServer.DependencyInjection.Extensions
{
	[SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling")]
	public static class ServiceCollectionExtension
	{
		#region Methods

		public static IServiceCollection AddCertificateForwarding(this IServiceCollection services, IServiceConfigurationBuilder serviceConfigurationBuilder)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(serviceConfigurationBuilder == null)
				throw new ArgumentNullException(nameof(serviceConfigurationBuilder));

			var configurationSection = serviceConfigurationBuilder.Configuration.GetSection(ConfigurationKeys.CertificateForwardingPath);

			return services.AddCertificateForwarding(options =>
			{
				configurationSection.Bind(options);
			});
		}

		private static IServiceCollection AddClaimsProvider(this IServiceCollection services, IConfiguration configuration)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			// TODO: fix claims-provider setup.

			//var claimsProviderSection = configuration.GetSection(ConfigurationKeys.ClaimsProviderPath);
			//var claimsProviderOptions = new DynamicOptions();
			//claimsProviderSection.Bind(claimsProviderOptions);

			//if(claimsProviderOptions.Type != null)
			//{
			//	var claimsProviderInterfaceType = typeof(IClaimsProvider);
			//	var claimsProviderType = TypeExtension.GetType("claims-provider", claimsProviderInterfaceType, claimsProviderOptions.Type);
			//	services.AddTransient(claimsProviderInterfaceType, claimsProviderType);
			//}
			//else
			//{
			//	services.AddSingleton<IClaimsProvider, EmptyClaimsProvider>();
			//}

			//services.Configure<ClaimsServiceClientOptions>(configuration.GetSection($"{ConfigurationKeys.ServiceClientsPath}:{nameof(ClaimsServiceClient)}"));
			//services.AddTransient<IClaimsServiceClient, ClaimsServiceClient>();

			return services;
		}

		public static IServiceCollection AddDataDirectory(this IServiceCollection services, IServiceConfigurationBuilder serviceConfigurationBuilder)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(serviceConfigurationBuilder == null)
				throw new ArgumentNullException(nameof(serviceConfigurationBuilder));

			var options = new DataDirectoryOptions();

			serviceConfigurationBuilder.Configuration.GetSection(ConfigurationKeys.DataDirectoryPath).Bind(options);

			var path = options.Path;

			if(!Path.IsPathRooted(path))
				path = Path.Combine(serviceConfigurationBuilder.HostEnvironment.ContentRootPath, path);

			serviceConfigurationBuilder.ApplicationDomain.SetData(ConfigurationKeys.DataDirectoryPath, path);

			return services;
		}

		public static IServiceCollection AddDataTransfer(this IServiceCollection services)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			services.AddScoped<IDataExporter, DataExporter>();
			services.AddScoped<IDataImporter, DataImporter>();

			return services;
		}

		public static IServiceCollection AddDevelopment(this IServiceCollection services, IServiceConfigurationBuilder serviceConfigurationBuilder)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(serviceConfigurationBuilder == null)
				throw new ArgumentNullException(nameof(serviceConfigurationBuilder));

			var developmentStartup = new DevelopmentStartup(serviceConfigurationBuilder);
			developmentStartup.ConfigureServices(services);
			services.TryAddSingleton<IDevelopmentStartup>(developmentStartup);

			return services;
		}

		public static IServiceCollection AddForwardedHeaders(this IServiceCollection services, IServiceConfigurationBuilder serviceConfigurationBuilder)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(serviceConfigurationBuilder == null)
				throw new ArgumentNullException(nameof(serviceConfigurationBuilder));

			return services.Configure<ForwardedHeadersOptions>(options =>
			{
				options.AllowedHosts.Clear();
				options.KnownNetworks.Clear();
				options.KnownProxies.Clear();

				var forwardedHeadersSection = serviceConfigurationBuilder.Configuration.GetSection(ConfigurationKeys.ForwardedHeadersPath);

				forwardedHeadersSection?.Bind(options);

				var extendedOptions = new ExtendedForwardedHeadersOptions();

				forwardedHeadersSection?.Bind(extendedOptions);

				foreach(var knownNetwork in extendedOptions.KnownNetworks)
				{
					options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse(knownNetwork.Prefix), knownNetwork.PrefixLength));
				}

				foreach(var proxy in extendedOptions.KnownProxies)
				{
					options.KnownProxies.Add(IPAddress.Parse(proxy));
				}
			});
		}

		public static IServiceCollection AddIdentity(this IServiceCollection services, IServiceConfigurationBuilder serviceConfigurationBuilder)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(serviceConfigurationBuilder == null)
				throw new ArgumentNullException(nameof(serviceConfigurationBuilder));

			var connectionString = serviceConfigurationBuilder.Configuration.GetConnectionString(serviceConfigurationBuilder.Data.ConnectionStringName);
			var databaseProvider = serviceConfigurationBuilder.Data.Provider;

			return databaseProvider switch
			{
				DatabaseProvider.Sqlite => services.AddIdentity<SqliteIdentity>(optionsBuilder => optionsBuilder.UseSqlite(connectionString)),
				DatabaseProvider.SqlServer => services.AddIdentity<SqlServerIdentity>(optionsBuilder => optionsBuilder.UseSqlServer(connectionString)),
				_ => throw new InvalidOperationException($"The database-provider \"{databaseProvider}\" is not supported.")
			};
		}

		private static IServiceCollection AddIdentity<T>(this IServiceCollection services, Func<DbContextOptionsBuilder, DbContextOptionsBuilder> optionsBuilderFunction) where T : IdentityContext
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(optionsBuilderFunction == null)
				throw new ArgumentNullException(nameof(optionsBuilderFunction));

			services.AddDbContext<T>(optionsBuilder => optionsBuilderFunction(optionsBuilder));

			services.AddScoped<IdentityContext>(serviceProvider => serviceProvider.GetRequiredService<T>());
			services.AddScoped<IIdentityFacade, IdentityFacade>();

			services.AddIdentity<User, Role>()
				.AddDefaultTokenProviders()
				.AddEntityFrameworkStores<T>()
				.AddUserManager<UserManager>()
				.AddUserStore<UserStore>();

			services.AddIdentityServerBuilder().AddAspNetIdentity<User>();

			services.ConfigureApplicationCookie(options =>
			{
				options.SetDefaults();
			});

			services.ConfigureExternalCookie(options =>
			{
				options.SetDefaults();
			});

			return services;
		}

		public static IIdentityServerBuilder AddIdentityServer(this IServiceCollection services, IServiceConfigurationBuilder serviceConfigurationBuilder)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(serviceConfigurationBuilder == null)
				throw new ArgumentNullException(nameof(serviceConfigurationBuilder));

			var connectionString = serviceConfigurationBuilder.Configuration.GetConnectionString(serviceConfigurationBuilder.Data.ConnectionStringName);
			var databaseProvider = serviceConfigurationBuilder.Data.Provider;

			return databaseProvider switch
			{
				DatabaseProvider.Sqlite => services.AddIdentityServer<SqliteConfiguration, SqliteOperational, SqliteSamlConfiguration, SqliteWsFederationConfiguration>(optionsBuilder => optionsBuilder.UseSqlite(connectionString), serviceConfigurationBuilder),
				DatabaseProvider.SqlServer => services.AddIdentityServer<SqlServerConfiguration, SqlServerOperational, SqlServerSamlConfiguration, SqlServerWsFederationConfiguration>(optionsBuilder => optionsBuilder.UseSqlServer(connectionString), serviceConfigurationBuilder),
				_ => throw new InvalidOperationException($"The database-provider \"{databaseProvider}\" is not supported.")
			};
		}

		private static IIdentityServerBuilder AddIdentityServer<TConfiguration, TOperational, TSaml, TWsFederation>(this IServiceCollection services, Func<DbContextOptionsBuilder, DbContextOptionsBuilder> optionsBuilderFunction, IServiceConfigurationBuilder serviceConfigurationBuilder) where TConfiguration : DbContext, IConfigurationDbContext where TOperational : DbContext, IPersistedGrantDbContext where TSaml : SamlConfigurationContext where TWsFederation : WsFederationConfigurationContext
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(optionsBuilderFunction == null)
				throw new ArgumentNullException(nameof(optionsBuilderFunction));

			if(serviceConfigurationBuilder == null)
				throw new ArgumentNullException(nameof(serviceConfigurationBuilder));

			var identityServerOptions = services.ConfigureIdentityServer(serviceConfigurationBuilder);

			var identityServerBuilder = services.AddIdentityServer()
				.AddConfigurationStore<TConfiguration>(options =>
				{
					options.ConfigureDbContext = optionsBuilder => optionsBuilderFunction(optionsBuilder);

					serviceConfigurationBuilder.Configuration.GetSection($"{ConfigurationKeys.IdentityServerPath}:{nameof(ExtendedIdentityServerOptions.ConfigurationStore)}").Bind(options);
				})
				.AddConfigurationStoreCache()
				.AddJwtBearerClientAuthentication()
				.AddMutualTlsSecretValidators()
				.AddSecretValidator<X509IssuerSecretValidator>()
				.AddOperationalStore<TOperational>(options =>
				{
					options.ConfigureDbContext = optionsBuilder => optionsBuilderFunction(optionsBuilder);

					serviceConfigurationBuilder.Configuration.GetSection($"{ConfigurationKeys.IdentityServerPath}:{nameof(ExtendedIdentityServerOptions.OperationalStore)}").Bind(options);
				});

			if(!serviceConfigurationBuilder.FeatureManager.IsEnabled(Feature.DynamicAuthenticationProviders))
			{
				// Remove some of the registered services.
				for(var i = services.Count - 1; i >= 0; i--)
				{
					var serviceDescriptor = services[i];

					if(serviceDescriptor.ServiceType == typeof(IIdentityProviderStore) && serviceDescriptor.Lifetime == ServiceLifetime.Transient)
						services.RemoveAt(i);
				}
			}

			if(identityServerOptions.SigningCertificate != null)
				identityServerBuilder.AddSigningCredential(serviceConfigurationBuilder.GetCertificate(identityServerOptions.SigningCertificate));

			foreach(var validationCertificate in identityServerOptions.ValidationCertificates)
			{
				identityServerBuilder.AddValidationKey(serviceConfigurationBuilder.GetCertificate(validationCertificate));
			}

			identityServerBuilder.AddIdentityServerPlugins<TSaml, TWsFederation>(optionsBuilderFunction, serviceConfigurationBuilder);

			return identityServerBuilder;
		}

		private static IIdentityServerBuilder AddIdentityServerPlugins<TSaml, TWsFederation>(this IIdentityServerBuilder identityServerBuilder, Func<DbContextOptionsBuilder, DbContextOptionsBuilder> optionsBuilderFunction, IServiceConfigurationBuilder serviceConfigurationBuilder) where TSaml : SamlConfigurationContext where TWsFederation : WsFederationConfigurationContext
		{
			if(identityServerBuilder == null)
				throw new ArgumentNullException(nameof(identityServerBuilder));

			if(optionsBuilderFunction == null)
				throw new ArgumentNullException(nameof(optionsBuilderFunction));

			if(serviceConfigurationBuilder == null)
				throw new ArgumentNullException(nameof(serviceConfigurationBuilder));

			if(serviceConfigurationBuilder.FeatureManager.IsEnabled(Feature.Saml))
			{
				var samlIdpOptionsSection = serviceConfigurationBuilder.Configuration.GetSection($"{ConfigurationKeys.IdentityServerPath}:{nameof(ExtendedIdentityServerOptions.Saml)}");
				var samlIdpOptions = new ExtendedSamlIdpOptions();
				samlIdpOptionsSection.Bind(samlIdpOptions);

				// This is for this extension-library. The Rsk-libraries uses SamlIdpOptions registered as a singleton.
				identityServerBuilder.Services.ConfigureIdentityServerSamlPlugin(samlIdpOptionsSection);

				identityServerBuilder.Services.AddDbContext<TSaml>(optionsBuilder => optionsBuilderFunction(optionsBuilder));
				identityServerBuilder.Services.AddScoped<ISamlConfigurationDbContext>(serviceProvider => serviceProvider.GetRequiredService<TSaml>());

				identityServerBuilder.AddSamlPlugin(options =>
					{
						// We need this because SamlIdpOptions is registered as a singleton.
						options.SetDefaults();
						samlIdpOptionsSection.Bind(options);
					})
					.AddServiceProviderStore<ServiceProviderStore>();

				identityServerBuilder.Services.Configure<ForceAuthenticationRouterOptions>(samlIdpOptions.ForceAuthentication.Options ?? (IConfiguration)new ConfigurationBuilder().Build());

				if(samlIdpOptions.ForceAuthentication.Router != null)
				{
					var forceAuthenticationRouterInterfaceType = typeof(IForceAuthenticationRouter);

					if(!forceAuthenticationRouterInterfaceType.IsAssignableFrom(samlIdpOptions.ForceAuthentication.Router))
						throw new InvalidOperationException($"The type {samlIdpOptions.ForceAuthentication.Router.FullName.ToStringRepresentation()} does not inherit from {forceAuthenticationRouterInterfaceType.FullName.ToStringRepresentation()}.");

					identityServerBuilder.Services.AddSingleton(forceAuthenticationRouterInterfaceType, samlIdpOptions.ForceAuthentication.Router);
				}
				else
				{
					identityServerBuilder.Services.AddSingleton<IForceAuthenticationRouter, NullForceAuthenticationRouter>();
				}

				identityServerBuilder.Services.RemoveAll<ISaml2SingleSignOnInteractionGenerator>();
				identityServerBuilder.Services.AddTransient<Rsk.Saml.DuendeIdentityServer.Generators.Saml2SingleSignOnInteractionGenerator>();
				identityServerBuilder.Services.AddTransient<ISaml2SingleSignOnInteractionGenerator, Saml2SingleSignOnInteractionGenerator>();

				identityServerBuilder.Services.RemoveAll<ISaml2SingleSignOnRequestValidator>();
				identityServerBuilder.Services.AddTransient<Rsk.Saml.Validation.Saml2SingleSignOnRequestValidator>();
				identityServerBuilder.Services.AddTransient<ISaml2SingleSignOnRequestValidator, Saml2SingleSignOnRequestValidator>();

				identityServerBuilder.Services.RemoveAll<ISamlInteractionService>();
				identityServerBuilder.Services.AddTransient<DefaultSamlInteractionService>();
				identityServerBuilder.Services.AddTransient<ExtendedSamlInteractionService>();
				identityServerBuilder.Services.AddTransient<IExtendedSamlInteractionService>(serviceProvider => serviceProvider.GetRequiredService<ExtendedSamlInteractionService>());
				identityServerBuilder.Services.AddTransient<ISamlInteractionService>(serviceProvider => serviceProvider.GetRequiredService<ExtendedSamlInteractionService>());

				identityServerBuilder.Services.RemoveAll<ISamlPersistedGrantService>();
				identityServerBuilder.Services.AddTransient<Rsk.Saml.Services.SamlPersistedGrantService>();
				identityServerBuilder.Services.AddTransient<ISamlPersistedGrantService, SamlPersistedGrantService>();

				identityServerBuilder.Services.TryAddSingleton<ISamlPluginBuilder, SamlPluginBuilder>();
			}

			// ReSharper disable InvertIf
			if(serviceConfigurationBuilder.FeatureManager.IsEnabled(Feature.WsFederation))
			{
				var wsFederationOptionsSection = serviceConfigurationBuilder.Configuration.GetSection($"{ConfigurationKeys.IdentityServerPath}:{nameof(ExtendedIdentityServerOptions.WsFederation)}");

				// This is for this extension-library. The Rsk-libraries uses WsFederationOptions registered as a singleton.
				identityServerBuilder.Services.ConfigureIdentityServerWsFederationPlugin(wsFederationOptionsSection);

				identityServerBuilder.Services.AddDbContext<TWsFederation>(optionsBuilder => optionsBuilderFunction(optionsBuilder));
				identityServerBuilder.Services.AddScoped<IWsFederationConfigurationDbContext>(serviceProvider => serviceProvider.GetRequiredService<TWsFederation>());

				identityServerBuilder.AddWsFederationPlugin(options =>
					{
						// We need this because WsFederationOptions is registered as a singleton.
						options.SetDefaults();
						wsFederationOptionsSection.Bind(options);
					})
					.AddRelyingPartyStore<RelyingPartyStore>();

				identityServerBuilder.Services.TryAddSingleton<IWsFederationPluginBuilder, WsFederationPluginBuilder>();
			}
			// ReSharper restore InvertIf

			return identityServerBuilder;
		}

		public static IServiceCollection AddRequestLocalization(this IServiceCollection services, IServiceConfigurationBuilder serviceConfigurationBuilder)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(serviceConfigurationBuilder == null)
				throw new ArgumentNullException(nameof(serviceConfigurationBuilder));

			services.Configure<RequestLocalizationOptions>(options =>
			{
				var requestLocalizationSection = serviceConfigurationBuilder.Configuration.GetSection(ConfigurationKeys.RequestLocalizationPath);

				options.RequestCultureProviders.Clear();
				options.SupportedCultures.Clear();
				options.SupportedUICultures.Clear();

				requestLocalizationSection.Bind(options);

				var defaultRequestCultureSection = requestLocalizationSection.GetSection(nameof(RequestLocalizationOptions.DefaultRequestCulture));

				var culture = defaultRequestCultureSection.GetSection(nameof(RequestCulture.Culture)).Value;
				var uiCulture = defaultRequestCultureSection.GetSection(nameof(RequestCulture.UICulture)).Value;

				culture ??= uiCulture;
				uiCulture ??= culture;

				if(culture != null)
					options.DefaultRequestCulture = new RequestCulture(culture, uiCulture);

				var requestCultureProviders = new List<string>();
				requestLocalizationSection.GetSection(nameof(RequestLocalizationOptions.RequestCultureProviders)).Bind(requestCultureProviders);
				foreach(var type in requestCultureProviders)
				{
					options.RequestCultureProviders.Add((IRequestCultureProvider)serviceConfigurationBuilder.InstanceFactory.Create(type));
				}
			});

			return services;
		}

		/// <summary>
		/// From https://github.com/IdentityServer/IdentityServer4/blob/main/src/IdentityServer4/host/Extensions/SameSiteHandlingExtensions.cs -> https://devblogs.microsoft.com/aspnet/upcoming-samesite-cookie-changes-in-asp-net-and-asp-net-core/
		/// </summary>
		public static IServiceCollection AddSameSiteCookiePolicy(this IServiceCollection services)
		{
			services.Configure<CookiePolicyOptions>(options =>
			{
				options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
				options.OnAppendCookie = cookieContext => CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
				options.OnDeleteCookie = cookieContext => CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
			});

			return services;
		}

		/// <summary>
		/// From https://github.com/IdentityServer/IdentityServer4/blob/main/src/IdentityServer4/host/Extensions/SameSiteHandlingExtensions.cs -> https://devblogs.microsoft.com/aspnet/upcoming-samesite-cookie-changes-in-asp-net-and-asp-net-core/
		/// </summary>
		private static void CheckSameSite(HttpContext httpContext, CookieOptions options)
		{
			if(httpContext == null)
				throw new ArgumentNullException(nameof(httpContext));

			if(options == null)
				throw new ArgumentNullException(nameof(options));

			if(options.SameSite != SameSiteMode.None)
				return;

			var userAgent = httpContext.Request.Headers["User-Agent"].ToString();

			if(!httpContext.Request.IsHttps || DisallowsSameSiteNone(userAgent))
				options.SameSite = SameSiteMode.Unspecified;
		}

		public static IServiceCollection Configure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment hostEnvironment, Action success = null)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			if(hostEnvironment == null)
				throw new ArgumentNullException(nameof(hostEnvironment));

			services.Configure<Options>(configuration);
			var rootOptions = new Options();
			configuration.Bind(rootOptions);

			if(!rootOptions.Enabled)
			{
				services.AddControllersWithViews();

				return services;
			}

			JsonConvert.DefaultSettings = () => new JsonSerializerSettings
			{
				Converters = new List<JsonConverter> { new JsonCertificateConverter() }
			};
			TypeDescriptor.AddAttributes(typeof(Type), new TypeConverterAttribute(typeof(TypeTypeConverter)));
			TypeDescriptor.AddAttributes(typeof(X509Certificate2), new TypeConverterAttribute(typeof(CertificateConverter)));

			var serviceConfiguration = new ServiceConfigurationBuilder(configuration, hostEnvironment);

			services.Configure<ExceptionHandlingOptions>(configuration.GetSection(ConfigurationKeys.ExceptionHandlingPath));
			services.Configure<SecurityHeaderOptions>(configuration.GetSection(ConfigurationKeys.SecurityHeadersPath));

			services.AddClaimsProvider(configuration);

			services.AddExtendedAuthorization(configuration);

			if(serviceConfiguration.FeatureManager.IsEnabled(Feature.DataDirectory))
				services.AddDataDirectory(serviceConfiguration);

			if(serviceConfiguration.FeatureManager.IsEnabled(Feature.CertificateForwarding))
				services.AddCertificateForwarding(serviceConfiguration);

			services.AddDataProtection(serviceConfiguration.CertificateResolver, configuration, hostEnvironment, serviceConfiguration.InstanceFactory);
			services.AddDataTransfer();

			if(serviceConfiguration.FeatureManager.IsEnabled(Feature.Development))
				services.AddDevelopment(serviceConfiguration);

			services.AddDistributedCache(configuration, hostEnvironment, serviceConfiguration.InstanceFactory);
			services.AddFeatureManagement();

			if(serviceConfiguration.FeatureManager.IsEnabled(Feature.ForwardedHeaders))
				services.AddForwardedHeaders(serviceConfiguration);

			services.AddIdentity(serviceConfiguration);
			services.AddIdentityServer(serviceConfiguration);
			services.AddOpenIdConnectClaimsRequest(configuration);
			services.AddPathBasedLocalization(configuration);
			services.AddRequestLocalization(serviceConfiguration);
			services.AddSameSiteCookiePolicy();
			services.AddTicketStore(configuration, hostEnvironment, serviceConfiguration.InstanceFactory);

			services.AddScoped<IFacade, Facade>();

			services.AddSingleton(AppDomain.CurrentDomain);
			services.AddSingleton<IApplicationDomain, ApplicationHost>();

			// This must come after AddPathBasedLocalization.
			services.AddControllersWithViews(options =>
				{
					options.Filters.Add<SecurityHeadersFilter>();
				})
				.AddDataAnnotationsLocalization()
				.AddViewLocalization();

			services.AddAuthentication(serviceConfiguration.CertificateResolver, configuration, serviceConfiguration.InstanceFactory, options =>
			{
				options.DefaultScheme = IdentityServerConstants.DefaultCookieAuthenticationScheme;
				options.DefaultSignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
				options.DefaultSignOutScheme = IdentityServerConstants.SignoutScheme;
			});

			services.AddScoped<IAuthenticationSchemeRetriever, AuthenticationSchemeRetriever>();

			success?.Invoke();

			return services;
		}

		private static ExtendedIdentityServerOptions ConfigureIdentityServer(this IServiceCollection services, IServiceConfigurationBuilder serviceConfigurationBuilder)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(serviceConfigurationBuilder == null)
				throw new ArgumentNullException(nameof(serviceConfigurationBuilder));

			var identityServerSection = serviceConfigurationBuilder.Configuration.GetSection(ConfigurationKeys.IdentityServerPath);

			var identityServerOptions = new ExtendedIdentityServerOptions();
			identityServerOptions.SetDefaults();
			identityServerSection.Bind(identityServerOptions);

			services.Configure<ExtendedIdentityServerOptions>(options =>
			{
				options.SetDefaults();
				options.Saml.SetDefaults();
				options.WsFederation.SetDefaults();
				options.BindKeyManagementSigningAlgorithms(identityServerSection);
				identityServerSection.Bind(options);
			});

			services.Configure<IdentityServerOptions>(options =>
			{
				options.SetDefaults();
				options.BindKeyManagementSigningAlgorithms(identityServerSection);
				identityServerSection.Bind(options);
			});

			return identityServerOptions;
		}

		private static void ConfigureIdentityServerSamlPlugin(this IServiceCollection services, IConfigurationSection samlIdpOptionsSection)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(samlIdpOptionsSection == null)
				throw new ArgumentNullException(nameof(samlIdpOptionsSection));

			services.Configure<ExtendedSamlIdpOptions>(options =>
			{
				options.SetDefaults();
				samlIdpOptionsSection.Bind(options);
			});

			services.Configure<SamlIdpOptions>(options =>
			{
				options.SetDefaults();
				samlIdpOptionsSection.Bind(options);
			});
		}

		private static void ConfigureIdentityServerWsFederationPlugin(this IServiceCollection services, IConfigurationSection wsFederationOptionsSection)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(wsFederationOptionsSection == null)
				throw new ArgumentNullException(nameof(wsFederationOptionsSection));

			services.Configure<WsFederationOptions>(options =>
			{
				options.SetDefaults();
				wsFederationOptionsSection.Bind(options);
			});
		}

		/// <summary>
		/// From https://github.com/IdentityServer/IdentityServer4/blob/main/src/IdentityServer4/host/Extensions/SameSiteHandlingExtensions.cs -> https://devblogs.microsoft.com/aspnet/upcoming-samesite-cookie-changes-in-asp-net-and-asp-net-core/
		/// </summary>
		[SuppressMessage("Globalization", "CA1307:Specify StringComparison")]
		private static bool DisallowsSameSiteNone(string userAgent)
		{
			if(userAgent == null)
				throw new ArgumentNullException(nameof(userAgent));

			if(userAgent.Contains("CPU iPhone OS 12") || userAgent.Contains("iPad; CPU OS 12"))
				return true;

			if(userAgent.Contains("Macintosh; Intel Mac OS X 10_14") && userAgent.Contains("Version/") && userAgent.Contains("Safari"))
				return true;

			// ReSharper disable ConvertIfStatementToReturnStatement
			if(userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6"))
				return true;
			// ReSharper restore ConvertIfStatementToReturnStatement

			return false;
		}

		#endregion
	}
}