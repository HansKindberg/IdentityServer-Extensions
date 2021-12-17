using System;
using HansKindberg.IdentityServer.Web;
using Rsk.Saml.Configuration;

namespace HansKindberg.IdentityServer.Saml.Configuration.Extensions
{
	public static class SamlIdpOptionsExtension
	{
		#region Methods

		public static void SetDefaults(this SamlIdpOptions options)
		{
			if(options == null)
				throw new ArgumentNullException(nameof(options));

			options.UseLegacyRsaEncryption = false;
			options.UserInteraction.RequestIdParameter = QueryStringKeys.SamlRequestId;
		}

		#endregion
	}
}