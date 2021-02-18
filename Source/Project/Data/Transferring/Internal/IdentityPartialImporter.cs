using System;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Identity;
using HansKindberg.IdentityServer.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace HansKindberg.IdentityServer.Data.Transferring.Internal
{
	public abstract class IdentityPartialImporter<TModel> : PartialImporter<TModel>
	{
		#region Constructors

		protected IdentityPartialImporter(IIdentityFacade facade, ILoggerFactory loggerFactory) : base(loggerFactory)
		{
			this.Facade = facade ?? throw new ArgumentNullException(nameof(facade));
		}

		#endregion

		#region Properties

		protected internal virtual IdentityContext DatabaseContext => this.Facade.DatabaseContext;
		protected internal virtual IIdentityFacade Facade { get; }

		#endregion

		#region Methods

		protected internal virtual async Task AddErrorAsync(IdentityResult identityResult, IDataImportResult result)
		{
			if(identityResult == null)
				throw new ArgumentNullException(nameof(identityResult));

			if(result == null)
				throw new ArgumentNullException(nameof(result));

			await this.AddErrorAsync(await this.CreateErrorMessageAsync(identityResult), result);
		}

		protected internal virtual async Task<string> CreateErrorMessageAsync(IdentityResult identityResult)
		{
			if(identityResult == null)
				throw new ArgumentNullException(nameof(identityResult));

			return await Task.FromResult(string.Join(" ", identityResult.Errors.Select(identityError => identityError.Description)));
		}

		#endregion
	}
}