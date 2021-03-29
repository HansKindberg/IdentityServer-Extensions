using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.FeatureManagement;
using HansKindberg.IdentityServer.FeatureManagement.Extensions;
using HansKindberg.IdentityServer.Identity;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using RegionOrebroLan.Logging.Extensions;
using Rsk.Saml.IdentityProvider.Storage.EntityFramework.Interfaces;
using Rsk.Saml.IdentityProvider.Storage.EntityFramework.Mappers;
using Rsk.WsFederation.EntityFramework.DbContexts;
using Rsk.WsFederation.EntityFramework.Mappers;
using Rsk.WsFederation.Models;
using ServiceProvider = Rsk.Saml.Models.ServiceProvider;
using UserLoginModel = HansKindberg.IdentityServer.Identity.Models.UserLogin;
using UserModel = HansKindberg.IdentityServer.Identity.Models.User;

namespace HansKindberg.IdentityServer.Data.Transferring
{
	/// <inheritdoc />
	public class DataExporter : IDataExporter
	{
		#region Fields

		private const string _defaultPassword = "?";
		private IEnumerable<Type> _exportableTypes;

		private static readonly IEnumerable<Type> _identityServerTypes = new[]
		{
			typeof(ApiResource),
			typeof(ApiScope),
			typeof(Client),
			typeof(IdentityResource)
		};

		private static readonly IEnumerable<Type> _identityTypes = new[]
		{
			typeof(UserLoginModel),
			typeof(UserModel)
		};

		private static readonly IEnumerable<Type> _samlPluginTypes = new[]
		{
			typeof(ServiceProvider)
		};

		private static readonly IEnumerable<Type> _wsFederationPluginTypes = new[]
		{
			typeof(RelyingParty)
		};

		#endregion

		#region Constructors

		public DataExporter(IConfigurationDbContext configurationDatabaseContext, IFeatureManager featureManager, IIdentityFacade identityFacade, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
		{
			this.ConfigurationDatabaseContext = configurationDatabaseContext ?? throw new ArgumentNullException(nameof(configurationDatabaseContext));
			this.FeatureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
			this.IdentityFacade = identityFacade ?? throw new ArgumentNullException(nameof(identityFacade));
			this.Logger = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))).CreateLogger(this.GetType());
			this.ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		#endregion

		#region Properties

		protected internal virtual IConfigurationDbContext ConfigurationDatabaseContext { get; }
		protected internal virtual string DefaultPassword => _defaultPassword;

		public virtual IEnumerable<Type> ExportableTypes
		{
			get
			{
				// ReSharper disable InvertIf
				if(this._exportableTypes == null)
				{
					var exportableTypes = this.IdentityServerTypes.Concat(this.IdentityTypes).ToArray();

					if(this.FeatureManager.IsEnabled(Feature.Saml))
						exportableTypes = exportableTypes.Concat(this.SamlPluginTypes).ToArray();

					if(this.FeatureManager.IsEnabled(Feature.WsFederation))
						exportableTypes = exportableTypes.Concat(this.WsFederationPluginTypes).ToArray();

					this._exportableTypes = exportableTypes;
				}
				// ReSharper restore InvertIf

				return this._exportableTypes;
			}
		}

		protected internal virtual IFeatureManager FeatureManager { get; }
		protected internal virtual IIdentityFacade IdentityFacade { get; }
		protected internal virtual IEnumerable<Type> IdentityServerTypes => _identityServerTypes;
		protected internal virtual IEnumerable<Type> IdentityTypes => _identityTypes;
		protected internal virtual ILogger Logger { get; }
		protected internal virtual IEnumerable<Type> SamlPluginTypes => _samlPluginTypes;
		protected internal virtual IServiceProvider ServiceProvider { get; }
		protected internal virtual IEnumerable<Type> WsFederationPluginTypes => _wsFederationPluginTypes;

		#endregion

		#region Methods

		protected internal virtual async Task ExportApiResourcesAsync(IDataExportResult result)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			await Task.CompletedTask;

			var apiResources = this.ConfigurationDatabaseContext.ApiResources
				.Include(apiResource => apiResource.Properties)
				.Include(apiResource => apiResource.Scopes)
				.Include(apiResource => apiResource.Secrets)
				.Include(apiResource => apiResource.UserClaims);

			result.Instances.Add(typeof(ApiResource), apiResources.Select(apiResource => apiResource.ToModel()));
		}

		protected internal virtual async Task ExportApiScopesAsync(IDataExportResult result)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			await Task.CompletedTask;

			var apiScopes = this.ConfigurationDatabaseContext.ApiScopes
				.Include(apiScope => apiScope.Properties)
				.Include(apiScope => apiScope.UserClaims);

			result.Instances.Add(typeof(ApiScope), apiScopes.Select(apiScope => apiScope.ToModel()));
		}

		public virtual async Task<IDataExportResult> ExportAsync(IEnumerable<Type> types)
		{
			types = (types ?? Enumerable.Empty<Type>()).ToArray();
			var exportableTypes = this.ExportableTypes.ToArray();
			var unexportableTypes = types.Where(type => !exportableTypes.Contains(type)).ToArray();
			types = types.Where(type => exportableTypes.Contains(type)).ToArray();

			if(unexportableTypes.Any())
				this.Logger.LogWarningIfEnabled($"The following types are not exportable and will be excluded: {string.Join(", ", (IEnumerable<Type>)unexportableTypes)}");

			var result = new DataExportResult();

			await this.ExportIdentityServerPluginTypesAsync(result, types);
			await this.ExportIdentityServerTypesAsync(result, types);
			await this.ExportIdentityTypesAsync(result, types);

			return result;
		}

		protected internal virtual async Task ExportClientsAsync(IDataExportResult result)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			await Task.CompletedTask;

			var clients = this.ConfigurationDatabaseContext.Clients
				.Include(client => client.AllowedCorsOrigins)
				.Include(client => client.AllowedGrantTypes)
				.Include(client => client.AllowedScopes)
				.Include(client => client.Claims)
				.Include(client => client.ClientSecrets)
				.Include(client => client.IdentityProviderRestrictions)
				.Include(client => client.PostLogoutRedirectUris)
				.Include(client => client.Properties)
				.Include(client => client.RedirectUris);

			result.Instances.Add(typeof(Client), clients.Select(client => client.ToModel()));
		}

		protected internal virtual async Task ExportIdentityResourcesAsync(IDataExportResult result)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			await Task.CompletedTask;

			var identityResources = this.ConfigurationDatabaseContext.IdentityResources
				.Include(identityResource => identityResource.Properties)
				.Include(identityResource => identityResource.UserClaims);

			result.Instances.Add(typeof(IdentityResource), identityResources.Select(identityResource => identityResource.ToModel()));
		}

		protected internal virtual async Task ExportIdentityServerPluginTypesAsync(IDataExportResult result, IEnumerable<Type> types)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			await Task.CompletedTask;

			types = (types ?? Enumerable.Empty<Type>()).ToArray();

			if(this.FeatureManager.IsEnabled(Feature.Saml) && types.Contains(typeof(ServiceProvider)))
				await this.ExportServiceProvidersAsync(result);

			if(this.FeatureManager.IsEnabled(Feature.WsFederation) && types.Contains(typeof(RelyingParty)))
				await this.ExportRelyingPartiesAsync(result);
		}

		protected internal virtual async Task ExportIdentityServerTypesAsync(IDataExportResult result, IEnumerable<Type> types)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			await Task.CompletedTask;

			types = (types ?? Enumerable.Empty<Type>()).ToArray();

			if(types.Contains(typeof(ApiResource)))
				await this.ExportApiResourcesAsync(result);

			if(types.Contains(typeof(ApiScope)))
				await this.ExportApiScopesAsync(result);

			if(types.Contains(typeof(Client)))
				await this.ExportClientsAsync(result);

			if(types.Contains(typeof(IdentityResource)))
				await this.ExportIdentityResourcesAsync(result);
		}

		protected internal virtual async Task ExportIdentityTypesAsync(IDataExportResult result, IEnumerable<Type> types)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			await Task.CompletedTask;

			types = (types ?? Enumerable.Empty<Type>()).ToArray();

			if(types.Contains(typeof(UserLoginModel)))
				result.Instances.Add(typeof(UserLoginModel), this.IdentityFacade.DatabaseContext.UserLogins.Select(userLogin => new UserLoginModel {Id = userLogin.UserId, Provider = userLogin.LoginProvider, UserIdentifier = userLogin.ProviderKey}).ToArray());

			if(types.Contains(typeof(UserModel)))
			{
				var users = this.IdentityFacade.Users.Where(user => user.PasswordHash != null);

				result.Instances.Add(typeof(UserModel), users.Select(userEntity => new UserModel {Email = userEntity.Email, Id = userEntity.Id, Password = this.DefaultPassword, UserName = userEntity.UserName}).ToArray());
			}
		}

		protected internal virtual async Task ExportRelyingPartiesAsync(IDataExportResult result)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			await Task.CompletedTask;

			using(var serviceScope = this.ServiceProvider.CreateScope())
			{
				var wsFederationDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<IWsFederationConfigurationDbContext>();

				var relyingParties = wsFederationDatabaseContext.RelyingParties
					.Include(relyingParty => relyingParty.ClaimMapping);

				result.Instances.Add(typeof(RelyingParty), relyingParties.Select(relyingParty => relyingParty.ToModel()).ToArray());
			}
		}

		protected internal virtual async Task ExportServiceProvidersAsync(IDataExportResult result)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			await Task.CompletedTask;

			using(var serviceScope = this.ServiceProvider.CreateScope())
			{
				var samlDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<ISamlConfigurationDbContext>();

				var serviceProviders = samlDatabaseContext.ServiceProviders
					.Include(serviceProvider => serviceProvider.AssertionConsumerServices)
					.Include(serviceProvider => serviceProvider.ClaimsMapping)
					.Include(serviceProvider => serviceProvider.SigningCertificates)
					.Include(serviceProvider => serviceProvider.SingleLogoutServices);

				result.Instances.Add(typeof(ServiceProvider), serviceProviders.Select(serviceProvider => serviceProvider.ToModel()).ToArray());
			}
		}

		#endregion
	}
}