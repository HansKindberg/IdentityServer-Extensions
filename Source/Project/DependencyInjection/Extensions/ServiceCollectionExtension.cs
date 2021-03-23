using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using HansKindberg.IdentityServer.Builder;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.Configuration.Extensions;
using HansKindberg.IdentityServer.Data;
using HansKindberg.IdentityServer.Data.Configuration;
using HansKindberg.IdentityServer.Data.Saml;
using HansKindberg.IdentityServer.Data.Transferring;
using HansKindberg.IdentityServer.Data.WsFederation;
using HansKindberg.IdentityServer.DataProtection.Configuration;
using HansKindberg.IdentityServer.Development;
using HansKindberg.IdentityServer.FeatureManagement;
using HansKindberg.IdentityServer.FeatureManagement.Extensions;
using HansKindberg.IdentityServer.Identity;
using HansKindberg.IdentityServer.Identity.Data;
using HansKindberg.IdentityServer.Logging.Configuration;
using HansKindberg.IdentityServer.Validation;
using HansKindberg.IdentityServer.Web.Authentication.Cookies.Extensions;
using HansKindberg.IdentityServer.Web.Configuration;
using HansKindberg.IdentityServer.Web.Mvc.Filters;
using HansKindberg.IdentityServer.Web.Mvc.Filters.Configuration;
using HansKindberg.Web.Authorization.DependencyInjection.Extensions;
using IdentityServer4;
using IdentityServer4.Configuration;
using IdentityServer4.EntityFramework.Interfaces;
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
using RegionOrebroLan;
using RegionOrebroLan.Configuration;
using RegionOrebroLan.Localization.DependencyInjection.Extensions;
using RegionOrebroLan.Web.Authentication.DependencyInjection.Extensions;
using Rsk.Saml.IdentityProvider.Storage.EntityFramework.Interfaces;
using Rsk.Saml.IdentityProvider.Storage.EntityFramework.Stores;
using Rsk.WsFederation.EntityFramework.DbContexts;
using Rsk.WsFederation.EntityFramework.Stores;

namespace HansKindberg.IdentityServer.DependencyInjection.Extensions
{
	[SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling")]
	public static class ServiceCollectionExtension
	{
		#region Methods

		public static IServiceCollection AddCertificateForwarding(this IServiceCollection services, IServiceConfiguration serviceConfiguration)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(serviceConfiguration == null)
				throw new ArgumentNullException(nameof(serviceConfiguration));

			var configurationSection = serviceConfiguration.Configuration.GetSection(ConfigurationKeys.CertificateForwardingPath);

			return services.AddCertificateForwarding(options =>
			{
				configurationSection.Bind(options);
			});
		}

		public static IServiceCollection AddDataDirectory(this IServiceCollection services, IServiceConfiguration serviceConfiguration)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(serviceConfiguration == null)
				throw new ArgumentNullException(nameof(serviceConfiguration));

			var options = new DataDirectoryOptions();

			serviceConfiguration.Configuration.GetSection(ConfigurationKeys.DataDirectoryPath).Bind(options);

			var path = options.Path;

			if(!Path.IsPathRooted(path))
				path = Path.Combine(serviceConfiguration.HostEnvironment.ContentRootPath, path);

			serviceConfiguration.ApplicationDomain.SetData(ConfigurationKeys.DataDirectoryPath, path);

			return services;
		}

		public static IServiceCollection AddDataProtection(this IServiceCollection services, IServiceConfiguration serviceConfiguration)
		{
			return services.AddFeature<ExtendedDataProtectionOptions>(ConfigurationKeys.DataProtectionPath, serviceConfiguration, true);
		}

		public static IServiceCollection AddDataTransfer(this IServiceCollection services)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			services.AddScoped<IDataExporter, DataExporter>();
			services.AddScoped<IDataImporter, DataImporter>();

			return services;
		}

		public static IServiceCollection AddDevelopment(this IServiceCollection services, IServiceConfiguration serviceConfiguration)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(serviceConfiguration == null)
				throw new ArgumentNullException(nameof(serviceConfiguration));

			var developmentStartup = new DevelopmentStartup(serviceConfiguration);
			developmentStartup.ConfigureServices(services);
			services.TryAddSingleton<IDevelopmentStartup>(developmentStartup);

			return services;
		}

		private static IServiceCollection AddFeature<T>(this IServiceCollection services, string configurationKey, IServiceConfiguration serviceConfiguration, bool bindConfigurationSection = false) where T : IServiceConfigurationOptions
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(serviceConfiguration == null)
				throw new ArgumentNullException(nameof(serviceConfiguration));

			var configurationSection = serviceConfiguration.Configuration.GetSection(configurationKey);
			var dynamicOptions = new DynamicOptions();
			configurationSection.Bind(dynamicOptions);

			// ReSharper disable InvertIf
			if(dynamicOptions.Type != null)
			{
				var configurationSections = new List<IConfigurationSection>
				{
					dynamicOptions.Options
				};

				if(bindConfigurationSection)
					configurationSections.Insert(0, configurationSection);

				((T)serviceConfiguration.InstanceFactory.Create(dynamicOptions.Type)).Add(serviceConfiguration, services, configurationSections.ToArray());
			}
			// ReSharper restore InvertIf

			return services;
		}

		public static IServiceCollection AddForwardedHeaders(this IServiceCollection services, IServiceConfiguration serviceConfiguration)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(serviceConfiguration == null)
				throw new ArgumentNullException(nameof(serviceConfiguration));

			return services.Configure<ForwardedHeadersOptions>(options =>
			{
				options.AllowedHosts.Clear();
				options.KnownNetworks.Clear();
				options.KnownProxies.Clear();

				var forwardedHeadersSection = serviceConfiguration.Configuration.GetSection(ConfigurationKeys.ForwardedHeadersPath);

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

		public static IServiceCollection AddIdentity(this IServiceCollection services, IServiceConfiguration serviceConfiguration)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(serviceConfiguration == null)
				throw new ArgumentNullException(nameof(serviceConfiguration));

			var dataOptions = serviceConfiguration.Data.Value;
			var connectionString = serviceConfiguration.Configuration.GetConnectionString(dataOptions.ConnectionStringName);
			var databaseProvider = dataOptions.Provider;

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

		public static IIdentityServerBuilder AddIdentityServer(this IServiceCollection services, IServiceConfiguration serviceConfiguration)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(serviceConfiguration == null)
				throw new ArgumentNullException(nameof(serviceConfiguration));

			var dataOptions = serviceConfiguration.Data.Value;
			var connectionString = serviceConfiguration.Configuration.GetConnectionString(dataOptions.ConnectionStringName);
			var databaseProvider = dataOptions.Provider;

			return databaseProvider switch
			{
				DatabaseProvider.Sqlite => services.AddIdentityServer<SqliteConfiguration, SqliteOperational, SqliteSamlConfiguration, SqliteWsFederationConfiguration>(optionsBuilder => optionsBuilder.UseSqlite(connectionString), serviceConfiguration),
				DatabaseProvider.SqlServer => services.AddIdentityServer<SqlServerConfiguration, SqlServerOperational, SqlServerSamlConfiguration, SqlServerWsFederationConfiguration>(optionsBuilder => optionsBuilder.UseSqlServer(connectionString), serviceConfiguration),
				_ => throw new InvalidOperationException($"The database-provider \"{databaseProvider}\" is not supported.")
			};
		}

		private static IIdentityServerBuilder AddIdentityServer<TConfiguration, TOperational, TSaml, TWsFederation>(this IServiceCollection services, Func<DbContextOptionsBuilder, DbContextOptionsBuilder> optionsBuilderFunction, IServiceConfiguration serviceConfiguration) where TConfiguration : DbContext, IConfigurationDbContext where TOperational : DbContext, IPersistedGrantDbContext where TSaml : SamlConfigurationContext where TWsFederation : WsFederationConfigurationContext
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(optionsBuilderFunction == null)
				throw new ArgumentNullException(nameof(optionsBuilderFunction));

			if(serviceConfiguration == null)
				throw new ArgumentNullException(nameof(serviceConfiguration));

			var identityServerOptions = services.ConfigureIdentityServer(serviceConfiguration);

			var identityServerBuilder = services.AddIdentityServer()
				.AddConfigurationStore<TConfiguration>(options =>
				{
					options.ConfigureDbContext = optionsBuilder => optionsBuilderFunction(optionsBuilder);

					serviceConfiguration.Configuration.GetSection($"{ConfigurationKeys.IdentityServerPath}:{nameof(ExtendedIdentityServerOptions.ConfigurationStore)}").Bind(options);
				})
				.AddConfigurationStoreCache()
				.AddJwtBearerClientAuthentication()
				.AddMutualTlsSecretValidators()
				.AddSecretValidator<X509IssuerSecretValidator>()
				.AddOperationalStore<TOperational>(options =>
				{
					options.ConfigureDbContext = optionsBuilder => optionsBuilderFunction(optionsBuilder);

					serviceConfiguration.Configuration.GetSection($"{ConfigurationKeys.IdentityServerPath}:{nameof(ExtendedIdentityServerOptions.OperationalStore)}").Bind(options);
				})
				.AddSigningCredential(serviceConfiguration.GetCertificate(identityServerOptions.SigningCertificate));

			foreach(var validationCertificate in identityServerOptions.ValidationCertificates)
			{
				identityServerBuilder.AddValidationKey(serviceConfiguration.GetCertificate(validationCertificate));
			}

			identityServerBuilder.AddIdentityServerPlugins<TSaml, TWsFederation>(optionsBuilderFunction, serviceConfiguration);

			return identityServerBuilder;
		}

		private static IIdentityServerBuilder AddIdentityServerPlugins<TSaml, TWsFederation>(this IIdentityServerBuilder identityServerBuilder, Func<DbContextOptionsBuilder, DbContextOptionsBuilder> optionsBuilderFunction, IServiceConfiguration serviceConfiguration) where TSaml : SamlConfigurationContext where TWsFederation : WsFederationConfigurationContext
		{
			if(identityServerBuilder == null)
				throw new ArgumentNullException(nameof(identityServerBuilder));

			if(optionsBuilderFunction == null)
				throw new ArgumentNullException(nameof(optionsBuilderFunction));

			if(serviceConfiguration == null)
				throw new ArgumentNullException(nameof(serviceConfiguration));

			if(serviceConfiguration.FeatureManager.IsEnabled(Feature.Saml))
			{
				identityServerBuilder.Services.AddDbContext<TSaml>(optionsBuilder => optionsBuilderFunction(optionsBuilder));
				identityServerBuilder.Services.AddScoped<ISamlConfigurationDbContext>(serviceProvider => serviceProvider.GetRequiredService<TSaml>());

				identityServerBuilder.AddSamlPlugin(options =>
					{
						serviceConfiguration.Configuration.GetSection($"{ConfigurationKeys.IdentityServerPath}:{nameof(ExtendedIdentityServerOptions.Saml)}").Bind(options);
					})
					.AddServiceProviderStore<ServiceProviderStore>();

				identityServerBuilder.Services.TryAddSingleton<ISamlPluginBuilder, SamlPluginBuilder>();
			}

			// ReSharper disable InvertIf
			if(serviceConfiguration.FeatureManager.IsEnabled(Feature.WsFederation))
			{
				identityServerBuilder.Services.AddDbContext<TWsFederation>(optionsBuilder => optionsBuilderFunction(optionsBuilder));
				identityServerBuilder.Services.AddScoped<IWsFederationConfigurationDbContext>(serviceProvider => serviceProvider.GetRequiredService<TWsFederation>());

				identityServerBuilder.AddWsFederationPlugin(options =>
					{
						serviceConfiguration.Configuration.GetSection($"{ConfigurationKeys.IdentityServerPath}:{nameof(ExtendedIdentityServerOptions.WsFederation)}").Bind(options);
					})
					.AddRelyingPartyStore<RelyingPartyStore>();

				identityServerBuilder.Services.TryAddSingleton<IWsFederationPluginBuilder, WsFederationPluginBuilder>();
			}
			// ReSharper restore InvertIf

			return identityServerBuilder;
		}

		public static IServiceCollection AddLoggingProvider(this IServiceCollection services, IServiceConfiguration serviceConfiguration)
		{
			return services.AddFeature<LoggingProviderOptions>(ConfigurationKeys.LoggingProviderPath, serviceConfiguration);
		}

		public static IServiceCollection AddRequestLocalization(this IServiceCollection services, IServiceConfiguration serviceConfiguration)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(serviceConfiguration == null)
				throw new ArgumentNullException(nameof(serviceConfiguration));

			services.Configure<RequestLocalizationOptions>(options =>
			{
				var requestLocalizationSection = serviceConfiguration.Configuration.GetSection(ConfigurationKeys.RequestLocalizationPath);

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
					options.RequestCultureProviders.Add((IRequestCultureProvider)serviceConfiguration.InstanceFactory.Create(type));
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

			var serviceConfiguration = new ServiceConfiguration(configuration, hostEnvironment);

			services.Configure<ExceptionHandlingOptions>(configuration.GetSection(ConfigurationKeys.ExceptionHandlingPath));
			services.Configure<SecurityHeaderOptions>(configuration.GetSection(ConfigurationKeys.SecurityHeadersPath));

			services.AddExtendedAuthorization(configuration);

			if(serviceConfiguration.FeatureManager.IsEnabled(Feature.DataDirectory))
				services.AddDataDirectory(serviceConfiguration);

			if(serviceConfiguration.FeatureManager.IsEnabled(Feature.CertificateForwarding))
				services.AddCertificateForwarding(serviceConfiguration);

			services.AddDataProtection(serviceConfiguration);
			services.AddDataTransfer();

			if(serviceConfiguration.FeatureManager.IsEnabled(Feature.Development))
				services.AddDevelopment(serviceConfiguration);

			services.AddFeatureManagement();

			if(serviceConfiguration.FeatureManager.IsEnabled(Feature.ForwardedHeaders))
				services.AddForwardedHeaders(serviceConfiguration);

			services.AddIdentity(serviceConfiguration);
			services.AddIdentityServer(serviceConfiguration);
			services.AddLoggingProvider(serviceConfiguration);
			services.AddPathBasedLocalization(configuration);
			services.AddRequestLocalization(serviceConfiguration);
			services.AddSameSiteCookiePolicy();

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

			success?.Invoke();

			return services;
		}

		private static ExtendedIdentityServerOptions ConfigureIdentityServer(this IServiceCollection services, IServiceConfiguration serviceConfiguration)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(serviceConfiguration == null)
				throw new ArgumentNullException(nameof(serviceConfiguration));

			var identityServerSection = serviceConfiguration.Configuration.GetSection(ConfigurationKeys.IdentityServerPath);

			var identityServerOptions = new ExtendedIdentityServerOptions();
			identityServerOptions.SetDefaults();
			identityServerSection.Bind(identityServerOptions);

			services.Configure<ExtendedIdentityServerOptions>(options =>
			{
				options.SetDefaults();
				identityServerSection.Bind(options);
			});

			services.Configure<IdentityServerOptions>(options =>
			{
				options.SetDefaults();
				identityServerSection.Bind(options);
			});

			return identityServerOptions;
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