using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Extensions;
using HansKindberg.IdentityServer.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HansKindberg.IdentityServer.Data.Transferring.Internal
{
	public class IdentityImporter : ContextImporter
	{
		#region Constructors

		public IdentityImporter(IIdentityFacade facade, ILoggerFactory loggerFactory) : base(loggerFactory)
		{
			this.Facade = facade ?? throw new ArgumentNullException(nameof(facade));
		}

		#endregion

		#region Properties

		protected internal virtual IIdentityFacade Facade { get; }

		#endregion

		#region Methods

		public override async Task<int> CommitAsync()
		{
			return await this.Facade.DatabaseContext.SaveChangesAsync();
		}

		protected internal override async Task<IEnumerable<IPartialImporter>> CreateImportersAsync()
		{
			var importers = new List<IPartialImporter>
			{
				new UserLoginImporter(this.Facade, this.LoggerFactory),
				new UserImporter(this.Facade, this.LoggerFactory)
			};

			return await Task.FromResult(importers.ToArray());
		}

		public override async Task ImportAsync(IConfiguration configuration, ImportOptions options, IDataImportResult result)
		{
			await this.ValidateModelsAsync(configuration, result);

			await base.ImportAsync(configuration, options, result);
		}

		protected internal virtual async Task ValidateModelsAsync(IConfiguration configuration, IDataImportResult result)
		{
			if(configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			if(result == null)
				throw new ArgumentNullException(nameof(result));

			var userLoginImporter = this.Importers.OfType<UserLoginImporter>().FirstOrDefault();
			var userImporter = this.Importers.OfType<UserImporter>().FirstOrDefault();

			if(userLoginImporter == null)
				return;

			if(userImporter == null)
				return;

			var userLoginModels = await userLoginImporter.GetModelsAsync(configuration);
			if(!userLoginModels.Any())
				return;

			var userModels = await userImporter.GetModelsAsync(configuration);
			if(!userModels.Any())
				return;

			var conflictingIdentifiers = userLoginModels.Select(userLogin => userLogin.Id).Intersect(userModels.Select(userModel => userModel.Id), StringComparer.OrdinalIgnoreCase).Where(item => item != null).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

			if(!conflictingIdentifiers.Any())
				return;

			result.Errors.Add($"The {userLoginImporter.ConfigurationKey}-section and the {userImporter.ConfigurationKey}-section have conflicting user-ids: {string.Join(", ", conflictingIdentifiers.Select(item => item.ToStringRepresentation()))}");
		}

		#endregion
	}
}