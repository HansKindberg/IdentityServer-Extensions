using System;
using Duende.IdentityServer.Configuration;
using HansKindberg.IdentityServer.Web;

namespace HansKindberg.IdentityServer.Configuration.Extensions
{
	public static class IdentityServerOptionsExtension
	{
		#region Methods

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