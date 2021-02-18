using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Identity;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RegionOrebroLan.Logging.Extensions;
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

		#endregion

		#region Constructors

		public DataExporter(IConfigurationDbContext configurationDatabaseContext, IIdentityFacade identityFacade, ILoggerFactory loggerFactory)
		{
			this.ConfigurationDatabaseContext = configurationDatabaseContext ?? throw new ArgumentNullException(nameof(configurationDatabaseContext));
			this.IdentityFacade = identityFacade ?? throw new ArgumentNullException(nameof(identityFacade));
			this.Logger = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))).CreateLogger(this.GetType());
		}

		#endregion

		#region Properties

		protected internal virtual IConfigurationDbContext ConfigurationDatabaseContext { get; }
		protected internal virtual string DefaultPassword => _defaultPassword;
		public virtual IEnumerable<Type> ExportableTypes => this._exportableTypes ??= this.IdentityServerTypes.Concat(this.IdentityTypes).ToArray();
		protected internal virtual IIdentityFacade IdentityFacade { get; }
		protected internal virtual IEnumerable<Type> IdentityServerTypes => _identityServerTypes;
		protected internal virtual IEnumerable<Type> IdentityTypes => _identityTypes;
		protected internal virtual ILogger Logger { get; }

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

		#endregion
	}
}