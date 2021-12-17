using System;
using HansKindberg.IdentityServer.Application.Models.Views.Consent;
using HansKindberg.IdentityServer.Web;

namespace HansKindberg.IdentityServer.Application.Models.Views.Device.Extensions
{
	public static class ConsentFormExtension
	{
		#region Methods

		public static string UserCode(this ConsentForm consentForm)
		{
			if(consentForm == null)
				throw new ArgumentNullException(nameof(consentForm));

			return consentForm.Dictionary.TryGetValue(QueryStringKeys.UserCode, out var userCode) ? userCode : null;
		}

		public static void UserCode(this ConsentForm consentForm, string userCode)
		{
			if(consentForm == null)
				throw new ArgumentNullException(nameof(consentForm));

			if(userCode == null)
				consentForm.Dictionary.Remove(QueryStringKeys.UserCode);
			else
				consentForm.Dictionary[QueryStringKeys.UserCode] = userCode;
		}

		#endregion
	}
}