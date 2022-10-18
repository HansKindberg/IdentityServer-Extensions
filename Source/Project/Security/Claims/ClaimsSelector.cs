using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Extensions;
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

	/// <inheritdoc />
	public abstract class ClaimsSelector<T> : ClaimsSelector where T : ISelectableClaim
	{
		#region Constructors

		protected ClaimsSelector(ILoggerFactory loggerFactory) : base(loggerFactory) { }

		#endregion

		#region Methods

		public override async Task<IDictionary<string, IClaimBuilderCollection>> GetClaimsAsync(ClaimsPrincipal claimsPrincipal, IClaimsSelectionResult selectionResult)
		{
			if(claimsPrincipal == null)
				throw new ArgumentNullException(nameof(claimsPrincipal));

			if(selectionResult == null)
				throw new ArgumentNullException(nameof(selectionResult));

			if(!ReferenceEquals(this, selectionResult.Selector))
				throw new ArgumentException("The selector-property of the selection-result is not this instance.", nameof(selectionResult));

			IDictionary<string, IClaimBuilderCollection> claimsDictionary = null;

			if(selectionResult.Selectables.TryGetValue(this.Key, out var selectables))
			{
				var selectableClaim = selectables.OfType<T>().FirstOrDefault(selectable => selectable.Selected);

				if(selectableClaim == null && this.SelectionRequired)
					throw new InvalidOperationException($"Selection required but there is no selected selectable of type \"{typeof(T)}\".");

				if(selectableClaim != null)
					claimsDictionary = selectableClaim.Build();
			}
			else if(this.SelectionRequired)
			{
				throw new InvalidOperationException($"There is no selectable with key {this.Key.ToStringRepresentation()}.");
			}

			return await Task.FromResult(claimsDictionary ?? new SortedDictionary<string, IClaimBuilderCollection>(StringComparer.OrdinalIgnoreCase)).ConfigureAwait(false);
		}

		#endregion
	}
}