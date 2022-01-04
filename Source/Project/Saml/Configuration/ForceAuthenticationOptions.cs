using System;
using Microsoft.Extensions.Configuration;

namespace HansKindberg.IdentityServer.Saml.Configuration
{
	public class ForceAuthenticationOptions
	{
		#region Properties

		public virtual bool Enabled { get; set; }
		public virtual IConfigurationSection Options { get; set; }
		public virtual Type Router { get; set; }

		#endregion
	}
}