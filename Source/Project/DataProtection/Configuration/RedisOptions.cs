using System;
using HansKindberg.IdentityServer.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;

namespace HansKindberg.IdentityServer.DataProtection.Configuration
{
	public class RedisOptions : ExtendedDataProtectionOptions
	{
		#region Properties

		public virtual string Key { get; set; } = "DataProtectionKeys";
		public virtual string Url { get; set; }

		#endregion

		#region Methods

		protected internal override void AddInternal(IDataProtectionBuilder dataProtectionBuilder, IServiceConfiguration serviceConfiguration)
		{
			if(dataProtectionBuilder == null)
				throw new ArgumentNullException(nameof(dataProtectionBuilder));

			dataProtectionBuilder.PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect(this.Url), this.Key);

			base.AddInternal(dataProtectionBuilder, serviceConfiguration);
		}

		#endregion
	}
}