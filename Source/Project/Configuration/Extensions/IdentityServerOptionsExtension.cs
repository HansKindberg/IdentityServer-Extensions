using System;
using System.Linq;
using Duende.IdentityServer.Configuration;
using HansKindberg.IdentityServer.Web;
using Microsoft.Extensions.Configuration;

namespace HansKindberg.IdentityServer.Configuration.Extensions
{
	public static class IdentityServerOptionsExtension
	{
		#region Methods

		public static void BindKeyManagementSigningAlgorithms(this IdentityServerOptions options, IConfigurationSection identityServerSection)
		{
			if(options == null)
				throw new ArgumentNullException(nameof(options));

			if(identityServerSection == null)
				throw new ArgumentNullException(nameof(identityServerSection));

			var signingAlgorithms = options.KeyManagement.SigningAlgorithms.ToList();

			foreach(var signingAlgorithmSection in identityServerSection.GetSection(nameof(IdentityServerOptions.KeyManagement)).GetSection(nameof(IdentityServerOptions.KeyManagement.SigningAlgorithms)).GetChildren())
			{
				var name = signingAlgorithmSection.GetSection(nameof(SigningAlgorithmOptions.Name)).Value;

				if(name == null)
					continue;

				var signingAlgorithm = new SigningAlgorithmOptions(name);
				signingAlgorithmSection.Bind(signingAlgorithm);

				signingAlgorithms.Add(signingAlgorithm);
			}

			options.KeyManagement.SigningAlgorithms = signingAlgorithms.ToArray();
		}

		public static void SetDefaults(this IdentityServerOptions options)
		{
			if(options == null)
				throw new ArgumentNullException(nameof(options));

			var events = options.Events;

			events.RaiseErrorEvents =
				events.RaiseFailureEvents =
					events.RaiseInformationEvents =
						events.RaiseSuccessEvents = true;

			var interaction = options.UserInteraction;

			interaction.ConsentReturnUrlParameter =
				interaction.CustomRedirectReturnUrlParameter =
					interaction.LoginReturnUrlParameter =
						QueryStringKeys.ReturnUrl;

			interaction.ConsentUrl = Paths.Consent;
			interaction.DeviceVerificationUrl = Paths.Device;
			interaction.DeviceVerificationUserCodeParameter = QueryStringKeys.UserCode;
			interaction.ErrorIdParameter = QueryStringKeys.ErrorId;
			interaction.ErrorUrl = Paths.Error;
			interaction.LoginUrl = Paths.SignIn;
			interaction.LogoutIdParameter = QueryStringKeys.SignOutId;
			interaction.LogoutUrl = Paths.SignOut;
		}

		#endregion
	}
}