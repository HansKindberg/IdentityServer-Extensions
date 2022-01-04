using Rsk.Saml.Configuration;

namespace HansKindberg.IdentityServer.Saml.Configuration
{
	public class ExtendedSamlIdpOptions : SamlIdpOptions
	{
		#region Properties

		public virtual ForceAuthenticationOptions ForceAuthentication { get; set; } = new ForceAuthenticationOptions();

		#endregion
	}
}