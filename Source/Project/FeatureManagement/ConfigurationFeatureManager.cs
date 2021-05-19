using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement;
using MoreLinq;

namespace HansKindberg.IdentityServer.FeatureManagement
{
	/// <summary>
	/// Simple feature-manager to be used during dependency-injection-configuration/service-configuration.
	/// </summary>
	public class ConfigurationFeatureManager : IFeatureManager
	{
		#region Constructors

		public ConfigurationFeatureManager(IConfiguration configuration)
		{
			this.Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		#endregion

		#region Properties

		protected internal virtual IConfiguration Configuration { get; }

		#endregion

		#region Methods

		public virtual async IAsyncEnumerable<string> GetFeatureNamesAsync()
		{
			foreach(var name in await this.GetFeatureNamesInternalAsync())
			{
				yield return name;
			}
		}

		protected internal virtual async Task<ISet<string>> GetFeatureNamesInternalAsync()
		{
			return await Task.FromResult(Enum.GetNames(typeof(Feature)).OrderBy(name => name, OrderByDirection.Ascending).ToHashSet(StringComparer.OrdinalIgnoreCase));
		}

		public virtual async Task<bool> IsEnabledAsync(string feature)
		{
			if(feature == null)
				return false;

			var featureNames = await this.GetFeatureNamesInternalAsync();
			if(!featureNames.Contains(feature))
				return false;

			var value = this.Configuration.GetSection($"{nameof(Microsoft.FeatureManagement)}:{feature}").Value;

			if(value == null)
				return false;

			return await Task.FromResult(bool.TryParse(value, out var enabled) && enabled);
		}

		public virtual async Task<bool> IsEnabledAsync<TContext>(string feature, TContext context)
		{
			return await this.IsEnabledAsync(feature);
		}

		#endregion
	}
}