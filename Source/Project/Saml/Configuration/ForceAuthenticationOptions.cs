using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HansKindberg.IdentityServer.Saml.Configuration
{
	public class ForceAuthenticationOptions
	{
		#region Properties

		public virtual bool Enabled { get; set; }
		public virtual IConfigurationSection Options { get; set; }
		public virtual Type Router { get; set; }
		public virtual ServiceLifetime RouterLifetime { get; set; } = ServiceLifetime.Transient;

		#endregion
	}
}