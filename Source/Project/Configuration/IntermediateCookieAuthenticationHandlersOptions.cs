using System;
using Microsoft.AspNetCore.Http;

namespace HansKindberg.IdentityServer.Configuration
{
	public class IntermediateCookieAuthenticationHandlersOptions
	{
		#region Fields

		private const string _certificateName = $"{_namePrefix}certificate";
		private const string _claimsSelectionName = $"{_namePrefix}claimsselection";
		private static readonly TimeSpan _expiration = TimeSpan.FromMinutes(10);
		private const string _namePrefix = "intermediate.";

		#endregion

		#region Properties

		public virtual IntermediateCookieAuthenticationOptions Certificate { get; set; } = new(_expiration, _certificateName, SameSiteMode.None);
		public virtual IntermediateCookieAuthenticationOptions ClaimsSelection { get; set; } = new(_expiration, _claimsSelectionName, SameSiteMode.Strict);

		#endregion
	}
}