using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RegionOrebroLan.Security.Claims;

namespace HansKindberg.IdentityServer.Security.Claims
{
	/// <inheritdoc />
	public abstract class ClaimsSelector : IClaimsSelector
	{
		#region Constructors

		protected ClaimsSelector(ILoggerFactory loggerFactory)
		{
			this.Logger = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))).CreateLogger(this.GetType());
		}

		#endregion

		#region Properties

		public virtual string Key { get; set; } = Guid.NewGuid().ToString("N");
		protected internal virtual ILogger Logger { get; }
		public virtual bool SelectionRequired { get; set; }

		#endregion

		#region Methods

		public abstract Task<IDictionary<string, IClaimBuilderCollection>> GetClaimsAsync(ClaimsPrincipal claimsPrincipal, IClaimsSelectionResult selectionResult);

		public virtual async Task InitializeAsync(IConfiguration optionsConfiguration)
		{
			optionsConfiguration?.Bind(this, binderOptions => { binderOptions.BindNonPublicProperties = true; });

			await Task.CompletedTask.ConfigureAwait(false);
		}

		public abstract Task<IClaimsSelectionResult> SelectAsync(ClaimsPrincipal claimsPrincipal, IDictionary<string, string> selections);

		#endregion
	}
}