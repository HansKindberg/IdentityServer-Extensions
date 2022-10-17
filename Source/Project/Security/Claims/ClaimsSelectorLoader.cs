using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Security.Claims.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RegionOrebroLan.Extensions;

namespace HansKindberg.IdentityServer.Security.Claims
{
	/// <inheritdoc />
	public class ClaimsSelectorLoader : IClaimsSelectorLoader
	{
		#region Fields

		private IDictionary<string, IEnumerable<KeyValuePair<IClaimsSelector, int>>> _claimsSelectorDictionary;

		#endregion

		#region Constructors

		public ClaimsSelectorLoader(IOptionsMonitor<ClaimsSelectionOptions> optionsMonitor, IServiceProvider serviceProvider)
		{
			this.OptionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
			this.ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

			this.OptionsChangeListener = optionsMonitor.OnChange(this.OnOptionsChanged);
		}

		#endregion

		#region Properties

		[SuppressMessage("Usage", "CA2227:Collection properties should be read only")]
		protected internal virtual IDictionary<string, IEnumerable<KeyValuePair<IClaimsSelector, int>>> ClaimsSelectorDictionary
		{
			get
			{
				// ReSharper disable All
				if(this._claimsSelectorDictionary == null)
				{
					lock(this.ClaimsSelectorDictionaryLock)
					{
						if(this._claimsSelectorDictionary == null)
						{
							this._claimsSelectorDictionary = this.CreateClaimsSelectorDictionary();
						}
					}
				}
				// ReSharper restore All

				return this._claimsSelectorDictionary;
			}
			set
			{
				lock(this.ClaimsSelectorDictionaryLock)
				{
					this._claimsSelectorDictionary = value;
				}
			}
		}

		protected internal virtual object ClaimsSelectorDictionaryLock { get; } = new object();
		protected internal virtual IDisposable OptionsChangeListener { get; }
		protected internal virtual IOptionsMonitor<ClaimsSelectionOptions> OptionsMonitor { get; }
		protected internal virtual IServiceProvider ServiceProvider { get; }

		#endregion

		#region Methods

		protected internal virtual IDictionary<string, IEnumerable<KeyValuePair<IClaimsSelector, int>>> CreateClaimsSelectorDictionary()
		{
			var intermediateDictionary = new Dictionary<string, List<KeyValuePair<IClaimsSelector, int>>>();

			var options = this.OptionsMonitor.CurrentValue.Selectors.Values;

			foreach(var option in options)
			{
				if(!option.Enabled)
					continue;

				var claimsSelector = (IClaimsSelector)this.ServiceProvider.GetRequiredService(option.Type);
				claimsSelector.InitializeAsync(option.Options).Wait();

				foreach(var (key, value) in option.AuthenticationSchemes)
				{
					if(!intermediateDictionary.TryGetValue(key, out var list))
					{
						list = new List<KeyValuePair<IClaimsSelector, int>>();
						intermediateDictionary.Add(key, list);
					}

					list.Add(new KeyValuePair<IClaimsSelector, int>(claimsSelector, value));
				}
			}

			var dictionary = new Dictionary<string, IEnumerable<KeyValuePair<IClaimsSelector, int>>>();

			foreach(var (key, value) in intermediateDictionary)
			{
				dictionary.Add(key, value.OrderBy(keyValuePair => keyValuePair.Value).ToArray());
			}

			return dictionary;
		}

		public virtual async Task<IEnumerable<IClaimsSelector>> GetClaimsSelectorsAsync(string authenticationScheme)
		{
			if(authenticationScheme == null)
				throw new ArgumentNullException(nameof(authenticationScheme));

			var claimsSelectors = new List<KeyValuePair<IClaimsSelector, int>>();

			foreach(var key in this.ClaimsSelectorDictionary.Keys)
			{
				if(key == null)
					continue;

				if(authenticationScheme.Like(key))
					claimsSelectors.AddRange(this.ClaimsSelectorDictionary[key]);
			}

			claimsSelectors.Sort((first, second) => first.Value.CompareTo(second.Value));

			return await Task.FromResult(claimsSelectors.Select(keyValuePair => keyValuePair.Key).ToArray());
		}

		protected internal virtual void OnOptionsChanged(ClaimsSelectionOptions options, string name)
		{
			this.ClaimsSelectorDictionary = null;
		}

		#endregion

		#region Other members

		#region Finalizers

		~ClaimsSelectorLoader()
		{
			this.OptionsChangeListener?.Dispose();
		}

		#endregion

		#endregion
	}
}